using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using UI.SeleniumTests.TestHelpers;

namespace UI.SeleniumTests.Pages;

public class LoginPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private By EmailInput => By.Id("email");
    private By PasswordInput => By.Id("password");
    private By SubmitButton => By.CssSelector("button[type='submit']");
    private By ErrorAlert => By.CssSelector(".error-alert");

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TestConfig.DefaultTimeout);
    }

    public LoginPage Open()
    {
        _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/login");
        _wait.Until(d => d.FindElement(EmailInput).Displayed);
        return this;
    }

    public void Login(string email, string password)
    {
        _driver.FindElement(EmailInput).Clear();
        _driver.FindElement(EmailInput).SendKeys(email);

        _driver.FindElement(PasswordInput).Clear();
        _driver.FindElement(PasswordInput).SendKeys(password);

        _driver.FindElement(SubmitButton).Click();
    }

    public bool HasErrorMessage()
    {
        return _driver.FindElements(ErrorAlert).Count > 0;
    }

    public string GetErrorMessage()
    {
        return _driver.FindElement(ErrorAlert).Text;
    }

    // Waits for a successful login redirect to /profile.
    public void WaitForLoginSuccess()
    {
        _wait.Until(d => d.Url.Contains("/profile"));
    }
}
