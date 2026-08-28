using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace VSMS.UiTests.Pages;

/// <summary>Page Object for /bookings/:id/edit (frontend/src/pages/EditBookingPage.jsx).</summary>
public class EditBookingPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public EditBookingPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    private IWebElement DateInput => _driver.FindElement(By.Id("preferredDate"));
    private IWebElement ProblemTextarea => _driver.FindElement(By.Id("requestedServiceOrProblem"));
    private IWebElement SaveButton => _driver.FindElement(By.CssSelector("button[type='submit']"));

    public void WaitForLoad() => _wait.Until(d => d.FindElement(By.Id("preferredDate")));

    public void UpdateBooking(string isoDate, string problemDescription)
    {
        WaitForLoad();

        DateInput.Clear();
        DateInput.SendKeys(isoDate);

        ProblemTextarea.Clear();
        ProblemTextarea.SendKeys(problemDescription);

        SaveButton.Click();
    }

    public string? GetErrorMessage() =>
        _driver.FindElements(By.CssSelector(".error-alert")).FirstOrDefault()?.Text;
}
