using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using VSMS.UiTests.Helpers;

namespace VSMS.UiTests.Pages;

/// <summary>
/// Page Object for /bookings/create (frontend/src/pages/CreateBookingPage.jsx).
/// Field ids/names copied verbatim from the JSX: vehicleId (select),
/// preferredDate (date input), requestedServiceOrProblem (textarea).
/// </summary>
public class CreateBookingPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public CreateBookingPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    private IWebElement VehicleSelect => _driver.FindElement(By.Id("vehicleId"));
    private IWebElement DateInput => _driver.FindElement(By.Id("preferredDate"));
    private IWebElement ProblemTextarea => _driver.FindElement(By.Id("requestedServiceOrProblem"));
    private IWebElement SubmitButton => _driver.FindElement(By.CssSelector("form button[type='submit']"));

    public void NavigateTo() => _driver.Navigate().GoToUrl($"{DriverFactory.BaseUrl}/bookings/create");

    public bool HasNoVehicleEmptyState()
    {
        _wait.Until(d =>
            d.FindElements(By.CssSelector(".empty-state")).Count > 0 ||
            d.FindElements(By.Id("vehicleId")).Count > 0);

        return _driver.FindElements(By.CssSelector(".empty-state")).Count > 0;
    }

    /// <param name="vehicleIndex">
    /// Which &lt;option&gt; to pick, 1-based, skipping the "Choose your vehicle" placeholder.
    /// </param>
    public void CreateBooking(int vehicleIndex, string isoDate, string problemDescription)
    {
        _wait.Until(d => d.FindElement(By.Id("vehicleId")));
        _wait.Until(d => new SelectElement(d.FindElement(By.Id("vehicleId"))).Options.Count > vehicleIndex);

        var js = (IJavaScriptExecutor)_driver;

        // Set select value via React native property setter
        js.ExecuteScript(@"
            var select = arguments[0];
            var index = arguments[1];
            var val = select.options[index].value;
            var nativeSetter = Object.getOwnPropertyDescriptor(window.HTMLSelectElement.prototype, 'value').set;
            nativeSetter.call(select, val);
            select.dispatchEvent(new Event('change', { bubbles: true }));
        ", VehicleSelect, vehicleIndex);

        // Set date input value via React native property setter
        js.ExecuteScript(@"
            var input = arguments[0];
            var val = arguments[1];
            var nativeSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;
            nativeSetter.call(input, val);
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.dispatchEvent(new Event('change', { bubbles: true }));
        ", DateInput, isoDate);

        ProblemTextarea.Clear();
        ProblemTextarea.SendKeys(problemDescription);

        SubmitButton.Click();
    }

    public string? GetErrorMessage() =>
        _driver.FindElements(By.CssSelector(".error-alert")).FirstOrDefault()?.Text;
}
