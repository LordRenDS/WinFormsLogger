using Microsoft.Extensions.Logging;
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
    private readonly Dictionary<DateTime, int> trackedInstances = new();
    private bool _isExiting = false;
    private Process? _activeProcess;
    private System.Windows.Forms.Timer _dbSaveTimer;

    public Form1(
        ILogger<Form1> logger, 
        IProcessRepository processes, 
        IProcessTracer processTracer, 
        ICredentialService credentialService, 
        ISystemEventWatcher systemEventWatcher,
        IServerSyncService serverSyncService,
        IConfigRepository configRepository,
        IDeviceIdentityService deviceIdentityService)
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
        
        // Ensure the tray icon uses the form's icon
        this.notifyIcon1.Icon = this.Icon;

        _dbSaveTimer = new System.Windows.Forms.Timer();
        _dbSaveTimer.Interval = 5 * 60 * 1000; // 5 minutes
        _dbSaveTimer.Tick += DbSaveTimer_Tick;
        _dbSaveTimer.Start();
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

        // Початкове завантаження кешу для процесів, що вже записані сьогодні
        var todayProcesses = processes.GetAllProcesses()
            .Where(p => p.ProcessStart.Date == DateTime.Today);
        
        foreach (var p in todayProcesses)
        {
            if (!trackedInstances.ContainsKey(p.ProcessStart))
            {
                trackedInstances.Add(p.ProcessStart, p.Id);
            }
        }

        bindingSource1.DataSource = processes.GetAllProcesses().ToList();
        dataGridView1.DataSource = bindingSource1;

        activeProcessTimer.Start();
    }

    private void loginToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var loginForm = new LoginForm();
        if (loginForm.ShowDialog() == DialogResult.OK)
        {
            credentialService.SaveCredentials(loginForm.Username, loginForm.Password);
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
                processes.UpdateProcess(_activeProcess);
                logger.LogInformation($"Periodic save: Updated duration for {_activeProcess.ProcessName}");
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

            if (_activeProcess != null && 
                _activeProcess.ProcessName == captured.ProcessName && 
                _activeProcess.WindowsName == captured.WindowsName &&
                _activeProcess.ProcessStart == captured.ProcessStart)
            {
                // Same process - update duration in memory
                _activeProcess.Duration = (int)(DateTime.Now - _activeProcess.ProcessStart).TotalSeconds;
                RefreshDataGridView(); // Update UI
                return;
            }

            // Process changed - save old one if exists
            if (_activeProcess != null)
            {
                processes.UpdateProcess(_activeProcess);
            }

            // Initialize new active process
            if (trackedInstances.TryGetValue(captured.ProcessStart, out int existingId))
            {
                captured.Id = existingId;
                captured.Duration = (int)(DateTime.Now - captured.ProcessStart).TotalSeconds;
                processes.UpdateProcess(captured);
            }
            else
            {
                captured.Duration = 0;
                captured.Id = processes.CreateProcess(captured);
                trackedInstances[captured.ProcessStart] = captured.Id;
            }

            _activeProcess = captured;
            logger.Log(LogLevel.Information, $"Started tracking new process: {captured.ProcessName}");
            RefreshDataGridView();
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, $"Monitoring error: {ex.Message}");
        }
    }

    private void RefreshDataGridView()
    {
        bindingSource1.DataSource = processes.GetAllProcesses().ToList();
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
                _activeProcess.Duration = (int)(DateTime.Now - _activeProcess.ProcessStart).TotalSeconds;
                processes.UpdateProcess(_activeProcess);
                logger.Log(LogLevel.Information, $"Оновлено тривалість процесу при виході: {_activeProcess.ProcessName} | {_activeProcess.Duration} сек.");
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
        syncNowToolStripMenuItem.Enabled = false;
        try
        {
            if (_activeProcess != null)
            {
                processes.UpdateProcess(_activeProcess);
            }
            await serverSyncService.SyncAsync();
            MessageBox.Show("Синхронізація завершена", "Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshDataGridView();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Manual sync failed");
            MessageBox.Show($"Помилка синхронізації: {ex.Message}", "Sync Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            syncNowToolStripMenuItem.Enabled = true;
        }
    }
}
