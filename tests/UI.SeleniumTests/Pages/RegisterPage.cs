using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using UI.SeleniumTests.TestHelpers;

namespace UI.SeleniumTests.Pages;

public class RegisterPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private By FullNameInput => By.Id("fullName");
    private By EmailInput => By.Id("email");
    private By PhoneInput => By.Id("phone");
    private By AddressInput => By.Id("address");
    private By PasswordInput => By.Id("password");
    private By ConfirmPasswordInput => By.Id("confirmPassword");
    private By SubmitButton => By.CssSelector("button[type='submit']");
    private By ErrorAlert => By.CssSelector(".error-alert");

    public RegisterPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TestConfig.DefaultTimeout);
    }

    public RegisterPage Open()
    {
        _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/register");
        _wait.Until(d => d.FindElement(EmailInput).Displayed);
        return this;
    }

    // Registers a full customer account (auth + profile in one flow).
    public void Register(
        string fullName,
        string email,
        string phone,
        string address,
        string password,
        string confirmPassword)
    {
        _driver.FindElement(FullNameInput).SendKeys(fullName);
        _driver.FindElement(EmailInput).SendKeys(email);
        _driver.FindElement(PhoneInput).SendKeys(phone);
        _driver.FindElement(AddressInput).SendKeys(address);
        _driver.FindElement(PasswordInput).SendKeys(password);
        _driver.FindElement(ConfirmPasswordInput).SendKeys(confirmPassword);
        _driver.FindElement(SubmitButton).Click();
    }

    public bool HasErrorMessage() => _driver.FindElements(ErrorAlert).Count > 0;

    public string GetErrorMessage() => _driver.FindElement(ErrorAlert).Text;

    // Waits for successful registration redirect (app takes new users to /profile).
    public void WaitForRegistrationSuccess()
    {
        _wait.Until(d =>
        {
            if (HasErrorMessage())
            {
                throw new InvalidOperationException($"Registration failed: {GetErrorMessage()}");
            }

            return d.Url.Contains("/profile") || d.Url.Contains("/vehicles");
        });
    }
}
