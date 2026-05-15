# WinFormsLogger Refactoring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor the WinFormsLogger application to follow C# best practices (Clean Architecture, DI, SOLID) and properly align user code with auto-generated code in Form1.

**Architecture:** We will introduce Dependency Injection via `Microsoft.Extensions.DependencyInjection`. Database access will be refactored into a proper Repository pattern with `using` statements for `IDisposable` resources. Hard-coded UI components in `Form1.cs` will be moved to `Form1.Designer.cs`.

**Tech Stack:** C# 12/10.0, WinForms, SQLite, Microsoft.Extensions.DependencyInjection.

---

### Task 1: Setup Dependency Injection

**Files:**
- Modify: `WinFormsLogger/WinFormsLogger.csproj`

- [ ] **Step 1: Add Microsoft.Extensions.DependencyInjection package**

Run: `dotnet add WinFormsLogger/WinFormsLogger.csproj package Microsoft.Extensions.DependencyInjection -v 10.0.7`
Expected: PASS (Package added successfully)

### Task 2: Refactor Database Layer

**Files:**
- Create: `WinFormsLogger/DB/Tables/IProcessRepository.cs`
- Modify: `WinFormsLogger/DB/Tables/ProcessesT.cs`
- Modify: `WinFormsLogger/DB/DataBaseMSQ.cs`

- [ ] **Step 1: Create IProcessRepository interface**

```csharp
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

public interface IProcessRepository
{
    IEnumerable<Process> GetAllProcesses();
    Process? GetProcessById(int id);
    IEnumerable<Process> GetProcessesByDate(DateOnly date);
    int UpdateProcess(Process process);
    int CreateProcess(Process process);
    int DeleteProcess(int id);
}
```

- [ ] **Step 2: Refactor ProcessesT to implement IProcessRepository and use proper 'using' blocks**

```csharp
using Microsoft.Data.Sqlite;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

internal class ProcessesT : IProcessRepository
{
    private readonly DataBaseMSQ _dataBase;

    public ProcessesT(DataBaseMSQ dataBase)
    {
        _dataBase = dataBase;
    }

    public IEnumerable<Process> GetAllProcesses()
    {
        var processes = new List<Process>();
        using var command = new SqliteCommand("SELECT * FROM Processes", _dataBase.SqConn);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            processes.Add(new Process
            {
                Id = reader.GetInt32(0),
                ProcessName = reader.GetString(1),
                WindowsName = reader.GetString(2),
                ProcessStart = reader.GetDateTime(3)
            });
        }
        return processes;
    }

    public Process? GetProcessById(int id)
    {
        using var command = new SqliteCommand("SELECT * FROM Processes WHERE Id = @Id", _dataBase.SqConn);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Process
            {
                Id = reader.GetInt32(0),
                ProcessName = reader.GetString(1),
                WindowsName = reader.GetString(2),
                ProcessStart = reader.GetDateTime(3)
            };
        }
        return null;
    }

    public IEnumerable<Process> GetProcessesByDate(DateOnly date)
    {
        using var command = new SqliteCommand("SELECT * FROM Processes WHERE date(process_start) = @Date", _dataBase.SqConn);
        command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
        using var reader = command.ExecuteReader();
        var processes = new List<Process>();
        while (reader.Read())
        {
            processes.Add(new Process
            {
                Id = reader.GetInt32(0),
                ProcessName = reader.GetString(1),
                WindowsName = reader.GetString(2),
                ProcessStart = reader.GetDateTime(3)
            });
        }
        return processes;
    }

    public int UpdateProcess(Process process)
    {
        // Fixed trailing comma before WHERE
        string statement = "UPDATE Processes SET process_name = @ProcessName, windows_name = @WindowsName, process_start = @ProcessStart WHERE Id = @Id";
        using var command = new SqliteCommand(statement, _dataBase.SqConn);
        command.Parameters.AddWithValue("@ProcessName", process.ProcessName);
        command.Parameters.AddWithValue("@WindowsName", process.WindowsName);
        command.Parameters.AddWithValue("@ProcessStart", process.ProcessStart);
        command.Parameters.AddWithValue("@Id", process.Id);
        return command.ExecuteNonQuery();
    }

    public int CreateProcess(Process process)
    {
        string statement = "INSERT INTO Processes (process_name, windows_name, process_start) VALUES (@ProcessName, @WindowsName, @ProcessStart)";
        using var command = new SqliteCommand(statement, _dataBase.SqConn);
        command.Parameters.AddWithValue("@ProcessName", process.ProcessName);
        command.Parameters.AddWithValue("@WindowsName", process.WindowsName);
        command.Parameters.AddWithValue("@ProcessStart", process.ProcessStart);
        return command.ExecuteNonQuery();
    }

    public int DeleteProcess(int id)
    {
        using var command = new SqliteCommand("DELETE FROM Processes WHERE Id = @Id", _dataBase.SqConn);
        command.Parameters.AddWithValue("@Id", id);
        return command.ExecuteNonQuery();
    }
}
```

