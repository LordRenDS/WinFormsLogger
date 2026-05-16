# Tracking and Synchronization Refactoring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Improve PC activity tracking accuracy, optimize process monitoring performance, and implement automated token-based synchronization with configurable settings.

**Architecture:** 
- Use `Environment.TickCount64` for accurate OS boot time.
- Implement in-memory duration tracking with periodic batch updates to SQLite to reduce I/O.
- Use `ApplicationSettingsBase` for persistent configuration.
- Enhance `ServerSyncService` with Bearer token authentication and custom device headers.

**Tech Stack:** C#, WinForms, .NET 10, SQLite, Microsoft.Extensions.DependencyInjection, CredentialManagement.

---

### Task 1: Device Identity and System Boot Tracking

**Files:**
- Modify: `WinFormsLogger\Services\SystemEventWatcher.cs`
- Modify: `WinFormsLogger\Form1.cs`
- Modify: `WinFormsLogger\DB\Tables\IProcessRepository.cs`
- Modify: `WinFormsLogger\DB\Tables\ProcessesT.cs`

- [ ] **Step 1: Update IProcessRepository to include a check for existing PowerOn event**
Modify `IProcessRepository.cs` (or `IScheduleRepository.cs` since PowerOn is a Schedule event) to add a method that checks for a status in a specific time range.
Actually, let's check `IScheduleRepository.cs`.

- [ ] **Step 2: Implement the check in SchedulesT.cs**
```csharp
// Add to IScheduleRepository.cs
bool Exists(int statusId, DateTime timestamp, TimeSpan tolerance);

// Implement in SchedulesT.cs
public bool Exists(int statusId, DateTime timestamp, TimeSpan tolerance) {
    lock (_dataBase.DbLock) {
        string statement = "SELECT COUNT(*) FROM Schedules WHERE pc_status_id = @StatusId AND timestamp >= @Start AND timestamp <= @End";
        using var command = new SqliteCommand(statement, _dataBase.SqConn);
        command.Parameters.AddWithValue("@StatusId", statusId);
        command.Parameters.AddWithValue("@Start", timestamp - tolerance);
        command.Parameters.AddWithValue("@End", timestamp + tolerance);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
```

- [ ] **Step 3: Update SystemEventWatcher to use OS boot time**
Modify `SystemEventWatcher.cs`:
```csharp
public void Start() {
    _logger.LogInformation("SystemEventWatcher starting...");
    LoadStatusMap();

    SystemEvents.SessionSwitch += OnSessionSwitch;
    SystemEvents.SessionEnding += OnSessionEnding;
    
    // Calculate boot time
    DateTime bootTime = DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount64);
    
    if (_statusMap.TryGetValue("PowerOn", out int statusId)) {
        // Check if already logged (tolerance 5 minutes)
        if (!_scheduleRepository.Exists(statusId, bootTime, TimeSpan.FromMinutes(5))) {
            LogStatusAtTime("PowerOn", bootTime);
        }
    }
}

private void LogStatusAtTime(string statusName, DateTime time) {
    if (_statusMap.TryGetValue(statusName, out int statusId)) {
        var schedule = new Schedule {
            PcStatusId = statusId,
            Timestamp = time,
            IsSynced = false
        };
        _scheduleRepository.Create(schedule);
    }
}
```

- [ ] **Step 4: Initialize PC ID in Form1**
In `Form1_Load`, check `IConfigRepository`. If empty, save `deviceIdentityService.GetDeviceId()`.

- [ ] **Step 5: Commit**
`git add . && git commit -m "- implement accurate poweron tracking and pc id initialization"`

### Task 2: Optimized In-Memory Process Tracking

**Files:**
- Modify: `WinFormsLogger\Form1.cs`

- [ ] **Step 1: Add state variables for in-memory tracking**
Add `private Process? _activeProcess;` and `private System.Windows.Forms.Timer _dbSaveTimer;` to `Form1`.

- [ ] **Step 2: Update ActiveProcessTimer_Tick to only update memory**
```csharp
private void ActiveProcessTimer_Tick(object? sender, EventArgs e) {
    try {
        Process active = processTracer.GetActiveProcess();
        
        if (_activeProcess != null && 
            _activeProcess.ProcessName == active.ProcessName && 
            _activeProcess.WindowsName == active.WindowsName &&
            _activeProcess.ProcessStart == active.ProcessStart) 
        {
            // Just update duration in memory
            _activeProcess.Duration = (int)(DateTime.Now - _activeProcess.ProcessStart).TotalSeconds;
            // Update UI only for this row if possible, or refresh
            return;
        }

        // Process changed - save old one if exists
        if (_activeProcess != null) {
            processes.UpdateProcess(_activeProcess);
        }

        // Check if this instance exists in DB (e.g. after app restart)
        if (trackedInstances.TryGetValue(active.ProcessStart, out int existingId)) {
            active.Id = existingId;
            active.Duration = (int)(DateTime.Now - active.ProcessStart).TotalSeconds;
        } else {
            active.Duration = 0;
            active.Id = processes.CreateProcess(active);
            trackedInstances[active.ProcessStart] = active.Id;
        }

        _activeProcess = active;
        RefreshDataGridView();
    } catch (Exception) { /* ... */ }
}
```

- [ ] **Step 3: Implement Periodic Save Timer**
Initialize `_dbSaveTimer` in constructor (5 minutes). In its tick, call `processes.UpdateProcess(_activeProcess)` if not null.

- [ ] **Step 4: Commit**
`git add . && git commit -m "- optimize process tracking with in-memory duration updates"`

### Task 3: Application Settings and Configuration UI

**Files:**
- Create: `WinFormsLogger\Services\AppSettings.cs`
- Create: `WinFormsLogger\Forms\SettingsForm.cs`
- Modify: `WinFormsLogger\Form1.cs`
- Modify: `WinFormsLogger\Program.cs`

- [ ] **Step 1: Create AppSettings service**
Inherit from `ApplicationSettingsBase`. Define `ServerUrl` and `SyncInterval`.

- [ ] **Step 2: Create SettingsForm**
Design a simple form with `ServerUrl` (string) and `SyncInterval` (int) inputs.

- [ ] **Step 3: Register and inject AppSettings**
Add to `Program.cs` and `Form1.cs`.

- [ ] **Step 4: Implement Settings menu click**
Open `SettingsForm` and save settings on OK.

- [ ] **Step 5: Commit**
`git add . && git commit -m "- add application settings and settings configuration form"`

### Task 4: Automated Synchronization with Token Auth

**Files:**
- Modify: `WinFormsLogger\Services\ServerSyncService.cs`
- Modify: `WinFormsLogger\Form1.cs`

- [ ] **Step 1: Update ServerSyncService to use Token and DeviceId**
```csharp
public async Task SyncAsync() {
    var credentials = _credentialService.GetCredentials();
    string token = credentials?.password; // Assuming token is stored in password field
    string deviceId = _deviceIdentityService.GetDeviceId();
    
    // Add headers to HttpClient (will need to inject or create HttpClient)
    // For now, update the JSON package to include these as requested or headers
}
```

- [ ] **Step 2: Add Sync Timer to Form1**
Use `SyncInterval` from settings to trigger `serverSyncService.SyncAsync()`.

- [ ] **Step 3: Commit**
`git add . && git commit -m "- implement automated synchronization with token authentication"`
