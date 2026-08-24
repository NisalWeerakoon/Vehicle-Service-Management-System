namespace UI.SeleniumTests.TestHelpers;

/// <summary>
/// Central place for anything environment-specific, so tests never
/// hard-code URLs. Override via environment variables in CI.
/// </summary>
public static class TestConfig
{
    // Frontend dev server (npm run dev). Change if you run it on another port.
    public static string BaseUrl =>
        Environment.GetEnvironmentVariable("QA_BASE_URL") ?? "http://localhost:5173";

    // How long Selenium waits for elements/navigation before failing.
    public static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(10);
}
