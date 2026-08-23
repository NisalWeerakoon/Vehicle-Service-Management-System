using FluentAssertions;
using OpenQA.Selenium;
using Xunit;

namespace CustomerBookingService.SeleniumTests;

public class ProfilePageTests : SeleniumTestBase
{
    /// <summary>Registers a fresh account through the real UI and returns its credentials.</summary>
    private (string email, string password) RegisterFreshAccount(string fullName = "Profile Test User")
    {
        var email = $"profile.{Guid.NewGuid():N}@example.com";
        const string password = "ProfileTest123!";

        GoTo("/register");
        WaitForElement(By.Id("fullName")).SendKeys(fullName);
        Driver.FindElement(By.Id("email")).SendKeys(email);
        Driver.FindElement(By.Id("phone")).SendKeys("0771234567");
        Driver.FindElement(By.Id("password")).SendKeys(password);
        Driver.FindElement(By.Id("confirmPassword")).SendKeys(password);
        Driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        WaitForUrlToContain("/profile");

        return (email, password);
    }

    [Fact]
    public void Profile_DisplaysCorrectDataAfterRegistration()
    {
        var (email, _) = RegisterFreshAccount(fullName: "Display Check User");

        WaitForElement(By.CssSelector("button.small-button"));
        var profileCard = WaitForElement(By.ClassName("profile-card"));
        profileCard.Text.Should().Contain("Display Check User");
        profileCard.Text.Should().Contain(email);
    }

    [Fact]
    public void EditProfile_UpdatingFullNameAndPhone_PersistsAfterSave()
    {
        RegisterFreshAccount(fullName: "Before Edit");

        WaitForElement(By.CssSelector("button.small-button")).Click(); // "Edit Profile"
        WaitForUrlToContain("/profile/edit");

        var fullNameInput = WaitForElement(By.Id("fullName"));
        fullNameInput.Clear();
        fullNameInput.SendKeys("After Edit");

        var phoneInput = Driver.FindElement(By.Id("phone"));
        phoneInput.Clear();
        phoneInput.SendKeys("0799998888");

        Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Success message appears, then the page auto-navigates back to /profile
        var success = WaitForElement(By.ClassName("success-alert"));
        success.Text.Should().Contain("updated successfully");

        WaitForUrlToContain("/profile");

        WaitForElement(By.CssSelector("button.small-button"));
        var profileCard = WaitForElement(By.ClassName("profile-card"));
        profileCard.Text.Should().Contain("After Edit");
        profileCard.Text.Should().Contain("0799998888");
    }

    [Fact]
    public void EditProfile_EmailFieldIsReadOnly()
    {
        var (email, _) = RegisterFreshAccount();

        WaitForElement(By.CssSelector("button.small-button")).Click();
        WaitForUrlToContain("/profile/edit");

        var emailField = WaitForElement(By.CssSelector("input[type='email']"));
        emailField.GetAttribute("disabled").Should().NotBeNull();
        emailField.GetAttribute("value").Should().Be(email);
    }

    [Fact]
    public void EditProfile_CancelButton_ReturnsToProfileWithoutSaving()
    {
        RegisterFreshAccount(fullName: "Should Not Change");

        WaitForElement(By.CssSelector("button.small-button")).Click();
        WaitForUrlToContain("/profile/edit");

        var fullNameInput = WaitForElement(By.Id("fullName"));
        fullNameInput.Clear();
        fullNameInput.SendKeys("This Should Be Discarded");

        Driver.FindElement(By.ClassName("secondary-button")).Click(); // "Cancel"
        WaitForUrlToContain("/profile");

        WaitForElement(By.CssSelector("button.small-button"));
        var profileCard = WaitForElement(By.ClassName("profile-card"));
        profileCard.Text.Should().Contain("Should Not Change");
        profileCard.Text.Should().NotContain("This Should Be Discarded");
    }

    [Fact]
    public void Logout_ClearsSessionAndRedirectsToLogin()
    {
        RegisterFreshAccount();

        WaitForElement(By.ClassName("logout-button")).Click();
        WaitForUrlToContain("/login");

        // Confirm the session really is gone -- going back to /profile
        // directly should bounce to /login again, not show cached data.
        GoTo("/profile");
        WaitForUrlToContain("/login");
        Driver.Url.Should().Contain("/login");
    }
}
