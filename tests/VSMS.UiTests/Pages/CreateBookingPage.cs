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

    public bool HasNoVehicleEmptyState() =>
        _driver.FindElements(By.CssSelector(".empty-state")).Any();

    /// <param name="vehicleIndex">
    /// Which &lt;option&gt; to pick, 1-based, skipping the "Choose your vehicle" placeholder.
    /// </param>
    public void CreateBooking(int vehicleIndex, string isoDate, string problemDescription)
    {
        _wait.Until(d => d.FindElement(By.Id("vehicleId")));

        new SelectElement(VehicleSelect).SelectByIndex(vehicleIndex);
        DateInput.Clear();
        DateInput.SendKeys(isoDate); // yyyy-MM-dd, matches the <input type="date">
        ProblemTextarea.Clear();
        ProblemTextarea.SendKeys(problemDescription);

        SubmitButton.Click();
    }

    public string? GetErrorMessage() =>
        _driver.FindElements(By.CssSelector(".error-alert")).FirstOrDefault()?.Text;
}
