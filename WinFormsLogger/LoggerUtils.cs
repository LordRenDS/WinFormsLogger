namespace WinFormsLogger;

internal class LoggerUtils
{
    public static string GetAppPath()
    {
        string appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Logger");
        if (!Directory.Exists(appPath))
            Directory.CreateDirectory(appPath);
        return appPath;
    }
}
