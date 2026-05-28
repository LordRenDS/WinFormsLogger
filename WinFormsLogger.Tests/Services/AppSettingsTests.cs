using Xunit;
using WinFormsLogger.Services;

namespace WinFormsLogger.Tests.Services;

public class AppSettingsTests
{
    [Fact]
    public void AppSettings_ShouldHaveExpectedDefaultValues()
    {
        // Arrange & Act
        var settings = new AppSettings();
        settings.Reset();

        // Assert
        Assert.Equal("http://localhost:8080", settings.ServerUrl);
        Assert.Equal(10, settings.SyncIntervalMinutes);
        Assert.True(settings.ShowServerLogs);
    }

    [Fact]
    public void AppSettings_ShouldAllowUpdatingProperties()
    {
        // Arrange
        var settings = new AppSettings();
        settings.Reset();

        // Act
        settings.ServerUrl = "https://example.com";
        settings.SyncIntervalMinutes = 30;
        settings.ShowServerLogs = false;

        // Assert
        Assert.Equal("https://example.com", settings.ServerUrl);
        Assert.Equal(30, settings.SyncIntervalMinutes);
        Assert.False(settings.ShowServerLogs);
    }
}
