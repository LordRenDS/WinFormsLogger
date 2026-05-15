namespace WinFormsLogger.DB.Models;

public class Schedule
{
    public int Id { get; set; }
    public int PcStatusId { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSynced { get; set; }
}
