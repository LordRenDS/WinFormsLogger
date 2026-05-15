using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace WinFormsLogger.Services;

public class DeviceIdentityService : IDeviceIdentityService {
    public string GetDeviceId() {
        string machineName = Environment.MachineName;
        string hardwareId = GetMotherboardId();
        string combined = $"{machineName}_{hardwareId}";
        
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return $"{machineName}-{Convert.ToHexString(hashBytes).Substring(0, 12)}";
    }

    private string GetMotherboardId() {
        try {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (var obj in searcher.Get()) {
                return obj["SerialNumber"]?.ToString() ?? "N/A";
            }
        } catch { /* ignore */ }
        return "UnknownHardware";
    }
}
