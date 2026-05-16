using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using WinFormsLogger.DB.Models;
using WinFormsLogger.DB.Tables;

namespace WinFormsLogger.Services;

public class SystemEventWatcher : ISystemEventWatcher, IDisposable
{
    private readonly ILogger<SystemEventWatcher> _logger;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IPcStatusRepository _pcStatusRepository;
    private readonly Dictionary<string, int> _statusMap = new();

    public SystemEventWatcher(
        ILogger<SystemEventWatcher> logger,
        IScheduleRepository scheduleRepository,
        IPcStatusRepository pcStatusRepository)
    {
        _logger = logger;
        _scheduleRepository = scheduleRepository;
        _pcStatusRepository = pcStatusRepository;
    }

    public void Start()
    {
        _logger.LogInformation("SystemEventWatcher starting...");
        LoadStatusMap();

        SystemEvents.SessionSwitch += OnSessionSwitch;
        SystemEvents.SessionEnding += OnSessionEnding;
        
        // Track PowerOn based on system boot time
        try
        {
            DateTime bootTime = DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount64);
            if (_statusMap.TryGetValue("PowerOn", out int powerOnId))
            {
                // Use 5 minute tolerance to avoid duplicate entries for the same boot
                if (!_scheduleRepository.Exists(powerOnId, bootTime, TimeSpan.FromMinutes(5)))
                {
                    LogStatusAtTime("PowerOn", bootTime);
                }
                else
                {
                    _logger.LogInformation("PowerOn event for current boot already exists. Skipping.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking boot time PowerOn event");
        }
    }

    private void LoadStatusMap()
    {
        var statuses = _pcStatusRepository.GetAll();
        foreach (var status in statuses)
        {
            if (status.Status != null)
            {
                _statusMap[status.Status] = status.Id;
            }
        }
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        try
        {
            _logger.LogInformation($"SessionSwitch: {e.Reason}");
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    LogStatus("Locked");
                    break;
                case SessionSwitchReason.SessionUnlock:
                    LogStatus("Unlocked");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling SessionSwitch event");
        }
    }

    private void OnSessionEnding(object sender, SessionEndingEventArgs e)
    {
        try
        {
            _logger.LogInformation($"SessionEnding: {e.Reason}");
            LogStatus("PowerOff");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling SessionEnding event");
        }
    }

    private void LogStatus(string statusName)
    {
        try
        {
            if (_statusMap.TryGetValue(statusName, out int statusId))
            {
                var schedule = new Schedule
                {
                    PcStatusId = statusId,
                    Timestamp = DateTime.Now,
                    IsSynced = false
                };
                _scheduleRepository.Create(schedule);
                _logger.LogInformation($"Logged PC status: {statusName}");
            }
            else
            {
                _logger.LogWarning($"Status '{statusName}' not found in database.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error logging status: {statusName}");
        }
    }

    private void LogStatusAtTime(string statusName, DateTime time)
    {
        try
        {
            if (_statusMap.TryGetValue(statusName, out int statusId))
            {
                var schedule = new Schedule
                {
                    PcStatusId = statusId,
                    Timestamp = time,
                    IsSynced = false
                };
                _scheduleRepository.Create(schedule);
                _logger.LogInformation($"Logged PC status: {statusName} at {time}");
            }
            else
            {
                _logger.LogWarning($"Status '{statusName}' not found in database.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error logging status: {statusName}");
        }
    }

    public void Dispose()
    {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        SystemEvents.SessionEnding -= OnSessionEnding;
    }
}
