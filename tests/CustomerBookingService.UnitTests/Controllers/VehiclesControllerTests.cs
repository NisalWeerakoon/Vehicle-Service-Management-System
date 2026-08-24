using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using CustomerBookingService.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CustomerBookingService.UnitTests.Controllers;

public class VehiclesControllerTests
{
    // ------------------------------------------------------------
    // Small factory to seed a User (+ optional Customer) quickly.
    // ------------------------------------------------------------
    private static async Task<(int userId, int customerId)> SeedCustomerUserAsync(
        CustomerBookingService.Data.CustomerBookingDbContext db)
    {
        var customer = new Customer
        {
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "0771234567"
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var user = new User
        {
            Email = "jane@example.com",
            PasswordHash = "hashed",
            Role = UserRole.Customer,
            CustomerId = customer.Id
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (user.Id, customer.Id);
    }

    private static VehicleCreateDto ValidCreateDto(string reg = "ABC-1234") => new()
    {
        RegistrationNumber = reg,
        Make = "Toyota",
        Model = "Corolla",
        Year = 2022,
        FuelType = "Petrol"
    };

    // ------------------------------------------------------------
    // CreateMyVehicle
    // ------------------------------------------------------------

    [Fact]
    public async Task CreateMyVehicle_ValidRequest_ReturnsCreatedWithVehicle()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, customerId) = await SeedCustomerUserAsync(db);
        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        var result = await controller.CreateMyVehicle(ValidCreateDto());

        var created = Assert.IsType<CreatedResult>(result.Result);
        var body = Assert.IsType<VehicleResponseDto>(created.Value);
        Assert.Equal("ABC-1234", body.RegistrationNumber);
        Assert.Equal(customerId, body.CustomerId);
    }

