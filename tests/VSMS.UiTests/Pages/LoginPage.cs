using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using VSMS.UiTests.Helpers;

namespace VSMS.UiTests.Pages;

/// <summary>
/// Page Object for /login (frontend/src/pages/LoginPage.jsx).
/// </summary>
public class LoginPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    private IWebElement EmailInput => _driver.FindElement(By.Id("email"));
    private IWebElement PasswordInput => _driver.FindElement(By.Id("password"));
    private IWebElement SubmitButton => _driver.FindElement(By.CssSelector("form button[type='submit']"));
    private IWebElement? ErrorAlert =>
        _driver.FindElements(By.CssSelector(".error-alert")).FirstOrDefault();

    public void NavigateTo() => _driver.Navigate().GoToUrl($"{DriverFactory.BaseUrl}/login");

    public void Login(string email, string password)
    {
        _wait.Until(d => d.FindElement(By.Id("email")));
        EmailInput.Clear();
        EmailInput.SendKeys(email);
        PasswordInput.Clear();
        PasswordInput.SendKeys(password);
        SubmitButton.Click();
    }

    public string? GetErrorMessage() => ErrorAlert?.Text;

    public void WaitForRedirectAwayFromLogin()
    {
        _wait.Until(d => !d.Url.Contains("/login"));
    }
}
