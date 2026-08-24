using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using UI.SeleniumTests.TestHelpers;

namespace UI.SeleniumTests.Pages;

public class VehiclesPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private By VehicleCards => By.CssSelector(".vehicle-card");
    private By AddVehicleButton => By.XPath("//button[contains(., 'Add Vehicle')]");
    private By EmptyState => By.CssSelector(".empty-state");

    public VehiclesPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TestConfig.DefaultTimeout);
    }

    public VehiclesPage Open()
    {
        _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/vehicles");
        _wait.Until(d =>
            d.FindElements(VehicleCards).Count > 0 ||
            d.FindElements(EmptyState).Count > 0);
        return this;
    }

    public void ClickAddVehicle() => _driver.FindElement(AddVehicleButton).Click();

    public bool IsGarageEmpty() => _driver.FindElements(EmptyState).Count > 0;

    public int VehicleCount() => _driver.FindElements(VehicleCards).Count;

    // Finds a vehicle card by its registration number text and returns true if present.
    public bool HasVehicleWithRegistration(string registrationNumber)
    {
        return _driver.FindElements(VehicleCards)
            .Any(card => card.Text.Contains(registrationNumber));
    }

    public void ClickEditForVehicle(string registrationNumber)
    {
        var card = _driver.FindElements(VehicleCards)
            .First(c => c.Text.Contains(registrationNumber));

        card.FindElement(By.XPath(".//button[contains(., 'Edit Vehicle')]")).Click();
    }
}
