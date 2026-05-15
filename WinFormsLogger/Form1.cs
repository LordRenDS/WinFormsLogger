using Microsoft.Extensions.Logging;
using WinFormsLogger.DB.Models;
using WinFormsLogger.DB.Tables;

namespace WinFormsLogger;

public partial class Form1 : Form
{
    private readonly ILogger<Form1> logger;
    private readonly IProcessRepository processes;
    private readonly IProcessTracer processTracer;
    private readonly Dictionary<DateTime, int> trackedInstances = new();

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

    private void ActiveProcessTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            Process activeProcess = processTracer.GetActiveProcess();

            // Перевіряємо, чи ми вже бачили цей екземпляр процесу
            if (trackedInstances.ContainsKey(activeProcess.ProcessStart))
            {
                return;
            }

            // Новий екземпляр процесу
            activeProcess.Duration = 0;
            activeProcess.Id = processes.CreateProcess(activeProcess);
            
            trackedInstances.Add(activeProcess.ProcessStart, activeProcess.Id);

            logger.Log(LogLevel.Information, $"Новий екземпляр процесу: {activeProcess.ProcessName} | {activeProcess.WindowsName} | Start: {activeProcess.ProcessStart}");
            RefreshDataGridView();
        }
        catch (Exception)
        {
            // logger.Log(LogLevel.Trace, $"Моніторинг: {ex.Message}");
        }
    }

    private void RefreshDataGridView()
    {
        bindingSource1.DataSource = processes.GetAllProcesses().ToList();
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
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
}
