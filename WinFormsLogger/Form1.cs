using Microsoft.Extensions.Logging;
using System.ComponentModel;
using WinFormsLogger.DB.Models;
using WinFormsLogger.DB.Tables;
using WinFormsLogger.Forms;
using WinFormsLogger.Services;

namespace WinFormsLogger;

public partial class Form1 : Form
{
    private readonly ILogger<Form1> logger;
    private readonly IProcessRepository processes;
    private readonly IProcessTracer processTracer;
    private readonly ICredentialService credentialService;
    private readonly ISystemEventWatcher systemEventWatcher;
    private readonly IServerSyncService serverSyncService;
    private readonly IConfigRepository configRepository;
    private readonly IDeviceIdentityService deviceIdentityService;
    private readonly AppSettings _appSettings;
    private readonly BindingList<Process> _processCache = new();
    private bool _isExiting = false;
    private Process? _activeProcess;
    private System.Windows.Forms.Timer _dbSaveTimer;
    private System.Windows.Forms.Timer _syncTimer;

    public Form1(
        ILogger<Form1> logger, 
        IProcessRepository processes, 
        IProcessTracer processTracer, 
        ICredentialService credentialService, 
        ISystemEventWatcher systemEventWatcher,
        IServerSyncService serverSyncService,
        IConfigRepository configRepository,
        IDeviceIdentityService deviceIdentityService,
        AppSettings appSettings)
    {
        InitializeComponent();
        this.logger = logger;
        this.processes = processes;
        this.processTracer = processTracer;
        this.credentialService = credentialService;
        this.systemEventWatcher = systemEventWatcher;
        this.serverSyncService = serverSyncService;
        this.configRepository = configRepository;
        this.deviceIdentityService = deviceIdentityService;
        this._appSettings = appSettings;
        
        // Ensure the tray icon uses the form's icon
        this.notifyIcon1.Icon = this.Icon;

        _dbSaveTimer = new System.Windows.Forms.Timer();
        _dbSaveTimer.Interval = 5 * 60 * 1000; // 5 minutes
        _dbSaveTimer.Tick += DbSaveTimer_Tick;
        _dbSaveTimer.Start();

        _syncTimer = new System.Windows.Forms.Timer();
        UpdateSyncTimerInterval();
        _syncTimer.Tick += SyncTimer_Tick;
        _syncTimer.Start();

        // Bind DataGridView to cache
        bindingSource1.DataSource = _processCache;
        dataGridView1.DataSource = bindingSource1;

        // Initialize log panel visibility
        showLogsToolStripMenuItem.Checked = _appSettings.ShowServerLogs;
        splitContainer1.Panel2Collapsed = !_appSettings.ShowServerLogs;
    }

    private void LogServerEvent(string message)
    {
        if (lstServerLogs.InvokeRequired)
        {
            lstServerLogs.Invoke(new Action(() => LogServerEvent(message)));
            return;
        }

        string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
        lstServerLogs.Items.Add(logEntry);

        // Scroll to the bottom
        lstServerLogs.SelectedIndex = lstServerLogs.Items.Count - 1;
        lstServerLogs.ClearSelected();
    }

    private void UpdateSyncTimerInterval()
    {
        _syncTimer.Interval = Math.Max(1, _appSettings.SyncIntervalMinutes) * 60 * 1000;
        logger.LogInformation("Sync timer interval set to {Interval} minutes", _appSettings.SyncIntervalMinutes);
    }

    private async void SyncTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            LogServerEvent("Background sync started...");
            if (_activeProcess != null)
            {
                _activeProcess.IsSynced = false;
                processes.UpdateProcess(_activeProcess);
            }
            await serverSyncService.SyncAsync();
            LogServerEvent("Background sync completed.");

