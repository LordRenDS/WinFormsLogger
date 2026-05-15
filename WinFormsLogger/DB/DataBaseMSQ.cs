using Microsoft.Data.Sqlite;

namespace WinFormsLogger;

public class DataBaseMSQ : IDisposable
{
    public SqliteConnection? SqConn { get; private set; }

    public DataBaseMSQ()
    {
        CreateConnection();
        CreateTables();
    }

    public void Dispose()
    {
        if (SqConn != null)
        {
            if (SqConn.State == System.Data.ConnectionState.Open)
            {
                SqConn.Close();
            }
            SqConn.Dispose();
        }
    }

    private static string GetDbPath()
    {
        string appPath = LoggerUtils.GetAppPath();
        return Path.Combine(appPath, "logger.db");
    }

    private void CreateConnection()
    {
        SqliteConnectionStringBuilder csb = new SqliteConnectionStringBuilder();
        string dbPath = GetDbPath();
        csb.DataSource = dbPath;
        csb.ForeignKeys = true;
        csb.RecursiveTriggers = true;
        this.SqConn = new SqliteConnection(csb.ToString());
        this.SqConn.Open();
    }

    private void CreateTables()
    {
        ExecuteQuery(TableStatement.PcStatusT);
        ExecuteQuery(TableStatement.ProcessesT);
        ExecuteQuery(TableStatement.SchedulesT);
    }

    private void ExecuteQuery(string statement)
    {
        using SqliteCommand command = new SqliteCommand(statement, this.SqConn);
        command.ExecuteNonQuery();
    }
}

file static class TableStatement
{
    public static readonly string ProcessesT = """
        CREATE TABLE IF NOT EXISTS Processes (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            process_name TEXT NOT NULL,
            windows_name TEXT NOT NULL,
            process_start TIMESTAMP NOT NULL
        );
        """;
    public static readonly string SchedulesT = """
        CREATE TABLE IF NOT EXISTS Schedules (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            pc_status_id INTEGER REFERENCES PcStatus(Id) ON DELETE CASCADE ON UPDATE CASCADE,
            action_time TIMESTAMP NOT NULL
        );
        """;
    public static readonly string PcStatusT = """
        CREATE TABLE IF NOT EXISTS PcStatus (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            status TEXT NOT NULL
        );
        """;
}
