namespace WinFormsLogger.DB.Models;

public class Process
{
    public int Id { get; set; }
    public string ProcessName { get; set; }
    public string WindowsName { get; set; }
    public DateTime ProcessStart { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, ProcessName: {ProcessName}, WindowsName: {WindowsName}, ProcessStart: {ProcessStart}";
    }
}