- [ ] **Step 3: Modify DataBaseMSQ to be dependency-injection friendly**

```csharp
using Microsoft.Data.Sqlite;

namespace WinFormsLogger;

public class DataBaseMSQ : IDisposable
{
    public SqliteConnection SqConn { get; private set; }

    public DataBaseMSQ()
    {
        CreateConnection();
        CreateTables();
    }

    public void Dispose()
    {
        if (SqConn != null)
        {
            if (SqConn.State == System.Data.ConnectionState.Open)
            {
                SqConn.Close();
            }
            SqConn.Dispose();
        }
    }

    private static string GetDbPath()
    {
        string appPath = LoggerUtils.GetAppPath();
        return Path.Combine(appPath, "logger.db");
    }

    private void CreateConnection()
    {
        SqliteConnectionStringBuilder csb = new SqliteConnectionStringBuilder();
        string dbPath = GetDbPath();
        csb.DataSource = dbPath;
        csb.ForeignKeys = true;
        csb.RecursiveTriggers = true;
        this.SqConn = new SqliteConnection(csb.ToString());
        this.SqConn.Open();
    }

    private void CreateTables()
    {
        ExecuteQuery(TableStatement.PcStatusT);
        ExecuteQuery(TableStatement.ProcessesT);
        ExecuteQuery(TableStatement.SchedulesT);
    }

    private void ExecuteQuery(string statement)
    {
        using SqliteCommand command = new SqliteCommand(statement, this.SqConn);
        command.ExecuteNonQuery();
    }
}

file static class TableStatement
{
    public static readonly string ProcessesT = """
        CREATE TABLE IF NOT EXISTS Processes (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            process_name TEXT NOT NULL,
            windows_name TEXT NOT NULL,
            process_start TIMESTAMP NOT NULL
        );
        """;
    public static readonly string SchedulesT = """
        CREATE TABLE IF NOT EXISTS Schedules (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            pc_status_id INTEGER REFERENCES PcStatus(Id) ON DELETE CASCADE ON UPDATE CASCADE,
            action_time TIMESTAMP NOT NULL
        );
        """;
    public static readonly string PcStatusT = """
        CREATE TABLE IF NOT EXISTS PcStatus (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            status TEXT NOT NULL
        );
        """;
}
```

### Task 3: Refactor ProcessTracer

**Files:**
- Create: `WinFormsLogger/IProcessTracer.cs`
- Modify: `WinFormsLogger/ProcessTracer.cs`

- [ ] **Step 1: Extract interface**

```csharp
using WinFormsLogger.DB.Models;

namespace WinFormsLogger;

public interface IProcessTracer
{
    Process GetActiveProcess();
}
```

- [ ] **Step 2: Update ProcessTracer to implement interface and avoid static methods**

```csharp
using System.Diagnostics;
using System.Runtime.InteropServices;
using Process = WinFormsLogger.DB.Models.Process;

namespace WinFormsLogger;

internal class ProcessTracer : IProcessTracer
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

    private static readonly string[] SystemUIProcesses =
    {
        "StartMenuExperienceHost", "SearchHost", "ShellExperienceHost",
        "LockApp", "TextInputHost", "ApplicationFrameHost", "ShellHost"
    };

    public Process GetActiveProcess()
    {
        IntPtr foregroundWindow = GetForegroundWindow();
        GetWindowThreadProcessId(foregroundWindow, out int processId);

        if (processId > 0)
        {
            var sysProcess = System.Diagnostics.Process.GetProcessById(processId);
            string windowTitle = GetWindowTitle(foregroundWindow);
            string className = GetWindowClassName(foregroundWindow);

            if (string.IsNullOrWhiteSpace(windowTitle))
                throw new Exception($"The window of process {sysProcess.ProcessName} has no title (probably a system element)");

            int currentSessionId = System.Diagnostics.Process.GetCurrentProcess().SessionId;
            if (sysProcess.SessionId != currentSessionId)
                throw new Exception($"Process {sysProcess.ProcessName}|{windowTitle} does not belong to the current user's session.");

            if (SystemUIProcesses.Contains(sysProcess.ProcessName, StringComparer.OrdinalIgnoreCase))
                throw new Exception($"Process {sysProcess.ProcessName}|{windowTitle} is part of the Windows system interface");

            if (sysProcess.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
                if (!className.Equals("CabinetWClass", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("The Desktop is active");

            return new Process
            {
                ProcessName = sysProcess.ProcessName,
                WindowsName = windowTitle,
                ProcessStart = sysProcess.StartTime
            };
        }
        
        throw new Exception("Process ID is invalid.");
    }

    private string GetWindowTitle(IntPtr hWnd)
    {
        const int nChars = 256;
        var buff = new System.Text.StringBuilder(nChars);
        GetWindowText(hWnd, buff, nChars);
        return buff.ToString();
    }

    private string GetWindowClassName(IntPtr hWnd)
    {
        const int nChars = 256;
        var buff = new System.Text.StringBuilder(nChars);
        GetClassName(hWnd, buff, nChars);
        return buff.ToString();
    }
}
```

