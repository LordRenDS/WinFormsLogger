using Microsoft.Data.Sqlite;

namespace WinFormsLogger;

internal class DataBaseMSQ : IDisposable
{
    public SqliteConnection SqConn { get; private set; }
    public DataBaseMSQ()
    {
        CreateConnection();
        CreateTables();
    }

    public void Dispose() => SqConn.Dispose();

    private static string GetDbPath()
    {
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Logger");
        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);
        return Path.Combine(appDataPath, "logger.db");
    }

    private void CreateConnection()
    {
        SqliteConnectionStringBuilder csb = new SqliteConnectionStringBuilder();
        string dbPath = GetDbPath();
        csb.DataSource = dbPath;
        csb.ForeignKeys = true;
        csb.RecursiveTriggers = true;
        this.SqConn = new SqliteConnection(csb.ToString());
    }

    private void CreateTables()
    {
        ExtcuteQuery(TableStatment.PcStatusT);
        ExtcuteQuery(TableStatment.ProcessesT);
        ExtcuteQuery(TableStatment.SchedulesT);
    }

    private void ExtcuteQuery(in string statement)
    {
        this.SqConn.Open();
        SqliteCommand commannd = new SqliteCommand(statement, this.SqConn);
        commannd.ExecuteNonQuery();
        this.SqConn.Close();
    }
}

file static class TableStatment
{
    readonly public static string ProcessesT = """
        CREATE TABLE IF NOT EXISTS Processes (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            process_name TEXT NOT NULL,
            windows_name TEXT NOT NULL,
            process_start TIMESTAMP NOT NULL
        );
        """;
    readonly public static string SchedulesT = """
        CREATE TABLE IF NOT EXISTS Schedules (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            pc_status_id REFERENCES PcStatus(Id) ON DELETE CASCADE ON UPDATE CASCADE,
            action_time TIMESTAMP NOT NULL
        );
        """;
    readonly public static string PcStatusT = """
        CREATE TABLE IF NOT EXISTS PcStatus (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            status TEXT NOT NULL
        );
        """;
}
