using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using UI.SeleniumTests.TestHelpers;

namespace UI.SeleniumTests.Pages;

public class EditVehiclePage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private By MakeInput => By.Id("make");
    private By ModelInput => By.Id("model");
    private By YearInput => By.Id("year");
    private By FuelTypeSelect => By.Id("fuelType");
    private By SaveButton => By.CssSelector("button[type='submit']");
    private By ErrorAlert => By.CssSelector(".error-alert");

    public EditVehiclePage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TestConfig.DefaultTimeout);
    }

    // Waits for the async vehicle fetch to finish and the form to render.
    public EditVehiclePage WaitForLoaded()
    {
        _wait.Until(d => d.FindElements(ModelInput).Count > 0);
        return this;
    }

    public void UpdateModel(string model)
    {
        var input = _driver.FindElement(ModelInput);
        input.Clear();
        input.SendKeys(model);
    }

    public void UpdateFuelType(string fuelType)
    {
        new SelectElement(_driver.FindElement(FuelTypeSelect)).SelectByValue(fuelType);
    }

    public void Save() => _driver.FindElement(SaveButton).Click();

    public bool HasErrorMessage() => _driver.FindElements(ErrorAlert).Count > 0;

    public string GetErrorMessage() => _driver.FindElement(ErrorAlert).Text;

    public void WaitForSaveSuccess()
    {
        _wait.Until(d =>
        {
            if (HasErrorMessage())
            {
                throw new InvalidOperationException($"Edit vehicle failed: {GetErrorMessage()}");
            }

            return d.Url.TrimEnd('/').EndsWith("/vehicles");
        });
    }
}
