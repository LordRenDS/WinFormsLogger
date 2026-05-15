# .NET Project Structures

## Clean Architecture (Standard)
```text
SolutionName/
├── src/
│   ├── Domain/              # Core entities, value objects, exceptions
│   ├── Application/         # Use cases, interfaces, DTOs, CQRS commands/queries
│   ├── Infrastructure/      # Data access (EF Core), external API clients, file system
│   └── WebUI/               # ASP.NET Core API, Blazor, or MVC
├── tests/
│   ├── Domain.UnitTests/
│   ├── Application.UnitTests/
│   └── WebUI.IntegrationTests/
└── SolutionName.sln
```

## Vertical Slice Architecture
- Focuses on "features" rather than "layers".
- Each slice contains everything needed for a specific feature (API endpoint, logic, data access).
- Highly recommended for microservices or modular monoliths.
