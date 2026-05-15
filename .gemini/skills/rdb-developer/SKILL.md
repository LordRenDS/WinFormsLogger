---
name: rdb-developer
description: Guidelines for relational database design, SQL optimization, and C# integration. Use this skill when designing schemas, writing SQL queries, or implementing data access layers in C#.
---

# Rdb Developer

## Overview

The Rdb Developer skill provides comprehensive guidance for building robust database-driven applications. It covers schema design principles, SQL best practices, and idiomatic C# integration patterns using the Repository pattern and ADO.NET providers.

## Guidelines

- **Schema Design:** Prioritize normalization, clear naming conventions, and appropriate indexing.
- **SQL Best Practices:** Use parameterized queries, optimize joins, and avoid `SELECT *` in production code.
- **C# Integration:** Implement the Repository pattern to decouple database logic from the application core. Always use `using` statements for resource management.

## Resources

### references/

- [Schema Design](references/schema-design.md): Principles of relational modeling and normalization.
- [SQL Best Practices](references/sql-best-practices.md): Writing efficient and secure SQL queries.
- [C# Integration](references/csharp-integration.md): Implementing repositories, data mapping, and resource management in C#.
