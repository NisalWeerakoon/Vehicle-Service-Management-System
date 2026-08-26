using OpenQA.Selenium;
using VSMS.UiTests.Helpers;
using VSMS.UiTests.Pages;
using Xunit;

namespace VSMS.UiTests.Tests;

/// <summary>
/// Full end-to-end UI tests against the running React app + CustomerBookingService API.
///
/// Prerequisites before running this class:
///   1. Backend running:  cd services/CustomerBookingService && dotnet run
///   2. Frontend running: cd frontend && npm run dev   (defaults to http://localhost:5173)
///   3. MySQL reachable with the connection string configured in appsettings/user-secrets.
///
/// Every test registers its own unique customer (via the UI) so tests do not
/// depend on each other or on pre-seeded data.
/// </summary>
public class ServiceBookingFlowTests : IDisposable
{
    private readonly IWebDriver _driver;

    public ServiceBookingFlowTests()
    {
        _driver = DriverFactory.CreateChromeDriver();
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [Fact]
    public void Customer_Can_Register_AddVehicle_CreateBooking_AndSeeItInList()
    {
        var email = $"qa.{Guid.NewGuid():N}@test.com";
        const string password = "Password123!";

        // 1. Register a new customer account
        var registerPage = new RegisterPage(_driver);
        registerPage.NavigateTo();
        registerPage.Register(
            fullName: "QA Test Customer",
            email: email,
            phone: "0771234567",
            password: password,
            confirmPassword: password);

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.Contains("/profile"));

        // 2. Register expects a vehicle to exist before a booking can be made
        var addVehiclePage = new AddVehiclePage(_driver);
        addVehiclePage.NavigateTo();
        addVehiclePage.AddVehicle(
            registrationNumber: $"QA-{DateTime.UtcNow.Ticks % 10000}",
            make: "Toyota",
            model: "Aqua",
            year: "2021",
            fuelType: "Hybrid");

        wait.Until(d => d.Url.Contains("/vehicles"));

        // 3. Create a service booking
        var createBookingPage = new CreateBookingPage(_driver);
        createBookingPage.NavigateTo();

        var preferredDate = DateTime.UtcNow.AddDays(5).ToString("yyyy-MM-dd");
        const string problemText = "Annual full service - automated UI test";

        createBookingPage.CreateBooking(
            vehicleIndex: 1,
            isoDate: preferredDate,
            problemDescription: problemText);

        wait.Until(d => d.Url.Contains("/bookings/") && !d.Url.EndsWith("/bookings/create"));

        // 4. Confirm it shows up in "My Bookings" as Pending
        var bookingsPage = new BookingsPage(_driver);
        bookingsPage.NavigateTo();
        bookingsPage.WaitForBookingsToLoad();

        Assert.True(bookingsPage.BookingCount >= 1);
        Assert.Equal(problemText, bookingsPage.GetFirstBookingProblemText());
        Assert.Contains("Pending", bookingsPage.GetFirstBookingStatus());
    }

    [Fact]
    public void CreateBooking_ShowsNoVehicleEmptyState_WhenCustomerHasNoVehicles()
    {
        var email = $"qa.{Guid.NewGuid():N}@test.com";
        const string password = "Password123!";

        var registerPage = new RegisterPage(_driver);
        registerPage.NavigateTo();
        registerPage.Register(
            fullName: "QA No Vehicle Customer",
            email: email,
            phone: "0779876543",
            password: password,
            confirmPassword: password);

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.Contains("/profile"));

        var createBookingPage = new CreateBookingPage(_driver);
        createBookingPage.NavigateTo();

        Assert.True(createBookingPage.HasNoVehicleEmptyState());
    }

    [Fact]
    public void Login_WithInvalidCredentials_ShowsErrorMessage()
    {
        var loginPage = new LoginPage(_driver);
        loginPage.NavigateTo();
        loginPage.Login("nonexistent.user@test.com", "WrongPassword1!");

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => loginPage.GetErrorMessage() != null);

        Assert.False(string.IsNullOrWhiteSpace(loginPage.GetErrorMessage()));
        // Should stay on the login page, not be redirected
        Assert.Contains("/login", _driver.Url);
    }
}
