using System.Threading.Tasks;

namespace WinFormsLogger.Services;

public interface IServerSyncService
{
    Task<string> LoginAsync(string username, string password);
    Task SyncAsync();
}
