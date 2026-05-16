using System.Configuration;

namespace WinFormsLogger.Services;

public class AppSettings : ApplicationSettingsBase
{
    [UserScopedSetting]
    [DefaultSettingValue("https://api.example.com/sync")]
    public string ServerUrl
    {
        get => (string)this[nameof(ServerUrl)];
        set => this[nameof(ServerUrl)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("10")]
    public int SyncIntervalMinutes
    {
        get => (int)this[nameof(SyncIntervalMinutes)];
        set => this[nameof(SyncIntervalMinutes)] = value;
    }
}
