using Microsoft.Data.Sqlite;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

internal class ProcessesT(SqliteConnection sqliteConnection)
{
    public List<Process> Processes
    {
        get
        {
            SqliteCommand command = new SqliteCommand("SELECT * FROM Processes", sqliteConnection);
            SqliteDataReader reader = command.ExecuteReader();
            field = new List<Process>();
            while (reader.Read())
            {
                Process process = new Process
                {
                    Id = reader.GetInt32(0),
                    ProcessName = reader.GetString(1),
                    WindowsName = reader.GetString(2),
                    ProcessStart = reader.GetDateTime(3)
                };
                field.Add(process);
            }
            return field;
        }
    }

    public Process? GetProcessById(in int id)
    {
        string statment = "SELECT * FROM Processes WHERE Id = @Id";
        SqliteCommand command = new SqliteCommand(statment, sqliteConnection);
        command.Parameters.AddWithValue("@Id", id);
        SqliteDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            Process process = new Process
            {
                Id = reader.GetInt32(0),
                ProcessName = reader.GetString(1),
                WindowsName = reader.GetString(2),
                ProcessStart = reader.GetDateTime(3)
            };
            return process;
        }
        return null;
    }

    public List<Process> GetProcessesByDate(DateOnly date)
    {
        string query = "SELECT * FROM Processes WHERE date(process_start) = @Date";
        SqliteCommand command = new SqliteCommand(query, sqliteConnection);
        command.Parameters.AddWithValue("@Date", date);
        SqliteDataReader reader = command.ExecuteReader();
        List<Process> processes = new List<Process>();
        while (reader.Read())
        {
            Process process = new Process
            {
                Id = reader.GetInt32(0),
                ProcessName = reader.GetString(1),
                WindowsName = reader.GetString(2),
                ProcessStart = reader.GetDateTime(3)
            };
            processes.Add(process);
        }
        return processes;
    }

    public int UpdateProcesses(in Process process)
    {
        string statment = "UPDATE Processes SET process_name = @ProcessName, windows_name = @WindowsName, process_start = @ProcessStart, WHERE Id = @Id";
        SqliteCommand command = new SqliteCommand(statment, sqliteConnection);
        command.Parameters.AddRange(
        [
            new SqliteParameter("@ProcessName", process.ProcessName),
            new SqliteParameter("@WindowsName", process.WindowsName),
            new SqliteParameter("@ProcessStart", process.ProcessStart),
            new SqliteParameter("@Id", process.Id)
        ]);
        return command.ExecuteNonQuery();
    }

    public int CreateProcesses(in Process process)
    {
        string statment = "INSERT INTO Processes (process_name, windows_name, process_start) VALUES (@ProcessName, @WindowsName, @ProcessStart)";
        SqliteCommand command = new SqliteCommand(statment, sqliteConnection);
        command.Parameters.AddRange(
        [
            new SqliteParameter("@ProcessName", process.ProcessName),
            new SqliteParameter("@WindowsName", process.WindowsName),
            new SqliteParameter("@ProcessStart", process.ProcessStart)
        ]);
        return command.ExecuteNonQuery();
    }

    public int DeleteProcesses(in int id)
    {
        string statment = "DELETE FROM Processes WHERE Id = @Id";
        SqliteCommand command = new SqliteCommand(statment, sqliteConnection);
        command.Parameters.AddWithValue("@Id", id);
        return command.ExecuteNonQuery();
    }
}
