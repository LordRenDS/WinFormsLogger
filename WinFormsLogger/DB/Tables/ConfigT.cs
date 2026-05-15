using Microsoft.Data.Sqlite;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

internal class ConfigT : IConfigRepository
{
    private readonly DataBaseMSQ _dataBase;

    public ConfigT(DataBaseMSQ dataBase)
    {
        _dataBase = dataBase;
    }

    public Config? GetConfig()
    {
        using var command = new SqliteCommand("SELECT pc_id FROM Config LIMIT 1", _dataBase.SqConn);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Config
            {
                PcId = reader.IsDBNull(0) ? null : reader.GetString(0)
            };
        }
        return null;
    }

    public void SaveConfig(Config config)
    {
        // Use UPSERT pattern for SQLite (INSERT OR REPLACE)
        string statement = "INSERT OR REPLACE INTO Config (pc_id) VALUES (@PcId)";
        using var command = new SqliteCommand(statement, _dataBase.SqConn);
        command.Parameters.AddWithValue("@PcId", (object?)config.PcId ?? DBNull.Value);
        command.ExecuteNonQuery();
    }
}
