using FluentAssertions;
using OpenQA.Selenium;
using Xunit;

namespace CustomerBookingService.SeleniumTests;

public class RegisterAndLoginPageTests : SeleniumTestBase
{
    [Fact]
    public void Register_HappyPath_CreatesAccountAndRedirectsToProfile()
    {
        var uniqueEmail = $"selenium.{Guid.NewGuid():N}@example.com";

        GoTo("/register");

        WaitForElement(By.Id("fullName")).SendKeys("Selenium Test User");
        Driver.FindElement(By.Id("email")).SendKeys(uniqueEmail);
        Driver.FindElement(By.Id("phone")).SendKeys("0771234567");
        Driver.FindElement(By.Id("address")).SendKeys("42 Test Lane");
        Driver.FindElement(By.Id("password")).SendKeys("SeleniumPass123!");
        Driver.FindElement(By.Id("confirmPassword")).SendKeys("SeleniumPass123!");

        Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        WaitForUrlToContain("/profile");
        Driver.Url.Should().Contain("/profile");

        // The two-step register-then-create-profile call should have
        // completed, so the profile page should show real data, not a
        // "profile not found" state.
        var profileCard = WaitForElement(By.ClassName("profile-card"));
        profileCard.Text.Should().Contain("Selenium Test User");
    }

    [Fact]
    public void Register_PasswordMismatch_ShowsErrorAndDoesNotSubmit()
    {
        GoTo("/register");

        WaitForElement(By.Id("fullName")).SendKeys("Mismatch User");
        Driver.FindElement(By.Id("email")).SendKeys($"mismatch.{Guid.NewGuid():N}@example.com");
        Driver.FindElement(By.Id("phone")).SendKeys("0771234567");
        Driver.FindElement(By.Id("password")).SendKeys("PasswordOne1!");
        Driver.FindElement(By.Id("confirmPassword")).SendKeys("PasswordTwo2!");

        Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        var error = WaitForElement(By.ClassName("error-alert"));
        error.Text.Should().Contain("Passwords do not match");

        // Must NOT have navigated away
        Driver.Url.Should().Contain("/register");
    }

    [Fact]
    public void Register_ShortPassword_ShowsClientSideError()
    {
        GoTo("/register");

        WaitForElement(By.Id("fullName")).SendKeys("Short PW User");
        Driver.FindElement(By.Id("email")).SendKeys($"shortpw.{Guid.NewGuid():N}@example.com");
        Driver.FindElement(By.Id("phone")).SendKeys("0771234567");
        Driver.FindElement(By.Id("password")).SendKeys("short1");
        Driver.FindElement(By.Id("confirmPassword")).SendKeys("short1");

        Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        var error = WaitForElement(By.ClassName("error-alert"));
        error.Text.Should().Contain("at least 8 characters");
    }

    [Fact]
    public void Login_WithWrongPassword_ShowsErrorAlert()
    {
        // First register a real account through the UI so we know the email exists
        var email = $"wrongpw.{Guid.NewGuid():N}@example.com";
        GoTo("/register");
        WaitForElement(By.Id("fullName")).SendKeys("Wrong PW Test");
        Driver.FindElement(By.Id("email")).SendKeys(email);
        Driver.FindElement(By.Id("phone")).SendKeys("0771234567");
        Driver.FindElement(By.Id("password")).SendKeys("CorrectPass123!");
        Driver.FindElement(By.Id("confirmPassword")).SendKeys("CorrectPass123!");
        Driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        WaitForUrlToContain("/profile");

        // Log out, then try logging back in with the wrong password
        WaitForElement(By.ClassName("logout-button")).Click();
        WaitForUrlToContain("/login");

        WaitForElement(By.Id("email")).SendKeys(email);
        Driver.FindElement(By.Id("password")).SendKeys("TotallyWrongPassword!");
        Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        var error = WaitForElement(By.ClassName("error-alert"));
        error.Text.Should().NotBeNullOrWhiteSpace();

        // Must still be on the login page
        Driver.Url.Should().Contain("/login");
    }

    [Fact]
    public void ProtectedRoute_WithoutLogin_RedirectsToLoginPage()
    {
        // Hitting /profile directly with no token in localStorage
        GoTo("/profile");

        WaitForUrlToContain("/login");
        Driver.Url.Should().Contain("/login");
    }
}
