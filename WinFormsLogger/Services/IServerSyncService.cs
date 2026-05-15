using System.Threading.Tasks;

namespace WinFormsLogger.Services;

public interface IServerSyncService
{
    Task SyncAsync();
}
