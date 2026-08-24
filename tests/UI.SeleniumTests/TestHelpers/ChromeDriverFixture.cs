using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace UI.SeleniumTests.TestHelpers;

/// <summary>
/// Creates one ChromeDriver instance shared across all tests in a test
/// class (xUnit re-uses the fixture per class, not per test), and quits
/// it once the class is done. Set QA_HEADLESS=false to watch it run.
/// Selenium 4's built-in Selenium Manager automatically detects the installed
/// Chrome version (e.g. 151) and manages matching ChromeDriver automatically.
/// </summary>
public class ChromeDriverFixture : IDisposable
{
    public IWebDriver Driver { get; }

    public ChromeDriverFixture()
    {
        var options = new ChromeOptions();

        var headless = Environment.GetEnvironmentVariable("QA_HEADLESS") != "false";
        if (headless)
        {
            options.AddArgument("--headless=new");
        }

        options.AddArgument("--window-size=1400,1000");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero; // we use explicit waits instead
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
    }
}
