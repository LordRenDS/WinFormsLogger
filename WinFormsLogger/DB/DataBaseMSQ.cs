using Microsoft.Data.Sqlite;

namespace WinFormsLogger;

public class DataBaseMSQ : IDisposable
{
    public SqliteConnection? SqConn { get; private set; }
    public object DbLock { get; } = new();

    public DataBaseMSQ()
    {
        CreateConnection();
        CreateTables();
        SeedData();
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
        ExecuteQuery(TableStatement.ConfigT);
        ExecuteQuery(TableStatement.PcStatusT);
        ExecuteQuery(TableStatement.ProcessesT);
        ExecuteQuery(TableStatement.SchedulesT);
    }

    private void SeedData()
    {
        lock (DbLock)
        {
            string[] statuses = { "PowerOn", "PowerOff", "Locked", "Unlocked" };
            foreach (var status in statuses)
            {
                using var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM PcStatus WHERE status = @status", this.SqConn);
                checkCommand.Parameters.AddWithValue("@status", status);
                var count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count == 0)
                {
                    using var insertCommand = new SqliteCommand("INSERT INTO PcStatus (status) VALUES (@status)", this.SqConn);
                    insertCommand.Parameters.AddWithValue("@status", status);
                    insertCommand.ExecuteNonQuery();
                }
            }
        }
    }

    private void ExecuteQuery(string statement)
    {
        lock (DbLock)
        {
            using SqliteCommand command = new SqliteCommand(statement, this.SqConn);
            command.ExecuteNonQuery();
        }
    }
}

file static class TableStatement
{
    public static readonly string ProcessesT = """
        CREATE TABLE IF NOT EXISTS Processes (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            process_name TEXT NOT NULL,
            windows_name TEXT NOT NULL,
            process_start TIMESTAMP NOT NULL,
            duration INTEGER NOT NULL DEFAULT 0,
            is_synced INTEGER NOT NULL DEFAULT 0
        );
        """;
    public static readonly string SchedulesT = """
        CREATE TABLE IF NOT EXISTS Schedules (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            pc_status_id INTEGER REFERENCES PcStatus(Id) ON DELETE CASCADE ON UPDATE CASCADE,
            timestamp TIMESTAMP NOT NULL,
            is_synced INTEGER NOT NULL DEFAULT 0
        );
        """;
    public static readonly string PcStatusT = """
        CREATE TABLE IF NOT EXISTS PcStatus (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            status TEXT NOT NULL
        );
        """;
    public static readonly string ConfigT = """
        CREATE TABLE IF NOT EXISTS Config (
            pc_id TEXT PRIMARY KEY
        );
        """;
}