    [Fact]
    public async Task CreateMyVehicle_NormalizesRegistrationNumber_TrimAndUpperCase()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, _) = await SeedCustomerUserAsync(db);
        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        var result = await controller.CreateMyVehicle(ValidCreateDto("  abc-1234  "));

        var created = Assert.IsType<CreatedResult>(result.Result);
        var body = Assert.IsType<VehicleResponseDto>(created.Value);
        Assert.Equal("ABC-1234", body.RegistrationNumber);
    }

    [Fact]
    public async Task CreateMyVehicle_DuplicateRegistrationNumber_IsCaseInsensitive_ReturnsBadRequest()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, customerId) = await SeedCustomerUserAsync(db);

        db.Vehicles.Add(new Vehicle
        {
            CustomerId = customerId,
            RegistrationNumber = "ABC-1234",
            Make = "Honda",
            Model = "Civic",
            Year = 2020,
            FuelType = "Petrol"
        });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        // Different case, should still be treated as a duplicate.
        var result = await controller.CreateMyVehicle(ValidCreateDto("abc-1234"));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Theory]
    [InlineData(1899)]      // below minimum
    [InlineData(1800)]      // well below minimum
    public async Task CreateMyVehicle_YearTooOld_ReturnsBadRequest(int year)
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, _) = await SeedCustomerUserAsync(db);
        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        var dto = ValidCreateDto();
        dto.Year = year;

        var result = await controller.CreateMyVehicle(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateMyVehicle_YearTooFarInFuture_ReturnsBadRequest()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, _) = await SeedCustomerUserAsync(db);
        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        var dto = ValidCreateDto();
        dto.Year = DateTime.UtcNow.Year + 2; // controller allows up to currentYear + 1

        var result = await controller.CreateMyVehicle(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateMyVehicle_UserHasNoCustomerProfile_ReturnsBadRequest()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();

        var user = new User
        {
            Email = "no-profile@example.com",
            PasswordHash = "hashed",
            Role = UserRole.Customer,
            CustomerId = null
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateVehiclesController(db, user.Id);

        var result = await controller.CreateMyVehicle(ValidCreateDto());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateMyVehicle_UserDoesNotExist_ReturnsUnauthorized()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        // No users seeded at all - simulate a token for a deleted/unknown user.
        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId: 9999);

        var result = await controller.CreateMyVehicle(ValidCreateDto());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    // ------------------------------------------------------------
    // GetMyVehicles / GetMyVehicleById
    // ------------------------------------------------------------

    [Fact]
    public async Task GetMyVehicles_OnlyReturnsVehiclesBelongingToCaller()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, customerId) = await SeedCustomerUserAsync(db);

        db.Vehicles.Add(new Vehicle { CustomerId = customerId, RegistrationNumber = "MINE-1", Make = "Toyota", Model = "Aqua", Year = 2019, FuelType = "Hybrid" });
        db.Vehicles.Add(new Vehicle { CustomerId = customerId + 999, RegistrationNumber = "OTHER-1", Make = "Nissan", Model = "Leaf", Year = 2021, FuelType = "Electric" });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        var result = await controller.GetMyVehicles();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var vehicles = Assert.IsAssignableFrom<IEnumerable<VehicleResponseDto>>(ok.Value);
        Assert.Single(vehicles);
        Assert.Equal("MINE-1", vehicles.First().RegistrationNumber);
    }

    [Fact]
    public async Task GetMyVehicleById_VehicleBelongsToSomeoneElse_ReturnsNotFound()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, _) = await SeedCustomerUserAsync(db);

        var othersVehicle = new Vehicle
        {
            CustomerId = 555,
            RegistrationNumber = "NOT-MINE",
            Make = "Ford",
            Model = "Focus",
            Year = 2018,
            FuelType = "Diesel"
        };
        db.Vehicles.Add(othersVehicle);
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        var result = await controller.GetMyVehicleById(othersVehicle.Id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // ------------------------------------------------------------
    // UpdateMyVehicle
    // ------------------------------------------------------------

    [Fact]
    public async Task UpdateMyVehicle_ValidRequest_UpdatesFieldsAndTimestamp()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, customerId) = await SeedCustomerUserAsync(db);

        var vehicle = new Vehicle
        {
            CustomerId = customerId,
            RegistrationNumber = "UPD-0001",
            Make = "Toyota",
            Model = "Corolla",
            Year = 2015,
            FuelType = "Petrol"
        };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        var update = new VehicleUpdateDto
        {
            Make = "Toyota",
            Model = "Corolla Altis",
            Year = 2016,
            FuelType = "Hybrid"
        };

        var result = await controller.UpdateMyVehicle(vehicle.Id, update);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<VehicleResponseDto>(ok.Value);
        Assert.Equal("Corolla Altis", body.Model);
        Assert.Equal("Hybrid", body.FuelType);
        Assert.NotNull(body.UpdatedAt);
    }

    [Fact]
    public async Task UpdateMyVehicle_VehicleDoesNotExist_ReturnsNotFound()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, _) = await SeedCustomerUserAsync(db);
        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        var update = new VehicleUpdateDto
        {
            Make = "Toyota",
            Model = "Corolla",
            Year = 2020,
            FuelType = "Petrol"
        };

        var result = await controller.UpdateMyVehicle(id: 12345, update);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateMyVehicle_InvalidYear_ReturnsBadRequest()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var (userId, customerId) = await SeedCustomerUserAsync(db);

        var vehicle = new Vehicle
        {
            CustomerId = customerId,
            RegistrationNumber = "UPD-0002",
            Make = "Toyota",
            Model = "Corolla",
            Year = 2015,
            FuelType = "Petrol"
        };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId);

        var update = new VehicleUpdateDto
        {
            Make = "Toyota",
            Model = "Corolla",
            Year = 1500,
            FuelType = "Petrol"
        };

        var result = await controller.UpdateMyVehicle(vehicle.Id, update);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ------------------------------------------------------------
    // Staff endpoints
    // ------------------------------------------------------------

    [Fact]
    public async Task CreateVehicleForCustomer_CustomerDoesNotExist_ReturnsNotFound()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();
        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId: 1, role: "ServiceAdvisor");

        var result = await controller.CreateVehicleForCustomer(customerId: 777, ValidCreateDto());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetVehiclesForCustomer_ReturnsAllVehiclesForThatCustomer()
    {
        var db = ControllerTestHelpers.BuildInMemoryContext();

        var customer = new Customer { FullName = "Staff Target", Email = "target@example.com", Phone = "0770000000" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        db.Vehicles.Add(new Vehicle { CustomerId = customer.Id, RegistrationNumber = "S-0001", Make = "Toyota", Model = "Hilux", Year = 2021, FuelType = "Diesel" });
        db.Vehicles.Add(new Vehicle { CustomerId = customer.Id, RegistrationNumber = "S-0002", Make = "Toyota", Model = "Vitz", Year = 2019, FuelType = "Petrol" });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateVehiclesController(db, userId: 1, role: "Administrator");

        var result = await controller.GetVehiclesForCustomer(customer.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var vehicles = Assert.IsAssignableFrom<IEnumerable<VehicleResponseDto>>(ok.Value);
        Assert.Equal(2, vehicles.Count());
    }
}
