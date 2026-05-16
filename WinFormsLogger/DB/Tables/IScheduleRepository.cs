using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

public interface IScheduleRepository
{
    IEnumerable<Schedule> GetAll();
    Schedule? GetById(int id);
    int Create(Schedule schedule);
    int Update(Schedule schedule);
    int Delete(int id);
    bool Exists(int statusId, DateTime timestamp, TimeSpan tolerance);
}
