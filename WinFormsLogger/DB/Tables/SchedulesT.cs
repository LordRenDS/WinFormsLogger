using Microsoft.Data.Sqlite;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

internal class SchedulesT : IScheduleRepository
{
    private readonly DataBaseMSQ _dataBase;

    public SchedulesT(DataBaseMSQ dataBase)
    {
        _dataBase = dataBase;
    }

    public IEnumerable<Schedule> GetAll()
    {
        lock (_dataBase.DbLock)
        {
            var schedules = new List<Schedule>();
            using var command = new SqliteCommand("SELECT id, pc_status_id, timestamp, is_synced FROM Schedules", _dataBase.SqConn);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                schedules.Add(new Schedule
                {
                    Id = reader.GetInt32(0),
                    PcStatusId = reader.GetInt32(1),
                    Timestamp = reader.GetDateTime(2),
                    IsSynced = reader.GetInt32(3) == 1
                });
            }
            return schedules;
        }
    }

    public Schedule? GetById(int id)
    {
        lock (_dataBase.DbLock)
        {
            using var command = new SqliteCommand("SELECT id, pc_status_id, timestamp, is_synced FROM Schedules WHERE id = @Id", _dataBase.SqConn);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Schedule
                {
                    Id = reader.GetInt32(0),
                    PcStatusId = reader.GetInt32(1),
                    Timestamp = reader.GetDateTime(2),
                    IsSynced = reader.GetInt32(3) == 1
                };
            }
            return null;
        }
    }

    public int Create(Schedule schedule)
    {
        lock (_dataBase.DbLock)
        {
            string statement = "INSERT INTO Schedules (pc_status_id, timestamp, is_synced) VALUES (@PcStatusId, @Timestamp, @IsSynced)";
            using var command = new SqliteCommand(statement, _dataBase.SqConn);
            command.Parameters.AddWithValue("@PcStatusId", schedule.PcStatusId);
            command.Parameters.AddWithValue("@Timestamp", schedule.Timestamp);
            command.Parameters.AddWithValue("@IsSynced", schedule.IsSynced ? 1 : 0);
            return command.ExecuteNonQuery();
        }
    }

    public int Update(Schedule schedule)
    {
        lock (_dataBase.DbLock)
        {
            string statement = "UPDATE Schedules SET pc_status_id = @PcStatusId, timestamp = @Timestamp, is_synced = @IsSynced WHERE id = @Id";
            using var command = new SqliteCommand(statement, _dataBase.SqConn);
            command.Parameters.AddWithValue("@PcStatusId", schedule.PcStatusId);
            command.Parameters.AddWithValue("@Timestamp", schedule.Timestamp);
            command.Parameters.AddWithValue("@IsSynced", schedule.IsSynced ? 1 : 0);
            command.Parameters.AddWithValue("@Id", schedule.Id);
            return command.ExecuteNonQuery();
        }
    }

    public int Delete(int id)
    {
        lock (_dataBase.DbLock)
        {
            using var command = new SqliteCommand("DELETE FROM Schedules WHERE id = @Id", _dataBase.SqConn);
            command.Parameters.AddWithValue("@Id", id);
            return command.ExecuteNonQuery();
        }
    }
}
