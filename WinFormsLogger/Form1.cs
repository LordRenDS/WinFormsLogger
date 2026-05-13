using Microsoft.Extensions.Logging;
using WinFormsLogger.DB.Models;
using WinFormsLogger.DB.Tables;

namespace WinFormsLogger;

public partial class Form1 : Form
{
    private BindingSource bindingSource1 = new BindingSource();
    private System.Windows.Forms.Timer activeProcessTimer = new System.Windows.Forms.Timer();
    private ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddFileLogger(configure => { });
        builder.AddConsole();
    });
    private ILogger<Form> logger;
    private ProcessesT processes;
    //private Dictionary<string, DateTimeOffset> trackedProcesses = new();
    private Dictionary<DateTimeOffset, string> trackedProcesses = new();
    private DataBaseMSQ db;

    public Form1()
    {
        InitializeComponent();
        this.logger = loggerFactory.CreateLogger<Form1>();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        logger.Log(LogLevel.Information, "Form1_Load");

        db = new DataBaseMSQ();
        db.SqConn.Open();
        processes = new ProcessesT(db.SqConn);
        bindingSource1.DataSource = processes.Processes;
        dataGridView1.DataSource = bindingSource1;
        LoadTrackedProcessesForToday();

        activeProcessTimer.Interval = 1000; // Перевіряти кожну секунду
        activeProcessTimer.Tick += ActiveProcessTimer_Tick;
        activeProcessTimer.Start();
    }

    private void ActiveProcessTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            Process activeProcess = ProcessTracer.GetActiveProcess();
            DateTimeOffset processKey = activeProcess.ProcessStart;

            if (trackedProcesses.ContainsKey(activeProcess.ProcessStart))
            {
                logger.Log(LogLevel.Debug, $"Активний процес (існуючий): {activeProcess.ProcessName}");
                return;
            }

            // Новий процес - зберегти в БД
            processes.CreateProcesses(activeProcess);
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
        List<Process> allProcesses = processes.GetProcessesByDate(DateOnly.FromDateTime(DateTime.Now));

        foreach (var process in allProcesses)
            trackedProcesses.Add(process.ProcessStart, $"{process.ProcessName}|{process.WindowsName}");

        logger.Log(LogLevel.Information, $"Завантажено {trackedProcesses.Count} відстежених процесів за сьогодні");
    }
    private void RefreshDataGridView()
    {
        bindingSource1.DataSource = processes.Processes;
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        activeProcessTimer?.Stop();
        activeProcessTimer?.Dispose();
        db?.SqConn?.Close();
        db?.Dispose();
        loggerFactory?.Dispose();
    }
}