using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using WinFormsLogger.DB.Tables;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.Services;

public class ServerSyncService : IServerSyncService
{
    private readonly IProcessRepository _processRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IPcStatusRepository _pcStatusRepository;
    private readonly IDeviceIdentityService _deviceIdentityService;
    private readonly ICredentialService _credentialService;
    private readonly AppSettings _appSettings;
    private readonly ILogger<ServerSyncService> _logger;
    private readonly HttpClient _httpClient;

    public ServerSyncService(
        IProcessRepository processRepository,
        IScheduleRepository scheduleRepository,
        IPcStatusRepository pcStatusRepository,
        IDeviceIdentityService deviceIdentityService,
        ICredentialService credentialService,
        AppSettings appSettings,
        ILogger<ServerSyncService> logger)
        : this(processRepository, scheduleRepository, pcStatusRepository, deviceIdentityService, credentialService, appSettings, logger, new HttpClient())
    {
    }

    internal ServerSyncService(
        IProcessRepository processRepository,
        IScheduleRepository scheduleRepository,
        IPcStatusRepository pcStatusRepository,
        IDeviceIdentityService deviceIdentityService,
        ICredentialService credentialService,
        AppSettings appSettings,
        ILogger<ServerSyncService> logger,
        HttpClient httpClient)
    {
        _processRepository = processRepository;
        _scheduleRepository = scheduleRepository;
        _pcStatusRepository = pcStatusRepository;
        _deviceIdentityService = deviceIdentityService;
        _credentialService = credentialService;
        _appSettings = appSettings;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        _logger.LogInformation("Attempting login for user: {Username}", username);

        var loginData = new { email = username, password = password };
        var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_appSettings.ServerUrl}/api/v1/auth/login", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Login failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
            throw new Exception($"Login failed: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(result);
        string token = doc.RootElement.GetProperty("access_token").GetString() ?? throw new Exception("Token not found in response");

        _logger.LogInformation("Login successful for user: {Username}", username);
        return token;
    }

    public async Task<SyncStatus> SyncAsync()
    {
        _logger.LogInformation("Starting server synchronization to {ServerUrl}...", _appSettings.ServerUrl);

        try
        {
            var unsyncedProcesses = _processRepository.GetAllProcesses().Where(p => !p.IsSynced).ToList();
            var unsyncedSchedules = _scheduleRepository.GetAll().Where(s => !s.IsSynced).ToList();

            if (unsyncedProcesses.Count == 0 && unsyncedSchedules.Count == 0)
            {
                _logger.LogInformation("No unsynced data found.");
                return SyncStatus.NoUnsyncedData;
            }

            var credentials = _credentialService.GetCredentials();
            var token = credentials?.password; // Token is stored in the password field
            var deviceId = _deviceIdentityService.GetDeviceId();
            var pcName = Environment.MachineName;

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Synchronization skipped: No authentication token found. Please login.");
                return SyncStatus.NotAuthenticated;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            bool registrationResult = await RegisterPcAsync(deviceId, pcName);
            if (!registrationResult)
            {
                _logger.LogWarning("Synchronization aborted: PC registration failed.");
                return SyncStatus.Failed;
            }

            bool processSyncResult = true;
            if (unsyncedProcesses.Count > 0)
            {
                processSyncResult = await SyncProcessesAsync(deviceId, pcName, unsyncedProcesses);
            }

            bool scheduleSyncResult = true;
            if (unsyncedSchedules.Count > 0)
            {
                scheduleSyncResult = await SyncSchedulesAsync(deviceId, pcName, unsyncedSchedules);
            }

            if (processSyncResult && scheduleSyncResult)
            {
                _logger.LogInformation("Synchronization completed successfully.");
                return SyncStatus.Success;
            }
            else
            {
                _logger.LogWarning("Synchronization partially failed. Some data might not have been synced.");
                return SyncStatus.PartiallyFailed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during server synchronization");
            return SyncStatus.Failed;
        }
    }

    private async Task<bool> RegisterPcAsync(string deviceId, string pcName)
    {
        _logger.LogInformation("Ensuring PC is registered on the server: {DeviceId} ({PcName})", deviceId, pcName);
        try
        {
            var payload = new { unique_id = deviceId, name = pcName };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_appSettings.ServerUrl}/api/v1/pcs", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("PC registered successfully.");
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("PC registration failed: {StatusCode} - {Error}", response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PC registration");
            return false;
        }
    }

    private async Task<bool> SyncProcessesAsync(string deviceId, string pcName, List<Process> processes)
    {
        _logger.LogInformation("Syncing {Count} processes...", processes.Count);
        try
        {
            var payload = new
            {
                data = processes.Select(p => new
                {
                    process_start = p.ProcessStart.ToString("yyyy-MM-dd HH:mm:ss"),
                    process_name = p.ProcessName,
                    window_name = p.WindowsName,
                    duration = p.Duration
                })
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_appSettings.ServerUrl}/api/v1/pcs/{deviceId}/processes", content);

            if (response.IsSuccessStatusCode)
            {
                foreach (var process in processes)
                {
                    process.IsSynced = true;
                    _processRepository.UpdateProcess(process);
                }
                _logger.LogInformation("Processes synced successfully.");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to sync processes: {StatusCode} - {Error}", response.StatusCode, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during processes synchronization");
            return false;
        }
    }

    private async Task<bool> SyncSchedulesAsync(string deviceId, string pcName, List<Schedule> schedules)
    {
        _logger.LogInformation("Syncing {Count} schedules...", schedules.Count);
        try
        {
            var statuses = _pcStatusRepository.GetAll().ToDictionary(s => s.Id, s => s.Status);

            var payload = new
            {
                data = schedules.Select(s => new
                {
                    timestamp = s.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    status = MapStatus(statuses.ContainsKey(s.PcStatusId) ? statuses[s.PcStatusId] : "unknown")
                })
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_appSettings.ServerUrl}/api/v1/pcs/{deviceId}/schedules", content);

            if (response.IsSuccessStatusCode)
            {
                foreach (var schedule in schedules)
                {
                    schedule.IsSynced = true;
                    _scheduleRepository.Update(schedule);
                }
                _logger.LogInformation("Schedules synced successfully.");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to sync schedules: {StatusCode} - {Error}", response.StatusCode, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during schedules synchronization");
            return false;
        }
    }

    private string MapStatus(string? localStatus)
    {
        return localStatus switch
        {
            "PowerOn" => "on",
            "Unlocked" => "on",
            "PowerOff" => "off",
            "Locked" => "off",
            _ => "on" // Default to on if unknown
        };
    }
}
