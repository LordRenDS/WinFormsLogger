# UI Responsiveness in WinForms

## The Golden Rule
**Never block the UI thread.** Any operation lasting longer than ~50ms should be offloaded to a background thread to prevent the application from becoming "Not Responding".

## Async/Await (Recommended)
Use `Task.Run` for CPU-bound tasks and `await` for I/O-bound tasks.
```csharp
private async void btnProcess_Click(object sender, EventArgs e)
{
    btnProcess.Enabled = false;
    lblStatus.Text = "Processing...";
    
    try
    {
        await Task.Run(() => LongRunningOperation());
        lblStatus.Text = "Completed!";
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}");
    }
    finally
    {
        btnProcess.Enabled = true;
    }
}
```

## UI Updates from Background Threads
Controls can only be updated from the thread that created them. Use `Control.Invoke` or `Control.BeginInvoke` if not using `async/await`.
```csharp
void UpdateProgress(int value)
{
    if (progressBar.InvokeRequired)
    {
        progressBar.Invoke(new Action(() => progressBar.Value = value));
    }
    else
    {
        progressBar.Value = value;
    }
}
```

## Progress Reporting
Use `IProgress<T>` for progress updates in asynchronous methods.
```csharp
public async Task DoWorkAsync(IProgress<int> progress)
{
    for (int i = 0; i <= 100; i++)
    {
        await Task.Delay(100); // Simulate work
        progress.Report(i);
    }
}
```

## BackgroundWorker (Legacy)
Useful for older .NET versions or specific event-driven background tasks. Prefer `Task` and `async/await` for modern development.
