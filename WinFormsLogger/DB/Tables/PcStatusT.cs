using Microsoft.Data.Sqlite;
using WinFormsLogger.DB.Models;

namespace WinFormsLogger.DB.Tables;

internal class PcStatusT : IPcStatusRepository
{
    private readonly DataBaseMSQ _dataBase;

    public PcStatusT(DataBaseMSQ dataBase)
    {
        _dataBase = dataBase;
    }

    public IEnumerable<PcStatus> GetAll()
    {
        lock (_dataBase.DbLock)
        {
            var statuses = new List<PcStatus>();
            using var command = new SqliteCommand("SELECT id, status FROM PcStatus", _dataBase.SqConn);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                statuses.Add(new PcStatus
                {
                    Id = reader.GetInt32(0),
                    Status = reader.IsDBNull(1) ? null : reader.GetString(1)
                });
            }
            return statuses;
        }
    }

    public PcStatus? GetById(int id)
    {
        lock (_dataBase.DbLock)
        {
            using var command = new SqliteCommand("SELECT id, status FROM PcStatus WHERE id = @Id", _dataBase.SqConn);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new PcStatus
                {
                    Id = reader.GetInt32(0),
                    Status = reader.IsDBNull(1) ? null : reader.GetString(1)
                };
            }
            return null;
        }
    }

    public int Create(PcStatus status)
    {
        lock (_dataBase.DbLock)
        {
            string statement = "INSERT INTO PcStatus (status) VALUES (@Status)";
            using var command = new SqliteCommand(statement, _dataBase.SqConn);
            command.Parameters.AddWithValue("@Status", (object?)status.Status ?? DBNull.Value);
            return command.ExecuteNonQuery();
        }
    }

    public int Update(PcStatus status)
    {
        lock (_dataBase.DbLock)
        {
            string statement = "UPDATE PcStatus SET status = @Status WHERE id = @Id";
            using var command = new SqliteCommand(statement, _dataBase.SqConn);
            command.Parameters.AddWithValue("@Status", (object?)status.Status ?? DBNull.Value);
            command.Parameters.AddWithValue("@Id", status.Id);
            return command.ExecuteNonQuery();
        }
    }

    public int Delete(int id)
    {
        lock (_dataBase.DbLock)
        {
            using var command = new SqliteCommand("DELETE FROM PcStatus WHERE id = @Id", _dataBase.SqConn);
            command.Parameters.AddWithValue("@Id", id);
            return command.ExecuteNonQuery();
        }
    }
}
