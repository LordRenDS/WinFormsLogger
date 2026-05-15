---
name: csharp-developer
description: Expert guidance on C# development, including architectural patterns, coding standards, and best practices. Use when designing new C# applications, refactoring existing code, or seeking advice on idiomatic C# and .NET development.
---

# C# Developer Skill

## Overview
This skill transforms Gemini CLI into a senior C#/.NET architect and developer. It provides structured guidance on modern C# development (C# 12+ and .NET 8+), focusing on performance, maintainability, and industry standards.

## Core Guidance

### 1. Modern C# Standards
- **Coding Style:** Follow [Microsoft's C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- **Latest Features:** Leverage Primary Constructors, Collection Expressions, and Raw String Literals where appropriate.
- **Async/Await:** Always use `Task.Run` sparingly; prefer asynchronous I/O and avoid `async void`.

### 2. Architecture & Design
- **Clean Architecture:** Separate concerns into Domain, Application, Infrastructure, and Presentation layers. See [references/project-structure.md](references/project-structure.md).
- **SOLID Principles:** Rigorously apply SOLID to ensure testability and flexibility. See [references/best-practices.md](references/best-practices.md).
- **Dependency Injection:** Use the built-in .NET DI container for managing service lifetimes (Singleton, Scoped, Transient).

### 3. Testing & Quality
- **Unit Testing:** Prefer xUnit with FluentAssertions and Moq/NSubstitute.
- **Integration Testing:** Use `WebApplicationFactory` for end-to-end API testing.

## Reference Material

- **Best Practices:** Detailed guide on idiomatic C#, design patterns, and performance optimizations. [references/best-practices.md](references/best-practices.md)
- **Project Structure:** Common architectural patterns and folder structures for .NET solutions. [references/project-structure.md](references/project-structure.md)
- **Skills Matrix:** Necessary technical skills and knowledge areas for C# developers at various levels. [references/skills-matrix.md](references/skills-matrix.md)

## Common Tasks
- **Code Review:** Provide the code snippet and ask for an idiomatic C# review.
- **Project Scaffolding:** Ask for a project structure recommendation based on your requirements.
- **Design Patterns:** Ask how to implement a specific pattern (e.g., CQRS, Repository, Factory) in C#.