### Task 4: Align UI Components in Form1

**Files:**
- Modify: `WinFormsLogger/Form1.Designer.cs`
- Modify: `WinFormsLogger/Form1.cs`

- [ ] **Step 1: Move component declarations to Form1.Designer.cs**

Open `WinFormsLogger/Form1.Designer.cs`. Find the `#endregion` for the generated code. Add the timer and binding source declarations at the bottom of the file (before `}`):

```csharp
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.Timer activeProcessTimer;
```

In `InitializeComponent()` inside `Form1.Designer.cs`, replace:
```csharp
            components = new System.ComponentModel.Container();
            bindingSource1 = new BindingSource(components);
            activeProcessTimer = new System.Windows.Forms.Timer(components);
```
right after `InitializeComponent()` opening brace. Then configure them:
```csharp
            // 
            // activeProcessTimer
            // 
            activeProcessTimer.Interval = 1000;
            activeProcessTimer.Tick += ActiveProcessTimer_Tick;
```
Ensure they are completely removed from `Form1.cs` fields.

- [ ] **Step 2: Refactor Form1.cs to use DI and remove manual UI component initialization**

```csharp
using Microsoft.Extensions.Logging;
using WinFormsLogger.DB.Models;
using WinFormsLogger.DB.Tables;

namespace WinFormsLogger;

public partial class Form1 : Form
{
    private readonly ILogger<Form1> _logger;
    private readonly IProcessRepository _processRepository;
    private readonly IProcessTracer _processTracer;
    private readonly Dictionary<DateTimeOffset, string> _trackedProcesses = new();

    public Form1(ILogger<Form1> logger, IProcessRepository processRepository, IProcessTracer processTracer)
    {
        InitializeComponent();
        _logger = logger;
        _processRepository = processRepository;
        _processTracer = processTracer;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        _logger.LogInformation("Form1_Load");
        
        RefreshDataGridView();
        LoadTrackedProcessesForToday();

        activeProcessTimer.Start();
    }

    private void ActiveProcessTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            Process activeProcess = _processTracer.GetActiveProcess();
            DateTimeOffset processKey = activeProcess.ProcessStart;

            if (_trackedProcesses.ContainsKey(processKey))
            {
                _logger.LogDebug($"Активний процес (існуючий): {activeProcess.ProcessName}");
                return;
            }

            _processRepository.CreateProcess(activeProcess);
            _trackedProcesses[processKey] = $"{activeProcess.ProcessName}|{activeProcess.WindowsName}";

            _logger.LogInformation($"Новий процес добавлений: {activeProcess.ProcessName}");
            RefreshDataGridView();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Помилка при моніторингу процесів: {ex.Message}");
        }
    }

    private void LoadTrackedProcessesForToday()
    {
        _trackedProcesses.Clear();
        var allProcesses = _processRepository.GetProcessesByDate(DateOnly.FromDateTime(DateTime.Now));

        foreach (var process in allProcesses)
            _trackedProcesses.TryAdd(process.ProcessStart, $"{process.ProcessName}|{process.WindowsName}");

        _logger.LogInformation($"Завантажено {_trackedProcesses.Count} відстежених процесів за сьогодні");
    }

    private void RefreshDataGridView()
    {
        bindingSource1.DataSource = _processRepository.GetAllProcesses().ToList();
        dataGridView1.DataSource = bindingSource1;
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        activeProcessTimer?.Stop();
    }
}
```

### Task 5: Configure DI in Program.cs

**Files:**
- Modify: `WinFormsLogger/Program.cs`

- [ ] **Step 1: Setup ServiceCollection and run Form1**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WinFormsLogger.DB.Tables;

namespace WinFormsLogger;

internal static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; }

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        var form1 = ServiceProvider.GetRequiredService<Form1>();
        Application.Run(form1);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddFileLogger(configure => { });
            builder.AddConsole();
        });

        services.AddSingleton<DataBaseMSQ>();
        services.AddTransient<IProcessRepository, ProcessesT>();
        services.AddTransient<IProcessTracer, ProcessTracer>();
        services.AddTransient<Form1>();
    }
}
```

- [ ] **Step 2: Verify Build**

Run: `dotnet build WinFormsLogger/WinFormsLogger.csproj`
Expected: Build succeeds with no errors.
