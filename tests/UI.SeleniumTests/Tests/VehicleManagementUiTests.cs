using UI.SeleniumTests.Pages;
using UI.SeleniumTests.TestHelpers;
using Xunit;

namespace UI.SeleniumTests.Tests;

/// <summary>
/// End-to-end UI tests against the real running frontend + backend.
/// Prerequisites before running (see README):
///   1. Backend running (dotnet run --project services/CustomerBookingService)
///   2. Frontend running (npm run dev, default http://localhost:5173)
/// Each test registers its own throwaway customer account so tests
/// don't collide or depend on shared seed data.
/// </summary>
public class VehicleManagementUiTests : IClassFixture<ChromeDriverFixture>
{
    private readonly ChromeDriverFixture _fixture;

    public VehicleManagementUiTests(ChromeDriverFixture fixture)
    {
        _fixture = fixture;
    }

    // Registers a brand-new customer and lands on /profile, ready to test vehicles.
    private void RegisterFreshCustomer()
    {
        var email = $"qa.selenium.{Guid.NewGuid():N}@example.com";

        new RegisterPage(_fixture.Driver)
            .Open()
            .Register(
                fullName: "QA Selenium Tester",
                email: email,
                phone: "0771234567",
                address: "123 Test Street, Colombo",
                password: "P@ssword123",
                confirmPassword: "P@ssword123");

        new RegisterPage(_fixture.Driver).WaitForRegistrationSuccess();
    }

    [Fact]
    public void Customer_Can_Register_A_New_Vehicle()
    {
        RegisterFreshCustomer();

        var registration = $"QA-{DateTime.Now:HHmmss}";

        var addPage = new AddVehiclePage(_fixture.Driver).Open();
        addPage.FillForm(registration, "Toyota", "Corolla", "2022", "Petrol");
        addPage.Submit();
        addPage.WaitForSaveSuccess();

        var vehiclesPage = new VehiclesPage(_fixture.Driver).Open();
        Assert.True(vehiclesPage.HasVehicleWithRegistration(registration));
    }

    [Fact]
    public void Customer_Cannot_Register_Vehicle_With_Duplicate_RegistrationNumber()
    {
        RegisterFreshCustomer();
        var registration = $"DUP-{DateTime.Now:HHmmss}";

        // First registration succeeds.
        var addPage = new AddVehiclePage(_fixture.Driver).Open();
        addPage.FillForm(registration, "Honda", "Civic", "2021", "Petrol");
        addPage.Submit();
        addPage.WaitForSaveSuccess();

        // Second attempt with same plate should be rejected.
        addPage = new AddVehiclePage(_fixture.Driver).Open();
        addPage.FillForm(registration, "Honda", "Civic", "2021", "Petrol");
        addPage.Submit();

        Assert.True(addPage.HasErrorMessage());
    }

    [Theory]
    [InlineData("1800")]                                   // below HTML min="1900"
    public void AddVehicle_YearBelowMinimum_BrowserBlocksSubmit(string invalidYear)
    {
        RegisterFreshCustomer();

        var addPage = new AddVehiclePage(_fixture.Driver).Open();
        addPage.FillForm("YR-0001", "Toyota", "Corolla", invalidYear, "Petrol");
        addPage.Submit();

        // The <input type="number" min="1900"> triggers native HTML5
        // validation, so the form should NOT navigate away.
        Assert.Contains("/vehicles/add", _fixture.Driver.Url);
    }

    [Fact]
    public void AddVehicle_Cancel_ReturnsToGarageWithoutSaving()
    {
        RegisterFreshCustomer();

        var addPage = new AddVehiclePage(_fixture.Driver).Open();
        addPage.FillForm("CANCEL-01", "Nissan", "Leaf", "2020", "Electric");
        addPage.Cancel();

        var vehiclesPage = new VehiclesPage(_fixture.Driver).Open();
        Assert.False(vehiclesPage.HasVehicleWithRegistration("CANCEL-01"));
    }

    [Fact]
    public void Customer_Can_Edit_An_Existing_Vehicle()
    {
        RegisterFreshCustomer();
        var registration = $"EDIT-{DateTime.Now:HHmmss}";

        var addPage = new AddVehiclePage(_fixture.Driver).Open();
        addPage.FillForm(registration, "Toyota", "Corolla", "2019", "Petrol");
        addPage.Submit();
        addPage.WaitForSaveSuccess();

        var vehiclesPage = new VehiclesPage(_fixture.Driver).Open();
        vehiclesPage.ClickEditForVehicle(registration);

        var editPage = new EditVehiclePage(_fixture.Driver).WaitForLoaded();
        editPage.UpdateModel("Corolla Altis");
        editPage.UpdateFuelType("Hybrid");
        editPage.Save();
        editPage.WaitForSaveSuccess();

        var updatedVehiclesPage = new VehiclesPage(_fixture.Driver).Open();
        Assert.True(updatedVehiclesPage.HasVehicleWithRegistration(registration));
    }

    [Fact]
    public void Unauthenticated_User_Is_Redirected_To_Login_From_Vehicles_Page()
    {
        // No login performed - simulate a stranger hitting a protected route directly.
        _fixture.Driver.Manage().Cookies.DeleteAllCookies();
        ((OpenQA.Selenium.IJavaScriptExecutor)_fixture.Driver)
            .ExecuteScript("window.localStorage.clear();");

        _fixture.Driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/vehicles");

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_fixture.Driver, TestConfig.DefaultTimeout);
        wait.Until(d => d.Url.Contains("/login"));

        Assert.Contains("/login", _fixture.Driver.Url);
    }

    [Fact]
    public void Login_With_Invalid_Credentials_Shows_Error()
    {
        var loginPage = new LoginPage(_fixture.Driver).Open();
        loginPage.Login("nonexistent.user@example.com", "WrongPassword1");

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_fixture.Driver, TestConfig.DefaultTimeout);
        wait.Until(d => loginPage.HasErrorMessage());

        Assert.True(loginPage.HasErrorMessage());
    }
}