            // Update UI
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateCacheSyncStatus));
            }
            else
            {
                UpdateCacheSyncStatus();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Automated background sync failed");
            LogServerEvent($"Background sync failed: {ex.Message}");
        }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        logger.Log(LogLevel.Information, "Form1_Load");
        systemEventWatcher.Start();

        // Initialize Device ID if not set
        var config = configRepository.GetConfig();
        if (config == null || string.IsNullOrEmpty(config.PcId))
        {
            var deviceId = deviceIdentityService.GetDeviceId();
            configRepository.SaveConfig(new Config { PcId = deviceId });
            logger.LogInformation($"Initialized Device ID: {deviceId}");
        }
        else
        {
            logger.LogInformation($"Using existing Device ID: {config.PcId}");
        }

        // Завантаження процесів за сьогодні у кеш
        var todayProcesses = processes.GetAllProcesses()
            .Where(p => p.ProcessStart.Date == DateTime.Today)
            .OrderByDescending(p => p.ProcessStart);
        
        foreach (var p in todayProcesses)
        {
            _processCache.Add(p);
        }

        activeProcessTimer.Interval = 1000; // Оновлюємо кожну секунду
        activeProcessTimer.Start();
    }

    private void loginToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var loginForm = new LoginForm(serverSyncService, credentialService);
        if (loginForm.ShowDialog() == DialogResult.OK)
        {
            MessageBox.Show("Вхід виконано успішно", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        credentialService.DeleteCredentials();
        MessageBox.Show("Вихід виконано", "Logout", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void DbSaveTimer_Tick(object? sender, EventArgs e)
    {
        if (_activeProcess != null)
        {
            try
            {
                _activeProcess.IsSynced = false; // Mark as unsynced since duration changed
                processes.UpdateProcess(_activeProcess);
                logger.LogInformation($"Periodic save: Updated duration and reset sync status for {_activeProcess.ProcessName}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during periodic database save");
            }
        }
    }

    private void ActiveProcessTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            Process captured = processTracer.GetActiveProcess();

            // Перевіряємо, чи це той самий процес, що й зараз активний
            if (_activeProcess != null && 
                _activeProcess.ProcessName == captured.ProcessName && 
                _activeProcess.WindowsName == captured.WindowsName &&
                _activeProcess.ProcessStart == captured.ProcessStart)
            {
                // Той самий процес - просто додаємо секунду до Duration у пам'яті
                // UI оновиться автоматично завдяки INotifyPropertyChanged
                _activeProcess.Duration++;
                return;
            }

            // Процес змінився або це перший запуск
            
            // Зберігаємо попередній процес у БД
            if (_activeProcess != null)
            {
                processes.UpdateProcess(_activeProcess);
            }

            // Шукаємо captured процес у нашому кеші (можливо користувач повернувся до вікна)
            var existingInCache = _processCache.FirstOrDefault(p => 
                p.ProcessName == captured.ProcessName && 
                p.WindowsName == captured.WindowsName &&
                p.ProcessStart == captured.ProcessStart);

            if (existingInCache != null)
            {
                _activeProcess = existingInCache;
            }
            else
            {
                // Новий процес, якого ще не було сьогодні (або принаймні у кеші)
                captured.Duration = 0;
                captured.Id = processes.CreateProcess(captured);
                _processCache.Insert(0, captured); // Додаємо на початок списку
                _activeProcess = captured;
            }

            logger.Log(LogLevel.Information, $"Active process changed: {captured.ProcessName}");
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, $"Monitoring error: {ex.Message}");
        }
    }

    private void RefreshDataGridView()
    {
        // Більше не потрібно завантажувати все з БД, 
        // BindingList та INotifyPropertyChanged роблять це автоматично.
        // Але якщо потрібно примусово перечитати (наприклад, після синхронізації), можна очистити і заповнити кеш.
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_isExiting && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide();
            notifyIcon1.ShowBalloonTip(2000, "WinFormsLogger", "Застосунок продовжує працювати у треї", ToolTipIcon.Info);
            return;
        }

        activeProcessTimer?.Stop();
        _dbSaveTimer?.Stop();

        try
        {
            if (_activeProcess != null)
            {
                processes.UpdateProcess(_activeProcess);
                logger.Log(LogLevel.Information, $"Збережено тривалість процесу при виході: {_activeProcess.ProcessName} | {_activeProcess.Duration} сек.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving active process during form closing");
        }
    }

    private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        ShowForm();
    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ShowForm();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _isExiting = true;
        Application.Exit();
    }

    private void ShowForm()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
    }

    private async void syncNowToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (credentialService.GetCredentials() == null)
        {
            MessageBox.Show("Будь ласка, спочатку виконайте вхід у систему (Меню -> Login), щоб мати змогу синхронізувати дані з сервером.", "Синхронізація", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        syncNowToolStripMenuItem.Enabled = false;
        try
        {
            LogServerEvent("Manual sync started...");
            if (_activeProcess != null)
            {
                _activeProcess.IsSynced = false; // Mark as unsynced to ensure incremental sync picks it up
                processes.UpdateProcess(_activeProcess);
            }
            await serverSyncService.SyncAsync();
            LogServerEvent("Manual sync completed.");
            
            // Update IsSynced status in cache from DB
            UpdateCacheSyncStatus();
            
            MessageBox.Show("Синхронізація завершена", "Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Manual sync failed");
            LogServerEvent($"Manual sync failed: {ex.Message}");
            MessageBox.Show($"Помилка синхронізації: {ex.Message}", "Sync Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            syncNowToolStripMenuItem.Enabled = true;
        }
    }

    private void UpdateCacheSyncStatus()
    {
        // Get all processes from DB for today to see their current sync status
        var dbProcesses = processes.GetAllProcesses()
            .Where(p => p.ProcessStart.Date == DateTime.Today)
            .ToDictionary(p => p.Id, p => p.IsSynced);

        foreach (var process in _processCache)
        {
            if (dbProcesses.TryGetValue(process.Id, out bool isSynced))
            {
                process.IsSynced = isSynced;
            }
        }
    }

    private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var settingsForm = new SettingsForm(_appSettings);
        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            logger.LogInformation("Settings updated: ServerUrl={ServerUrl}, SyncInterval={SyncInterval}",
                _appSettings.ServerUrl, _appSettings.SyncIntervalMinutes);
            UpdateSyncTimerInterval();
        }
    }

    private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        exitToolStripMenuItem_Click(sender, e);
    }

    private void showLogsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        splitContainer1.Panel2Collapsed = !showLogsToolStripMenuItem.Checked;
        _appSettings.ShowServerLogs = showLogsToolStripMenuItem.Checked;
        _appSettings.Save();
    }
}
