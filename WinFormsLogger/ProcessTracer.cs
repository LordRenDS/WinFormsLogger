using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinFormsLogger;

internal class ProcessTracer
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    private static readonly string[] SystemUIProcesses =
{
    "StartMenuExperienceHost", // Меню Пуск
    "SearchHost",              // Пошук Windows
    "ShellExperienceHost",     // Центр сповіщень, меню Wi-Fi/звуку
    "LockApp",                 // Екран блокування
    "TextInputHost",           // Панель емодзі / сенсорна клавіатура
    "ApplicationFrameHost",     // Рамки системних вікон
    "ShellHost" // Системні елементи оболонки
};

    public static DB.Models.Process GetActiveProcess()
    {
        IntPtr foregroundWindow = GetForegroundWindow();
        int processId = 0;

        // Отримати ID процесу активного вікна
        GetWindowThreadProcessId(foregroundWindow, out processId);


        if (processId > 0)
        {
            Process process = Process.GetProcessById(processId);
            string windowTitle = GetWindowTitle(foregroundWindow);

            if (string.IsNullOrWhiteSpace(windowTitle))
                throw new Exception($"The window of process {process.ProcessName}|{windowTitle} has no title (probably a system element)");

            int currentSessionId = Process.GetCurrentProcess().SessionId;
            if (process.SessionId != currentSessionId)
                throw new Exception($"Process {process.ProcessName}|{windowTitle} does not belong to the current user's session.");

            if (SystemUIProcesses.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                throw new Exception($"Process {process.ProcessName}|{windowTitle} is part of the Windows system interface");

            if (process.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase) &&
        windowTitle == "Program Manager")
                throw new Exception("The Desktop is active");

            // Створити новий запис про процес
            var newProcess = new DB.Models.Process
            {
                ProcessName = process.ProcessName,
                WindowsName = windowTitle,
                ProcessStart = process.StartTime
            };

            return newProcess;
        }
        else
            throw new Exception("Process ID is invalid.");
    }

    private static string GetWindowTitle(IntPtr hWnd)
    {
        const int nChars = 256;
        System.Text.StringBuilder buff = new System.Text.StringBuilder(nChars);
        GetWindowText(hWnd, buff, nChars);
        return buff.ToString();
    }
}
