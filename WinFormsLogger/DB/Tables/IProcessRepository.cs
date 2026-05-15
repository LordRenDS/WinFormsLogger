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
