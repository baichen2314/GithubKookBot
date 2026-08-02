using GithubKookBot.Models;
using GithubKookBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GithubKookBot;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        FileLogger.Info("GithubKookBot 程序开始启动");
        string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        if (!File.Exists(configPath))
        {
            FileLogger.Info("未检测到 config.json，自动生成默认配置文件");
            CreateDefaultAppSettings(configPath);
        }
        ConfigSettings globalAppSettings;
        string jsonText = File.ReadAllText(configPath);
        var jsonOpt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var doc = JsonDocument.Parse(jsonText);
        string appSettingJson = doc.RootElement.GetProperty("Config").GetRawText();
        globalAppSettings = JsonSerializer.Deserialize<ConfigSettings>(appSettingJson) ?? new ConfigSettings();

        var services = new ServiceCollection();
        services.AddSingleton(globalAppSettings);
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        services.AddHttpClient<GithubService>();
        services.AddHttpClient<KookService>();
        services.AddSingleton<StateStore>();
        services.AddSingleton<UpdateChecker>();
        services.AddTransient<Main>();
        var sp = services.BuildServiceProvider();
        try
        {
            var form = sp.GetRequiredService<Main>();
            Application.Run(form);
            FileLogger.Info("程序正常退出");
        }
        catch (Exception ex)
        {
            FileLogger.Error("程序启动发生致命异常", ex);
            MessageBox.Show($"程序启动失败：{ex.Message}", "致命错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void CreateDefaultAppSettings(string filePath)
    {
        var defaultSetting = new ConfigSettings
        {
            GithubToken = "",
            KookBotToken = "",
            KookChannelId = "",
            CheckIntervalMinutes = 30,
            UpdateMode = "Release",
            Subscriptions = new List<SubscriptionConfig>(),
            BaiduApiKey = "",
            BaiduSecretKey = "",
            FetchCommitCount = 10
        };
        var root = new
        {
            Logging = new { LogLevel = new { Default = "Information", Microsoft = "Warning" } },
            Config = defaultSetting
        };
        string json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
        FileLogger.Info($"默认配置文件已生成：{filePath}");
    }
}