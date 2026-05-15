# C# Best Practices

## Idiomatic C#
- **Naming:** PascalCase for classes and methods, camelCase for local variables and private fields (with `_` prefix).
- **LINQ:** Use LINQ for readability, but be mindful of performance in hot paths (avoid unnecessary allocations).
- **Records:** Use `record` for immutable data transfer objects (DTOs).
- **Pattern Matching:** Prefer switch expressions over nested if-else chains.

## Performance
- **Span<T> & Memory<T>:** Use for high-performance memory management without allocations.
- **ValueTask:** Use for async methods that often complete synchronously.
- **ArrayPool:** Use for frequently allocated large arrays.

## Error Handling
- **Exceptions:** Use exceptions only for truly exceptional circumstances.
- **Result Pattern:** Consider using a Result object for expected domain errors.
- **Global Exception Handling:** Implement middleware or filters for consistent API error responses.
