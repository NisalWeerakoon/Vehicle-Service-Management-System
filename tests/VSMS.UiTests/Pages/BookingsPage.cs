using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using VSMS.UiTests.Helpers;

namespace VSMS.UiTests.Pages;

/// <summary>
/// Page Object for /bookings (frontend/src/pages/BookingsPage.jsx).
/// </summary>
public class BookingsPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public BookingsPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    private IReadOnlyCollection<IWebElement> BookingCards =>
        _driver.FindElements(By.CssSelector(".booking-card"));

    public void NavigateTo() => _driver.Navigate().GoToUrl($"{DriverFactory.BaseUrl}/bookings");

    public void WaitForBookingsToLoad()
    {
        _wait.Until(d =>
            d.FindElements(By.CssSelector(".booking-card")).Count > 0 ||
            d.FindElements(By.CssSelector(".empty-state")).Count > 0);
    }

    public int BookingCount => BookingCards.Count;

    /// <summary>
    /// The API returns bookings newest-first (BookingsController.GetMyBookings
    /// orders by CreatedAt descending), so the first card is always the latest.
    /// </summary>
    public string GetFirstBookingProblemText()
    {
        var firstCard = BookingCards.First();
        return firstCard.FindElement(By.CssSelector(".booking-problem p")).Text;
    }

    public string GetFirstBookingStatus()
    {
        var firstCard = BookingCards.First();
        return firstCard.FindElement(By.CssSelector(".booking-status")).Text;
    }

    public void ClickCreateBooking() =>
        _driver.FindElement(By.CssSelector(".page-heading button.primary-button")).Click();
}
