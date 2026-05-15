# WinForms Architecture Best Practices

## Separation of Concerns
Avoid putting business logic, data access, or complex calculations directly into the `Form` class.

## Model-View-Presenter (MVP)
The most common pattern for WinForms.
- **Model:** Data and business logic.
- **View:** The Form itself (implements an interface).
- **Presenter:** Coordinates between Model and View.

### Example View Interface
```csharp
public interface IProcessView
{
    void DisplayProcesses(List<Process> processes);
    string SearchFilter { get; }
    event EventHandler SearchClicked;
}
```

## Data Binding
Use `BindingSource` to synchronize data between objects and controls. It simplifies complex UI updates and provides support for currency management (current item selection).

## Error Handling
- Use `Application.ThreadException` to catch unhandled UI thread exceptions globally.
- Use `AppDomain.CurrentDomain.UnhandledException` for non-UI threads.
- Always provide user-friendly error messages via `MessageBox`.

## Resource Management
Always call `Dispose()` on controls, bitmaps, or graphics objects that are manually created and not added to a container's `Controls` collection.
- Use `using` statements for temporary resources.
- Override `Dispose(bool disposing)` if a custom control holds unmanaged resources.
