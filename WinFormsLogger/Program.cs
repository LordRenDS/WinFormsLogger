using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WinFormsLogger.DB.Tables;
using WinFormsLogger.Services;

namespace WinFormsLogger;

internal static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        var form1 = ServiceProvider.GetRequiredService<Form1>();
        Application.Run(form1);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddFileLogger(configure => { });
            builder.AddConsole();
        });

        services.AddSingleton<DataBaseMSQ>();
        services.AddTransient<IProcessRepository, ProcessesT>();
        services.AddTransient<IConfigRepository, ConfigT>();
        services.AddTransient<IPcStatusRepository, PcStatusT>();
        services.AddTransient<IScheduleRepository, SchedulesT>();
        services.AddTransient<IProcessTracer, ProcessTracer>();
        services.AddSingleton<IDeviceIdentityService, DeviceIdentityService>();
        services.AddTransient<Form1>();
    }
}
