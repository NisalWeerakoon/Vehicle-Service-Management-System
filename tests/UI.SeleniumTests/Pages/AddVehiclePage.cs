using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using UI.SeleniumTests.TestHelpers;

namespace UI.SeleniumTests.Pages;

public class AddVehiclePage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private By RegistrationNumberInput => By.Id("registrationNumber");
    private By MakeInput => By.Id("make");
    private By ModelInput => By.Id("model");
    private By YearInput => By.Id("year");
    private By FuelTypeSelect => By.Id("fuelType");
    private By CancelButton => By.XPath("//button[normalize-space()='Cancel']");
    private By SubmitButton => By.CssSelector("button[type='submit']");
    private By ErrorAlert => By.CssSelector(".error-alert");

    public AddVehiclePage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TestConfig.DefaultTimeout);
    }

    public AddVehiclePage Open()
    {
        _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/vehicles/add");
        _wait.Until(d => d.FindElement(RegistrationNumberInput).Displayed);
        return this;
    }

    public void FillForm(string registrationNumber, string make, string model, string year, string fuelType)
    {
        _driver.FindElement(RegistrationNumberInput).Clear();
        _driver.FindElement(RegistrationNumberInput).SendKeys(registrationNumber);

        _driver.FindElement(MakeInput).Clear();
        _driver.FindElement(MakeInput).SendKeys(make);

        _driver.FindElement(ModelInput).Clear();
        _driver.FindElement(ModelInput).SendKeys(model);

        _driver.FindElement(YearInput).Clear();
        _driver.FindElement(YearInput).SendKeys(year);

        new SelectElement(_driver.FindElement(FuelTypeSelect)).SelectByValue(fuelType);
    }

    public void Submit() => _driver.FindElement(SubmitButton).Click();

    public void Cancel() => _driver.FindElement(CancelButton).Click();

    public bool HasErrorMessage()
    {
        try
        {
            _wait.Until(d => d.FindElements(ErrorAlert).Count > 0 && d.FindElement(ErrorAlert).Displayed);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetErrorMessage() => _driver.FindElement(ErrorAlert).Text;

    // Waits for the redirect back to the garage list after a successful save.
    public void WaitForSaveSuccess()
    {
        _wait.Until(d =>
        {
            if (HasErrorMessage())
            {
                throw new InvalidOperationException($"Add vehicle failed: {GetErrorMessage()}");
            }

            return d.Url.TrimEnd('/').EndsWith("/vehicles");
        });
    }
}
