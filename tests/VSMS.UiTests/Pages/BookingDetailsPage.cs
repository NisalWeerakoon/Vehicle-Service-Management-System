using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace VSMS.UiTests.Pages;

/// <summary>Page Object for /bookings/:id (frontend/src/pages/BookingDetailsPage.jsx).</summary>
public class BookingDetailsPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public BookingDetailsPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public void WaitForLoad() =>
        _wait.Until(d => d.FindElements(By.CssSelector(".booking-details-card")).Count > 0);

    public string GetStatusText() =>
        _driver.FindElement(By.CssSelector(".booking-details-header .booking-status")).Text;

    public bool HasEditButton() =>
        _driver.FindElements(By.XPath("//button[contains(., 'Edit Booking')]")).Any();

    public bool HasCancelButton() =>
        _driver.FindElements(By.CssSelector("button.danger-button")).Any();

    public void ClickEditBooking() =>
        _driver.FindElement(By.XPath("//button[contains(., 'Edit Booking')]")).Click();

    /// <summary>Clicks "Cancel Booking" and accepts the native window.confirm() dialog.</summary>
    public void ClickCancelBookingAndConfirm()
    {
        _driver.FindElement(By.CssSelector("button.danger-button")).Click();
        _wait.Until(d => d.SwitchTo().Alert() != null);
        _driver.SwitchTo().Alert().Accept();
    }
}
