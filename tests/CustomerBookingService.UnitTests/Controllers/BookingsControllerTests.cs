using CustomerBookingService.DTOs;
using CustomerBookingService.Models;
using CustomerBookingService.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CustomerBookingService.UnitTests.Controllers;

public class BookingsControllerTests
{
    // ======================================================
    // POST /api/bookings/me  (Customer creates own booking)
    // ======================================================

    [Fact]
    public async Task CreateMyBooking_Returns401_WhenUserDoesNotExist()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        var controller = ControllerTestHelpers.CreateController(db, userId: 999);

        var result = await controller.CreateMyBooking(new BookingCreateDto
        {
            VehicleId = 1,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Oil change"
        });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateMyBooking_Returns400_WhenUserHasNoCustomerProfile()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        db.Users.Add(new User { Id = 1, Email = "a@test.com", PasswordHash = "x", CustomerId = null });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateController(db, userId: 1);

        var result = await controller.CreateMyBooking(new BookingCreateDto
        {
            VehicleId = 1,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Oil change"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateMyBooking_Returns400_WhenPreferredDateIsInThePast()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        db.Users.Add(new User { Id = 1, Email = "a@test.com", PasswordHash = "x", CustomerId = 10 });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateController(db, userId: 1);

        var result = await controller.CreateMyBooking(new BookingCreateDto
        {
            VehicleId = 1,
            PreferredDate = DateTime.UtcNow.AddDays(-1),
            RequestedServiceOrProblem = "Oil change"
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("past", badRequest.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateMyBooking_Returns400_WhenVehicleDoesNotBelongToCustomer()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        db.Users.Add(new User { Id = 1, Email = "a@test.com", PasswordHash = "x", CustomerId = 10 });
        db.Vehicles.Add(new Vehicle
        {
            Id = 5,
            CustomerId = 99, // belongs to a different customer
            RegistrationNumber = "ABC-123",
            Make = "Toyota",
            Model = "Aqua",
            FuelType = "Hybrid"
        });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateController(db, userId: 1);

        var result = await controller.CreateMyBooking(new BookingCreateDto
        {
            VehicleId = 5,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Brake check"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateMyBooking_Returns201_AndPendingStatus_OnValidRequest()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        db.Users.Add(new User { Id = 1, Email = "a@test.com", PasswordHash = "x", CustomerId = 10 });
        db.Vehicles.Add(new Vehicle
        {
            Id = 5,
            CustomerId = 10,
            RegistrationNumber = "ABC-123",
            Make = "Toyota",
            Model = "Aqua",
            FuelType = "Hybrid"
        });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateController(db, userId: 1);

        var result = await controller.CreateMyBooking(new BookingCreateDto
        {
            VehicleId = 5,
            PreferredDate = DateTime.UtcNow.AddDays(2),
            RequestedServiceOrProblem = "Full service"
        });

        var created = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<BookingResponseDto>(created.Value);

        Assert.Equal("Pending", dto.Status);
        Assert.StartsWith("BKG-", dto.BookingReference);
        Assert.Equal("Toyota Aqua", dto.VehicleName);
    }

    [Fact]
    public async Task CreateMyBooking_TrimsWhitespace_FromRequestedServiceOrProblem()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        db.Users.Add(new User { Id = 1, Email = "a@test.com", PasswordHash = "x", CustomerId = 10 });
        db.Vehicles.Add(new Vehicle
        {
            Id = 5,
            CustomerId = 10,
            RegistrationNumber = "ABC-123",
            Make = "Toyota",
            Model = "Aqua",
            FuelType = "Hybrid"
        });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateController(db, userId: 1);

        var result = await controller.CreateMyBooking(new BookingCreateDto
        {
            VehicleId = 5,
            PreferredDate = DateTime.UtcNow.AddDays(2),
            RequestedServiceOrProblem = "   Brake pads worn   "
        });

        var created = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<BookingResponseDto>(created.Value);

        Assert.Equal("Brake pads worn", dto.RequestedServiceOrProblem);
    }

    // ======================================================
    // GET /api/bookings/me  (Customer views own bookings)
    // ======================================================

    [Fact]
    public async Task GetMyBookings_OnlyReturnsBookingsForCurrentCustomer_NewestFirst()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        db.Users.Add(new User { Id = 1, Email = "a@test.com", PasswordHash = "x", CustomerId = 10 });
        db.Vehicles.Add(new Vehicle { Id = 1, CustomerId = 10, RegistrationNumber = "AAA-111", Make = "Honda", Model = "Fit", FuelType = "Petrol" });
        db.Vehicles.Add(new Vehicle { Id = 2, CustomerId = 20, RegistrationNumber = "BBB-222", Make = "Nissan", Model = "Leaf", FuelType = "Electric" });

        db.Bookings.Add(new Booking
        {
            BookingReference = "BKG-1", CustomerId = 10, VehicleId = 1,
            PreferredDate = DateTime.UtcNow.AddDays(1), RequestedServiceOrProblem = "Old one",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        });
        db.Bookings.Add(new Booking
        {
            BookingReference = "BKG-2", CustomerId = 10, VehicleId = 1,
            PreferredDate = DateTime.UtcNow.AddDays(2), RequestedServiceOrProblem = "Newest one",
            CreatedAt = DateTime.UtcNow
        });
        db.Bookings.Add(new Booking
        {
            BookingReference = "BKG-3", CustomerId = 20, VehicleId = 2, // different customer
            PreferredDate = DateTime.UtcNow.AddDays(1), RequestedServiceOrProblem = "Not mine",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateController(db, userId: 1);

        var result = await controller.GetMyBookings();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var bookings = Assert.IsAssignableFrom<IEnumerable<BookingResponseDto>>(ok.Value).ToList();

        Assert.Equal(2, bookings.Count);
        Assert.Equal("Newest one", bookings.First().RequestedServiceOrProblem);
    }

    // ======================================================
    // GET /api/bookings/me/{id}  (Customer views one own booking)
    // ======================================================

    [Fact]
    public async Task GetMyBookingById_Returns404_WhenBookingBelongsToDifferentCustomer()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        db.Users.Add(new User { Id = 1, Email = "a@test.com", PasswordHash = "x", CustomerId = 10 });
        db.Vehicles.Add(new Vehicle { Id = 2, CustomerId = 20, RegistrationNumber = "BBB-222", Make = "Nissan", Model = "Leaf", FuelType = "Electric" });
        db.Bookings.Add(new Booking
        {
            Id = 1, BookingReference = "BKG-3", CustomerId = 20, VehicleId = 2,
            PreferredDate = DateTime.UtcNow.AddDays(1), RequestedServiceOrProblem = "Not mine"
        });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateController(db, userId: 1);

        var result = await controller.GetMyBookingById(1);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // ======================================================
    // POST /api/bookings/staff  (Staff creates booking for customer)
    // ======================================================

    [Fact]
    public async Task CreateBookingByStaff_Returns404_WhenCustomerDoesNotExist()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        var controller = ControllerTestHelpers.CreateController(db, userId: 1, role: "ServiceAdvisor");

        var result = await controller.CreateBookingByStaff(new StaffBookingCreateDto
        {
            CustomerId = 999,
            VehicleId = 1,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Check engine light"
        });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateBookingByStaff_Returns400_WhenVehicleDoesNotBelongToCustomer()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        db.Customers.Add(new Customer { Id = 10, FullName = "Jane", Email = "jane@test.com", Phone = "0771234567" });
        db.Vehicles.Add(new Vehicle { Id = 5, CustomerId = 99, RegistrationNumber = "XYZ-999", Make = "Kia", Model = "Rio", FuelType = "Petrol" });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateController(db, userId: 1, role: "ServiceAdvisor");

        var result = await controller.CreateBookingByStaff(new StaffBookingCreateDto
        {
            CustomerId = 10,
            VehicleId = 5,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Check engine light"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateBookingByStaff_Returns201_OnValidRequest()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        db.Customers.Add(new Customer { Id = 10, FullName = "Jane", Email = "jane@test.com", Phone = "0771234567" });
        db.Vehicles.Add(new Vehicle { Id = 5, CustomerId = 10, RegistrationNumber = "XYZ-999", Make = "Kia", Model = "Rio", FuelType = "Petrol" });
        await db.SaveChangesAsync();

        var controller = ControllerTestHelpers.CreateController(db, userId: 1, role: "ServiceAdvisor");

        var result = await controller.CreateBookingByStaff(new StaffBookingCreateDto
        {
            CustomerId = 10,
            VehicleId = 5,
            PreferredDate = DateTime.UtcNow.AddDays(1),
            RequestedServiceOrProblem = "Check engine light"
        });

        var created = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<BookingResponseDto>(created.Value);
        Assert.Equal("Pending", dto.Status);
    }

    // ======================================================
    // GET /api/bookings/{id}  (Staff views any booking)
    // ======================================================

    [Fact]
    public async Task GetBookingById_Returns404_WhenBookingDoesNotExist()
    {
        using var db = DbContextFactory.CreateInMemoryDb();
        var controller = ControllerTestHelpers.CreateController(db, userId: 1, role: "Administrator");

        var result = await controller.GetBookingById(123);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
