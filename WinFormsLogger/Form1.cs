using Microsoft.Extensions.Logging;
using WinFormsLogger.DB.Models;
using WinFormsLogger.DB.Tables;

namespace WinFormsLogger;

public partial class Form1 : Form
{
    private readonly ILogger<Form1> logger;
    private readonly IProcessRepository processes;
    private readonly IProcessTracer processTracer;
    private Dictionary<DateTimeOffset, string> trackedProcesses = new();

    public Form1(ILogger<Form1> logger, IProcessRepository processes, IProcessTracer processTracer)
    {
        InitializeComponent();
        this.logger = logger;
        this.processes = processes;
        this.processTracer = processTracer;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        logger.Log(LogLevel.Information, "Form1_Load");

        bindingSource1.DataSource = processes.GetAllProcesses().ToList();
        dataGridView1.DataSource = bindingSource1;
        LoadTrackedProcessesForToday();

        activeProcessTimer.Start();
    }

    private void ActiveProcessTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            Process activeProcess = processTracer.GetActiveProcess();
            DateTimeOffset processKey = activeProcess.ProcessStart;

            if (trackedProcesses.ContainsKey(activeProcess.ProcessStart))
            {
                logger.Log(LogLevel.Debug, $"Активний процес (існуючий): {activeProcess.ProcessName}");
                return;
            }

            // Новий процес - зберегти в БД
            processes.CreateProcess(activeProcess);
            trackedProcesses[processKey] = $"{activeProcess.ProcessName}|{activeProcess.WindowsName}";

            logger.Log(LogLevel.Information, $"Новий процес добавлений: {activeProcess.ProcessName}");
            RefreshDataGridView();
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, $"Помилка при моніторингу процесів: {ex.Message}");
        }
    }

    private void LoadTrackedProcessesForToday()
    {
        trackedProcesses.Clear();
        var allProcesses = processes.GetProcessesByDate(DateOnly.FromDateTime(DateTime.Now)).ToList();

        foreach (var process in allProcesses)
            trackedProcesses.Add(process.ProcessStart, $"{process.ProcessName}|{process.WindowsName}");

        logger.Log(LogLevel.Information, $"Завантажено {trackedProcesses.Count} відстежених процесів за сьогодні");
    }
    private void RefreshDataGridView()
    {
        bindingSource1.DataSource = processes.GetAllProcesses().ToList();
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        activeProcessTimer?.Stop();
    }
}
