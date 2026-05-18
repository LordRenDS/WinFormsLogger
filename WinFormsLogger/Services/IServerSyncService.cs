using System.Threading.Tasks;

namespace WinFormsLogger.Services;

public enum SyncStatus
{
    Success,
    NoUnsyncedData,
    NotAuthenticated,
    PartiallyFailed,
    Failed
}

public interface IServerSyncService
{
    Task<string> LoginAsync(string username, string password);
    Task<SyncStatus> SyncAsync();
}
