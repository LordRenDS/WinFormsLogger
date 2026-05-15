# C# Database Integration Reference

This reference outlines the patterns and best practices for integrating C# applications with relational databases using manual ADO.NET-style providers (like `Microsoft.Data.Sqlite`).

## Manual Repository Pattern

Separate database logic from business logic by using interfaces and repository classes.

### 1. Define the Interface
Define the contract for data operations in the `DB.Tables` namespace.

```csharp
public interface IProcessRepository
{
    IEnumerable<Process> GetAllProcesses();
    Process? GetProcessById(int id);
    int CreateProcess(Process process);
    // ... other CRUD operations
}
```

### 2. Implement the Repository
Implement the interface using a database connection provider.

```csharp
internal class ProcessesT : IProcessRepository
{
    private readonly DataBaseMSQ _dataBase;

    public ProcessesT(DataBaseMSQ dataBase)
    {
        _dataBase = dataBase;
    }

    public IEnumerable<Process> GetAllProcesses()
    {
        var processes = new List<Process>();
        using var command = new SqliteCommand("SELECT * FROM Processes", _dataBase.SqConn);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            processes.Add(MapReaderToProcess(reader));
        }
        return processes;
    }
}
```

## Resource Management (IDisposable)

Always use `using` statements (or `using var` in modern C#) for objects that implement `IDisposable` to ensure connections, commands, and readers are properly closed and disposed of, even if an exception occurs.

```csharp
// Prefer 'using var' for cleaner method scope management
using var command = new SqliteCommand(sql, connection);
using var reader = command.ExecuteReader();
```

## Data Mapping (Reader to Model)

Map database rows to C# objects manually. Handle potential `DBNull` values for optional columns.

```csharp
private Process MapReaderToProcess(SqliteDataReader reader)
{
    return new Process
    {
        Id = reader.GetInt32(0),
        ProcessName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
        WindowsName = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
        ProcessStart = reader.GetDateTime(3)
    };
}
```

### Best Practices:
- Use ordinal indexes (e.g., `reader.GetInt32(0)`) for performance if the query structure is fixed.
- Use named access (e.g., `reader.GetOrdinal("process_name")`) if the query structure might vary.

## Exception Handling

Database operations can fail due to connection issues, constraint violations, or syntax errors. Provide context when catching exceptions.

```csharp
public int CreateProcess(Process process)
{
    try
    {
        string statement = "INSERT INTO Processes (process_name) VALUES (@Name)";
        using var command = new SqliteCommand(statement, _dataBase.SqConn);
        command.Parameters.AddWithValue("@Name", process.ProcessName);
        return command.ExecuteNonQuery();
    }
    catch (SqliteException ex)
    {
        // Log the error or wrap it in a custom exception
        throw new DatabaseOperationException("Failed to create process record.", ex);
    }
}
```

## Parameterized Queries

**Never** use string concatenation to build SQL queries. Always use parameters to prevent SQL injection.

```csharp
// Correct way
command.Parameters.AddWithValue("@Id", id);

// Wrong way (Vulnerable)
// var command = new SqliteCommand("SELECT * FROM Processes WHERE Id = " + id, conn);
```
