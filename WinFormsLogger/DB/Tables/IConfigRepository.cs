using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

public interface IConfigRepository
{
    Config? GetConfig();
    void SaveConfig(Config config);
}
