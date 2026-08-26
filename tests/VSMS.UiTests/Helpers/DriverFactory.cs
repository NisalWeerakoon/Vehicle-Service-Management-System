using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace VSMS.UiTests.Helpers;

public static class DriverFactory
{
    /// <summary>
    /// Base URL of the running React dev server (npm run dev in /frontend).
    /// Override with the VSMS_UI_BASE_URL environment variable for CI/staging.
    /// </summary>
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("VSMS_UI_BASE_URL") ?? "http://localhost:5173";

    public static IWebDriver CreateChromeDriver()
    {
        var options = new ChromeOptions();

        // Set VSMS_UI_HEADLESS=false locally if you want to watch the browser run.
        var headless = Environment.GetEnvironmentVariable("VSMS_UI_HEADLESS") != "false";

        if (headless)
        {
            options.AddArgument("--headless=new");
        }

        options.AddArgument("--window-size=1400,1000");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        return new ChromeDriver(options);
    }
}
