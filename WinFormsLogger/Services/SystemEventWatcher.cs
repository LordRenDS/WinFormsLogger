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
        
        LogStatus("PowerOn");
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

    private void OnSessionEnding(object sender, SessionEndingEventArgs e)
    {
        _logger.LogInformation($"SessionEnding: {e.Reason}");
        LogStatus("PowerOff");
    }

    private void LogStatus(string statusName)
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

    public void Dispose()
    {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        SystemEvents.SessionEnding -= OnSessionEnding;
    }
}
