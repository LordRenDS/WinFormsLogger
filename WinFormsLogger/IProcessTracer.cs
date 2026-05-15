using WinFormsLogger.DB.Models;

namespace WinFormsLogger;

public interface IProcessTracer
{
    Process GetActiveProcess();
}
