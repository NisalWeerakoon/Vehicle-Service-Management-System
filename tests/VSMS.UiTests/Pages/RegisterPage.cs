using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using VSMS.UiTests.Helpers;

namespace VSMS.UiTests.Pages;

/// <summary>
/// Page Object for /register (frontend/src/pages/RegisterPage.jsx).
/// Registration is two API calls under the hood (auth + customer profile)
/// but is a single form submission from the UI's point of view.
/// </summary>
public class RegisterPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public RegisterPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public void NavigateTo() => _driver.Navigate().GoToUrl($"{DriverFactory.BaseUrl}/register");

    public void Register(
        string fullName,
        string email,
        string phone,
        string password,
        string confirmPassword)
    {
        _wait.Until(d => d.FindElement(By.Name("fullName")));

        _driver.FindElement(By.Name("fullName")).SendKeys(fullName);
        _driver.FindElement(By.Name("email")).SendKeys(email);
        _driver.FindElement(By.Name("phone")).SendKeys(phone);
        _driver.FindElement(By.Name("password")).SendKeys(password);
        _driver.FindElement(By.Name("confirmPassword")).SendKeys(confirmPassword);

        _driver.FindElement(By.CssSelector("form button[type='submit']")).Click();
    }

    public string? GetErrorMessage() =>
        _driver.FindElements(By.CssSelector(".error-alert")).FirstOrDefault()?.Text;
}
