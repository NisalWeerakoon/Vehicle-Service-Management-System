using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CustomerBookingService.SeleniumTests;

/// <summary>
/// Base class every Selenium test class inherits from. Spins up a fresh
/// Chrome browser per test class, points it at the running frontend, and
/// gives subclasses a WebDriverWait for reliable waits (React renders
/// async, so Thread.Sleep is never the right tool here).
///
/// PREREQUISITES before running any test in this project:
///   1. Backend running:  cd services/CustomerBookingService && dotnet run
///   2. Frontend running: cd frontend && npm run dev
///   3. Chrome installed locally (Selenium Manager downloads the driver
///      automatically the first time you run a test).
/// </summary>
public abstract class SeleniumTestBase : IDisposable
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    /// <summary>Override via the FRONTEND_BASE_URL env var if your Vite dev server uses a different port.</summary>
    protected static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "http://localhost:5173";

    protected SeleniumTestBase()
    {
        var options = new ChromeOptions();

        // Run headless in CI; set HEADED=1 locally to watch the browser.
        if (Environment.GetEnvironmentVariable("HEADED") != "1")
        {
            options.AddArgument("--headless=new");
        }
        options.AddArgument("--window-size=1280,800");

        Driver = new ChromeDriver(options);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
    }

    protected void GoTo(string relativePath) => Driver.Navigate().GoToUrl($"{BaseUrl}{relativePath}");

    protected IWebElement WaitForElement(By by) =>
        Wait.Until(driver => driver.FindElement(by));

    protected void WaitForUrlToContain(string fragment) =>
        Wait.Until(driver => driver.Url.Contains(fragment));

    /// <summary>Logs in through the real UI and waits for the redirect to /profile.</summary>
    protected void LoginAs(string email, string password)
    {
        GoTo("/login");
        WaitForElement(By.Id("email")).SendKeys(email);
        Driver.FindElement(By.Id("password")).SendKeys(password);
        Driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        WaitForUrlToContain("/profile");
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
    }
}
