using System.IO;

namespace GithubKookBot.Models;

public static class FileLogger
{
    public static Action<string>? OnLogOutput;

    private static readonly string LogDir = Path.Combine(AppContext.BaseDirectory, "Logs");
    private static readonly string LogFile;
    private const string LoggerTag = "GithubKookBot";

    static FileLogger()
    {
        if (!Directory.Exists(LogDir))
            Directory.CreateDirectory(LogDir);
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        LogFile = Path.Combine(LogDir, $"{date}.txt");
    }

    public static void Info(string msg) => WriteLine("INFO", msg, null);

    public static void Warn(string msg) => WriteLine("WARN", msg, null);

    public static void Error(string msg, Exception? ex = null) => WriteLine("ERROR", msg, ex);

    private static void WriteLine(string level, string msg, Exception? ex)
    {
        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
        string logLine = $"{time}|{level}|{LoggerTag}|{msg}";

        OnLogOutput?.Invoke(logLine);

        if (ex == null)
        {
            File.AppendAllText(LogFile, logLine + Environment.NewLine);
            return;
        }
        string stackText = $"{logLine}{Environment.NewLine}" +
                           $"异常类型：{ex.GetType().FullName}{Environment.NewLine}" +
                           $"异常消息：{ex.Message}{Environment.NewLine}" +
                           $"堆栈信息：{ex.StackTrace}{Environment.NewLine}";
        OnLogOutput?.Invoke(stackText);
        File.AppendAllText(LogFile, stackText);
    }
}