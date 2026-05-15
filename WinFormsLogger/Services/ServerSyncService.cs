using Microsoft.Extensions.Logging;
using System.Text.Json;
using WinFormsLogger.DB.Tables;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.Services;

public class ServerSyncService : IServerSyncService
{
    private readonly IProcessRepository _processRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IDeviceIdentityService _deviceIdentityService;
    private readonly ICredentialService _credentialService;
    private readonly ILogger<ServerSyncService> _logger;

    public ServerSyncService(
        IProcessRepository processRepository,
        IScheduleRepository scheduleRepository,
        IDeviceIdentityService deviceIdentityService,
        ICredentialService credentialService,
        ILogger<ServerSyncService> logger)
    {
        _processRepository = processRepository;
        _scheduleRepository = scheduleRepository;
        _deviceIdentityService = deviceIdentityService;
        _credentialService = credentialService;
        _logger = logger;
    }

    public async Task SyncAsync()
    {
        _logger.LogInformation("Starting server synchronization...");

        try
        {
            var unsyncedProcesses = _processRepository.GetAllProcesses().Where(p => !p.IsSynced).ToList();
            var unsyncedSchedules = _scheduleRepository.GetAll().Where(s => !s.IsSynced).ToList();

            if (unsyncedProcesses.Count == 0 && unsyncedSchedules.Count == 0)
            {
                _logger.LogInformation("No unsynced data found.");
                return;
            }

            var credentials = _credentialService.GetCredentials();
            var username = credentials?.username ?? "Unknown";

            var syncPackage = new
            {
                DeviceId = _deviceIdentityService.GetDeviceId(),
                Username = username,
                Processes = unsyncedProcesses.Select(p => new
                {
                    p.ProcessName,
                    p.WindowsName,
                    p.ProcessStart,
                    p.Duration
                }),
                Schedules = unsyncedSchedules.Select(s => new
                {
                    s.PcStatusId,
                    s.Timestamp
                })
            };

            string json = JsonSerializer.Serialize(syncPackage, new JsonSerializerOptions { WriteIndented = true });
            
            _logger.LogInformation("Generated Sync JSON:\n{Json}", json);

            // Stub: simulate network request
            await Task.Delay(500); 

            _logger.LogInformation("Simulated sync successful. Updating database records...");

            foreach (var process in unsyncedProcesses)
            {
                process.IsSynced = true;
                _processRepository.UpdateProcess(process);
            }

            foreach (var schedule in unsyncedSchedules)
            {
                schedule.IsSynced = true;
                _scheduleRepository.Update(schedule);
            }

            _logger.LogInformation("Synchronization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during server synchronization");
        }
    }
}
