using System;
using Xunit;
using WinFormsLogger.Services;

namespace WinFormsLogger.Tests.Services;

public class DeviceIdentityServiceTests
{
    [Fact]
    public void GetDeviceId_ShouldReturnCorrectlyFormattedId()
    {
        // Arrange
        var service = new DeviceIdentityService();

        // Act
        string deviceId = service.GetDeviceId();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(deviceId));
        Assert.Contains(Environment.MachineName, deviceId);
        Assert.Contains("-", deviceId);
        
        int lastHyphenIndex = deviceId.LastIndexOf('-');
        Assert.True(lastHyphenIndex > 0);
        
        string machineNamePart = deviceId.Substring(0, lastHyphenIndex);
        string hashPart = deviceId.Substring(lastHyphenIndex + 1);
        
        Assert.Equal(Environment.MachineName, machineNamePart);
        Assert.Equal(12, hashPart.Length); // 12-char hex substring
        Assert.Matches("^[0-9A-F]{12}$", hashPart); // 12 uppercase hex characters
    }

    [Fact]
    public void GetDeviceId_ShouldBeDeterministic()
    {
        // Arrange
        var service = new DeviceIdentityService();

        // Act
        string id1 = service.GetDeviceId();
        string id2 = service.GetDeviceId();

        // Assert
        Assert.Equal(id1, id2);
    }
}
