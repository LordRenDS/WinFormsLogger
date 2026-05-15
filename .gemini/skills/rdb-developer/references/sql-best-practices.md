# SQL Best Practices for SQLite & C#

This reference provides best practices for writing efficient, secure, and maintainable SQL within C# applications using `Microsoft.Data.Sqlite`.

## Parameterization (SQL Injection Prevention)

**Never** use string interpolation or concatenation to build SQL queries with user-provided data. This exposes the application to SQL Injection attacks.

### The Right Way (Parameters)
Always use `SqliteParameter` to safely pass values to your queries.

```csharp
public void UpdateProcessTitle(long id, string newTitle)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var sql = "UPDATE Processes SET WindowTitle = @title WHERE Id = @id";
    using var command = new SqliteCommand(sql, connection);
    
    // Explicitly add parameters
    command.Parameters.AddWithValue("@title", newTitle);
    command.Parameters.AddWithValue("@id", id);

    command.ExecuteNonQuery();
}
```

**Why use parameters?**
- **Security:** Prevents SQL injection by treating input as data, not executable code.
- **Performance:** Allows the database to cache the query execution plan.
- **Type Safety:** Handles data type conversions (e.g., escaping single quotes in strings) automatically.

## Basic CRUD Optimization

### Efficient SELECT Patterns
- **Avoid `SELECT *`:** Only request the columns you actually need. This reduces memory usage and network/IO overhead.
- **Use LIMIT:** When fetching large datasets for UI, use `LIMIT` and `OFFSET` for pagination.

```sql
-- Good: Explicit columns
SELECT Id, ProcessName, WindowTitle FROM Processes WHERE IsActive = 1 LIMIT 50;

-- Bad: Over-fetching
SELECT * FROM Processes;
```

### Batch INSERTs with Transactions
SQLite is very fast at reading but can be slow at individual writes because every `INSERT` is its own transaction by default. Group multiple inserts into a single transaction.

```csharp
public void BulkInsert(IEnumerable<ProcessEntry> entries)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();
    using var transaction = connection.BeginTransaction();

    var sql = "INSERT INTO Processes (ProcessName, WindowTitle) VALUES (@name, @title)";
    using var command = new SqliteCommand(sql, connection, transaction);
    
    var nameParam = command.Parameters.Add("@name", SqliteType.Text);
    var titleParam = command.Parameters.Add("@title", SqliteType.Text);

    foreach (var entry in entries)
    {
        nameParam.Value = entry.Name;
        titleParam.Value = entry.Title;
        command.ExecuteNonQuery();
    }

    transaction.Commit();
}
```

## Indexing Strategies

Indexes significantly speed up `SELECT` and `WHERE` operations but slow down `INSERT`, `UPDATE`, and `DELETE` operations.

### When to Add an Index
- Columns used frequently in `WHERE` clauses.
- Columns used in `JOIN` conditions.
- Columns used in `ORDER BY` or `GROUP BY` clauses.

### Covering Indexes
A covering index includes all columns requested by a query, allowing SQLite to satisfy the query entirely from the index without looking up the actual table rows.

```sql
-- Speeds up: SELECT WindowTitle FROM Processes WHERE ProcessName = 'chrome';
CREATE INDEX idx_process_lookup ON Processes (ProcessName, WindowTitle);
```

### Costs and Trade-offs
- **Storage:** Every index takes up additional disk space.
- **Write Performance:** Every `INSERT`/`UPDATE`/`DELETE` must also update all relevant indexes.
- **Maintenance:** Avoid "over-indexing." Don't index columns with very low cardinality (e.g., a `IsDeleted` boolean column).

## Manual Transaction Management

Use transactions to ensure **Atomicity** (all operations succeed or none do) and to improve performance for write operations.

### Standard Pattern
Always use a `try-catch` block or the `using` pattern with transactions.

```csharp
public void ProcessTransfer(long sourceId, long targetId, decimal amount)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();
    using var transaction = connection.BeginTransaction();

    try
    {
        // 1. Deduct from source
        var decrCmd = new SqliteCommand("UPDATE Accounts SET Balance = Balance - @amt WHERE Id = @id", connection, transaction);
        decrCmd.Parameters.AddWithValue("@amt", amount);
        decrCmd.Parameters.AddWithValue("@id", sourceId);
        decrCmd.ExecuteNonQuery();

        // 2. Add to target
        var incrCmd = new SqliteCommand("UPDATE Accounts SET Balance = Balance + @amt WHERE Id = @id", connection, transaction);
        incrCmd.Parameters.AddWithValue("@amt", amount);
        incrCmd.Parameters.AddWithValue("@id", targetId);
        incrCmd.ExecuteNonQuery();

        // All good, commit changes
        transaction.Commit();
    }
    catch (Exception)
    {
        // Something failed, roll back everything
        transaction.Rollback();
        throw;
    }
}
```

**Key Rules:**
- **Keep transactions short:** Transactions lock the database (or parts of it), which can block other operations.
- **Dispose everything:** Ensure `SqliteConnection`, `SqliteCommand`, and `SqliteTransaction` are disposed (use `using` blocks).
