using Microsoft.Extensions.Logging;

namespace WinFormsLogger;

public class FileLoggerConfiguration
{
    public string FilePath { get; set; } = Path.Combine(LoggerUtils.GetAppPath(), "log.txt");
    public LogLevel MinLogLevel { get; set; } = LogLevel.Information;
}

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly FileLoggerConfiguration _config;
    private static readonly object _lock = new object();

    public FileLogger(string categoryName, FileLoggerConfiguration config)
    {
        _categoryName = categoryName;
        _config = config;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _config.MinLogLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string message = formatter(state, exception);
        var logRecord = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] [{_categoryName}] {message}";

        if (exception != null)
        {
            logRecord += Environment.NewLine + exception.ToString();
        }

        WriteToFile(logRecord);
    }

    private void WriteToFile(string message)
    {
        lock (_lock)
        {
            try
            {
                string? directory = Path.GetDirectoryName(_config.FilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.AppendAllText(_config.FilePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка запису в лог: {ex.Message}");
            }
        }
    }
}

public class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerConfiguration _config;

    public FileLoggerProvider(FileLoggerConfiguration config)
    {
        _config = config;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _config);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder,
        Action<FileLoggerConfiguration> configure)
    {
        FileLoggerConfiguration config = new FileLoggerConfiguration();
        configure(config);
        builder.AddProvider(new FileLoggerProvider(config));
        return builder;
    }
}