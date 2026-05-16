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
    private readonly AppSettings _appSettings;
    private readonly ILogger<ServerSyncService> _logger;

    public ServerSyncService(
        IProcessRepository processRepository,
        IScheduleRepository scheduleRepository,
        IDeviceIdentityService deviceIdentityService,
        ICredentialService credentialService,
        AppSettings appSettings,
        ILogger<ServerSyncService> logger)
    {
        _processRepository = processRepository;
        _scheduleRepository = scheduleRepository;
        _deviceIdentityService = deviceIdentityService;
        _credentialService = credentialService;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task SyncAsync()
    {
        _logger.LogInformation("Starting server synchronization to {ServerUrl}...", _appSettings.ServerUrl);

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
            var token = credentials?.password; // Token is stored in the password field
            var deviceId = _deviceIdentityService.GetDeviceId();

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Synchronization skipped: No authentication token found. Please login.");
                return;
            }

            var syncPackage = new
            {
                DeviceId = deviceId,
                Username = credentials?.username,
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
            
            _logger.LogInformation("Using Authorization: Bearer {TokenPrefix}...", token.Substring(0, Math.Min(token.Length, 8)));
            _logger.LogInformation("Using X-Device-Id: {DeviceId}", deviceId);
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
