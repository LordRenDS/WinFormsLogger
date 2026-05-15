using Microsoft.Data.Sqlite;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

internal class ProcessesT : IProcessRepository
{
    private readonly DataBaseMSQ _dataBase;

    public ProcessesT(DataBaseMSQ dataBase)
    {
        _dataBase = dataBase;
    }

    public IEnumerable<Process> GetAllProcesses()
    {
        lock (_dataBase.DbLock)
        {
            var processes = new List<Process>();
            using var command = new SqliteCommand("SELECT * FROM Processes", _dataBase.SqConn);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                processes.Add(new Process
                {
                    Id = reader.GetInt32(0),
                    ProcessName = reader.GetString(1),
                    WindowsName = reader.GetString(2),
                    ProcessStart = reader.GetDateTime(3),
                    Duration = reader.GetInt32(4),
                    IsSynced = reader.GetInt32(5) == 1
                });
            }
            return processes;
        }
    }

    public Process? GetProcessById(int id)
    {
        lock (_dataBase.DbLock)
        {
            using var command = new SqliteCommand("SELECT * FROM Processes WHERE Id = @Id", _dataBase.SqConn);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Process
                {
                    Id = reader.GetInt32(0),
                    ProcessName = reader.GetString(1),
                    WindowsName = reader.GetString(2),
                    ProcessStart = reader.GetDateTime(3),
                    Duration = reader.GetInt32(4),
                    IsSynced = reader.GetInt32(5) == 1
                };
            }
            return null;
        }
    }

    public IEnumerable<Process> GetProcessesByDate(DateOnly date)
    {
        lock (_dataBase.DbLock)
        {
            using var command = new SqliteCommand("SELECT * FROM Processes WHERE date(process_start) = @Date", _dataBase.SqConn);
            command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
            using var reader = command.ExecuteReader();
            var processes = new List<Process>();
            while (reader.Read())
            {
                processes.Add(new Process
                {
                    Id = reader.GetInt32(0),
                    ProcessName = reader.GetString(1),
                    WindowsName = reader.GetString(2),
                    ProcessStart = reader.GetDateTime(3),
                    Duration = reader.GetInt32(4),
                    IsSynced = reader.GetInt32(5) == 1
                });
            }
            return processes;
        }
    }

    public int UpdateProcess(Process process)
    {
        lock (_dataBase.DbLock)
        {
            string statement = "UPDATE Processes SET process_name = @ProcessName, windows_name = @WindowsName, process_start = @ProcessStart, duration = @Duration, is_synced = @IsSynced WHERE Id = @Id";
            using var command = new SqliteCommand(statement, _dataBase.SqConn);
            command.Parameters.AddWithValue("@ProcessName", process.ProcessName);
            command.Parameters.AddWithValue("@WindowsName", process.WindowsName);
            command.Parameters.AddWithValue("@ProcessStart", process.ProcessStart);
            command.Parameters.AddWithValue("@Duration", process.Duration);
            command.Parameters.AddWithValue("@IsSynced", process.IsSynced ? 1 : 0);
            command.Parameters.AddWithValue("@Id", process.Id);
            return command.ExecuteNonQuery();
        }
    }

    public int CreateProcess(Process process)
    {
        lock (_dataBase.DbLock)
        {
            string statement = "INSERT INTO Processes (process_name, windows_name, process_start, duration, is_synced) VALUES (@ProcessName, @WindowsName, @ProcessStart, @Duration, @IsSynced); SELECT last_insert_rowid();";
            using var command = new SqliteCommand(statement, _dataBase.SqConn);
            command.Parameters.AddWithValue("@ProcessName", process.ProcessName);
            command.Parameters.AddWithValue("@WindowsName", process.WindowsName);
            command.Parameters.AddWithValue("@ProcessStart", process.ProcessStart);
            command.Parameters.AddWithValue("@Duration", process.Duration);
            command.Parameters.AddWithValue("@IsSynced", process.IsSynced ? 1 : 0);
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }

    public int DeleteProcess(int id)
    {
        lock (_dataBase.DbLock)
        {
            using var command = new SqliteCommand("DELETE FROM Processes WHERE Id = @Id", _dataBase.SqConn);
            command.Parameters.AddWithValue("@Id", id);
            return command.ExecuteNonQuery();
        }
    }
}
