# Schema Design for SQLite & C#

This reference provides guidelines for designing efficient and robust relational database schemas for SQLite, specifically tailored for C# applications without an ORM.

## Normalization

Normalization reduces data redundancy and improves data integrity. Aim for **Third Normal Form (3NF)** for most application data.

| Form | Requirement | Goal |
| :--- | :--- | :--- |
| **1NF** | Atomic values, no repeating groups. | Eliminate duplicate columns and sets of data. |
| **2NF** | 1NF + non-key attributes depend on the *entire* primary key. | Ensure every column relates directly to the record's identity. |
| **3NF** | 2NF + no transitive dependencies. | Ensure columns depend *only* on the primary key, not other non-key columns. |

**When to use:**
- **Always start with 3NF:** It prevents update anomalies and ensures data consistency.
- **Denormalize sparingly:** Only for performance-critical read operations where joins become a significant bottleneck.

## Primary and Foreign Keys

### Primary Keys (PK)
- Use `INTEGER PRIMARY KEY` for auto-incrementing unique identifiers.
- Avoid natural keys (like SSN or Email) as primary keys; prefer surrogate keys.
- In SQLite, `INTEGER PRIMARY KEY` is an alias for the internal `rowid`.

### Foreign Keys (FK)
- Use FKs to enforce relationships between tables (1:1, 1:N).
- **CRITICAL:** SQLite requires enabling foreign key support at runtime for each connection:
  ```sql
  PRAGMA foreign_keys = ON;
  ```
- Use `ON DELETE CASCADE` or `ON DELETE SET NULL` to manage related data automatically.

## Data Types & C# Mapping

SQLite uses dynamic typing with storage classes. Use the following mapping for consistency in C#.

| SQLite Storage Class | Recommended C# Type | Use Case |
| :--- | :--- | :--- |
| **INTEGER** | `long`, `int`, `bool`, `Enum` | IDs, counts, flags (0/1), Unix timestamps. |
| **REAL** | `double`, `float`, `decimal` | Precision measurements, financial data (use `decimal` in C#). |
| **TEXT** | `string`, `Guid`, `DateTime` | Names, descriptions, UUIDs, ISO8601 dates. |
| **BLOB** | `byte[]` | Images, encrypted data, binary files. |
| **NULL** | `Nullable<T>` (e.g., `int?`) | Optional fields. |

**Note on Dates:** SQLite has no dedicated DATE type. Store dates as `TEXT` (ISO8601: `YYYY-MM-DD HH:MM:SS`) or `INTEGER` (Unix epoch) and handle conversion in C#.

## Integrity Constraints

Enforce business rules at the database level to ensure data quality regardless of application logic.

- **NOT NULL:** Prevents missing data in critical fields.
- **UNIQUE:** Ensures no duplicate values in columns like `Username` or `ExternalId`.
- **CHECK:** Validates data against a specific expression.
  - Example: `CHECK (Length > 0)` or `CHECK (Status IN ('Active', 'Inactive'))`.
- **DEFAULT:** Provides a value when none is specified.
  - Example: `CreatedAt TEXT DEFAULT (datetime('now'))`.
