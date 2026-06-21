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

            if (processId == System.Diagnostics.Process.GetCurrentProcess().Id)
                throw new Exception("The application itself is active (tracking ignored)");

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
        return buff.ToString().Trim();
    }

    private string GetWindowClassName(IntPtr hWnd)
    {
        const int nChars = 256;
        var buff = new System.Text.StringBuilder(nChars);
        GetClassName(hWnd, buff, nChars);
        return buff.ToString();
    }
}
