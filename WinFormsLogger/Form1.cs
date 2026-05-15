using Microsoft.Extensions.Logging;
using WinFormsLogger.DB.Models;
using WinFormsLogger.DB.Tables;

namespace WinFormsLogger;

public partial class Form1 : Form
{
    private readonly ILogger<Form1> logger;
    private readonly IProcessRepository processes;
    private readonly IProcessTracer processTracer;
    private Process? lastActiveProcess = null;
    private DateTime lastActiveTime;

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

        activeProcessTimer.Start();
    }

    private void ActiveProcessTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            Process activeProcess = processTracer.GetActiveProcess();

            if (lastActiveProcess != null && 
                lastActiveProcess.ProcessName == activeProcess.ProcessName && 
                lastActiveProcess.WindowsName == activeProcess.WindowsName)
            {
                // Вікно не змінилося, просто чекаємо далі
                return;
            }

            // Якщо вікно змінилося, розраховуємо тривалість попереднього
            if (lastActiveProcess != null)
            {
                lastActiveProcess.Duration = (int)(DateTime.Now - lastActiveTime).TotalSeconds;
                processes.UpdateProcess(lastActiveProcess);
            }

            // Записуємо новий процес
            activeProcess.Duration = 0;
            activeProcess.Id = processes.CreateProcess(activeProcess);
            
            lastActiveProcess = activeProcess;
            lastActiveTime = DateTime.Now;

            logger.Log(LogLevel.Information, $"Новий активний процес: {activeProcess.ProcessName} | {activeProcess.WindowsName}");
            RefreshDataGridView();
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, $"Помилка при моніторингу процесів: {ex.Message}");
        }
    }

    private void RefreshDataGridView()
    {
        bindingSource1.DataSource = processes.GetAllProcesses().ToList();
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        activeProcessTimer?.Stop();

        if (lastActiveProcess != null)
        {
            lastActiveProcess.Duration = (int)(DateTime.Now - lastActiveTime).TotalSeconds;
            processes.UpdateProcess(lastActiveProcess);
        }
    }
}
