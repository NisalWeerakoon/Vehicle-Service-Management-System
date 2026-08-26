using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using VSMS.UiTests.Helpers;

namespace VSMS.UiTests.Pages;

/// <summary>
/// Page Object for /vehicles/add (frontend/src/pages/AddVehiclePage.jsx).
/// A vehicle must exist before a booking can be created for it.
/// </summary>
public class AddVehiclePage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public AddVehiclePage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public void NavigateTo() => _driver.Navigate().GoToUrl($"{DriverFactory.BaseUrl}/vehicles/add");

    public void AddVehicle(
        string registrationNumber,
        string make,
        string model,
        string year,
        string fuelType)
    {
        _wait.Until(d => d.FindElement(By.Name("registrationNumber")));

        _driver.FindElement(By.Name("registrationNumber")).SendKeys(registrationNumber);
        _driver.FindElement(By.Name("make")).SendKeys(make);
        _driver.FindElement(By.Name("model")).SendKeys(model);

        var yearInput = _driver.FindElement(By.Name("year"));
        yearInput.Clear();
        yearInput.SendKeys(year);

        new SelectElement(_driver.FindElement(By.Name("fuelType"))).SelectByValue(fuelType);

        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
    }
}
