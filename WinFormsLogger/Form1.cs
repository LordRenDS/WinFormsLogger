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
    private readonly Dictionary<DateTime, int> trackedInstances = new();
    private bool _isExiting = false;

    public Form1(ILogger<Form1> logger, IProcessRepository processes, IProcessTracer processTracer, ICredentialService credentialService)
    {
        InitializeComponent();
        this.logger = logger;
        this.processes = processes;
        this.processTracer = processTracer;
        this.credentialService = credentialService;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        logger.Log(LogLevel.Information, "Form1_Load");

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

    private void ActiveProcessTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            Process activeProcess = processTracer.GetActiveProcess();

            if (trackedInstances.TryGetValue(activeProcess.ProcessStart, out int existingId))
            {
                // Процес уже відстежується. Оновлюємо його тривалість.
                activeProcess.Id = existingId;
                activeProcess.Duration = (int)(DateTime.Now - activeProcess.ProcessStart).TotalSeconds;
                processes.UpdateProcess(activeProcess);
                return;
            }

            // Це новий запуск процесу - створюємо запис
            activeProcess.Duration = 0;
            activeProcess.Id = processes.CreateProcess(activeProcess);
            trackedInstances[activeProcess.ProcessStart] = activeProcess.Id;

            logger.Log(LogLevel.Information, $"Зафіксовано новий запуск: {activeProcess.ProcessName}");
            RefreshDataGridView();
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, $"Помилка при моніторингу: {ex.Message}");
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

        try
        {
            Process activeProcess = processTracer.GetActiveProcess();
            if (trackedInstances.TryGetValue(activeProcess.ProcessStart, out int processId))
            {
                var processToUpdate = processes.GetProcessById(processId);
                if (processToUpdate != null)
                {
                    processToUpdate.Duration = (int)(DateTime.Now - processToUpdate.ProcessStart).TotalSeconds;
                    processes.UpdateProcess(processToUpdate);
                    logger.Log(LogLevel.Information, $"Оновлено тривалість процесу при виході: {processToUpdate.ProcessName} | {processToUpdate.Duration} сек.");
                }
            }
        }
        catch (Exception)
        {
            // ignore
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
}
