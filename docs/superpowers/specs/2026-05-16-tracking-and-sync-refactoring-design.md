# Design Document: Tracking and Synchronization Refactoring

**Date:** 2026-05-16
**Status:** Approved
**Topic:** Improving PC activity tracking, process duration monitoring, and server synchronization.

## 1. Executive Summary
This design addresses several inefficiencies and missing features in the WinFormsLogger application:
- Incorrect PC "PowerOn" time tracking.
- High database load due to per-second updates of process duration.
- Lack of automatic (timer-based) synchronization.
- Missing configuration UI for server settings.
- Implementation of Token-based (Bearer) authentication for REST API.

## 2. System Tracking (PC Activity)
### 2.1 PowerOn Tracking
- **Problem:** Current implementation logs the application start time as "PowerOn".
- **Solution:** Calculate OS boot time using `Environment.TickCount64`.
  - Formula: `DateTime bootTime = DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount64)`.
  - At application startup, the `SystemEventWatcher` will check if a `PowerOn` event for the current boot session already exists in the local database. If not, it will be created.
### 2.2 PowerOff Tracking
- **Logic:** Keep the logic in `OnSessionEnding`.
- **Improvement:** Ensure `IProcessRepository` and `IScheduleRepository` flush any pending changes to the SQLite database before the process terminates.

## 3. Process Duration Monitoring (Performance)
### 3.1 In-Memory Tracking
- The `Form1` class will maintain a reference to the currently active `Process` object in memory.
- The per-second timer will update the `Duration` property of this object only.
- The `DataGridView` will be refreshed visually from memory without database calls.
### 3.2 Batch Updates
- Active process duration will be persisted to the database only under the following conditions:
  - Every 5 minutes (via a background save timer).
  - Immediately before a server synchronization.
  - During application exit.
- This reduces SQLite write operations from 1/sec to roughly 1/300sec.

## 4. Identity and Authentication (REST API)
### 4.1 Device Identity
- Ensure `pc_id` is generated once per installation using `DeviceIdentityService` and stored in the `Config` table.
- Use this `pc_id` in all sync requests to identify the source device.
### 4.2 Token-Based Auth
- **Login:** `LoginForm` will be updated (when API is ready) to exchange credentials for a JWT/Bearer token.
- **Storage:** Tokens will be stored securely using `CredentialService`.
- **Sync:** `ServerSyncService` will include the token in the `Authorization: Bearer <token>` header and the `DeviceId` in a custom header (e.g., `X-Device-Id`).

## 5. Automation and Configuration
### 5.1 Application Settings
- Utilize `Properties.Settings.Default` to store:
  - `ServerUrl` (String, Default: Placeholder).
  - `SyncIntervalMinutes` (Int, Default: 10).
### 5.2 Sync Timer
- Implement a `System.Windows.Forms.Timer` in `Form1` that triggers `ServerSyncService.SyncAsync()` based on the `SyncIntervalMinutes` setting.
### 5.3 Settings UI
- Create `SettingsForm.cs` allowing users to:
  - Edit `ServerUrl`.
  - Edit `SyncIntervalMinutes`.
  - Save changes to `Properties.Settings`.

## 6. Implementation Plan Highlights
1.  **Phase 1:** Update `SystemEventWatcher` and `DeviceIdentity` initialization.
2.  **Phase 2:** Refactor `Form1` to use In-Memory tracking for active processes.
3.  **Phase 3:** Implement `Application Settings` and `SettingsForm`.
4.  **Phase 4:** Update `ServerSyncService` to include Auth headers and Device ID.
5.  **Phase 5:** Implement the automatic synchronization timer.
