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

    public static DB.Models.Process GetActiveProcess()
    {
        //try
        //{
        IntPtr foregroundWindow = GetForegroundWindow();
        int processId = 0;

        // Отримати ID процесу активного вікна
        GetWindowThreadProcessId(foregroundWindow, out processId);

        if (processId > 0)
        {
            Process process = Process.GetProcessById(processId);

            if (process.SessionId == 0) {
                throw new Exception("The process is running in the system session");
            }

            string windowTitle = GetWindowTitle(foregroundWindow);

            // Створити новий запис про процес
            var newProcess = new DB.Models.Process
            {
                ProcessName = process.ProcessName,
                WindowsName = windowTitle,
                ProcessStart = process.StartTime
            };

            return newProcess;
        }
        throw new Exception("Process ID is invalid.");
        //}
        //catch (Exception ex)
        //{
        //    // Обробка помилок
        //}
    }

    private static string GetWindowTitle(IntPtr hWnd)
    {
        const int nChars = 256;
        System.Text.StringBuilder buff = new System.Text.StringBuilder(nChars);
        GetWindowText(hWnd, buff, nChars);
        return buff.ToString();
    }
}
