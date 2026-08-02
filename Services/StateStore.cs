using System.Text.Json;
using GithubKookBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GithubKookBot.Services;

public class StateStore
{
    private readonly ILogger<StateStore> _logger;
    private readonly string _stateFilePath;
    private Dictionary<string, SubscriptionState> _states = new();

    public StateStore(ILogger<StateStore> logger)
    {
        _logger = logger;
        _stateFilePath = Path.Combine(AppContext.BaseDirectory, "subscription_state.json");
        LoadState();
    }

    private void LoadState()
    {
        try
        {
            if (File.Exists(_stateFilePath))
            {
                var json = File.ReadAllText(_stateFilePath);
                _states = JsonSerializer.Deserialize<Dictionary<string, SubscriptionState>>(json)
                          ?? new Dictionary<string, SubscriptionState>();
                string logMsg = $"已加载 {_states.Count} 个订阅状态";
                _logger.LogInformation(logMsg);
                FileLogger.Info(logMsg);
            }
            else
            {
                string logMsg = "状态文件不存在，将创建新的状态文件";
                _logger.LogInformation(logMsg);
                FileLogger.Info(logMsg);
            }
        }
        catch (Exception ex)
        {
            string logMsg = "加载状态文件失败，将使用空状态";
            _logger.LogError(ex, logMsg);
            FileLogger.Error(logMsg, ex);
            _states = new Dictionary<string, SubscriptionState>();
        }
    }

    private void SaveState()
    {
        try
        {
            var json = JsonSerializer.Serialize(_states, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_stateFilePath, json);
        }
        catch (Exception ex)
        {
            string logMsg = "保存状态文件失败";
            _logger.LogError(ex, logMsg);
            FileLogger.Error(logMsg, ex);
        }
    }

    public SubscriptionState GetState(string repoFullName)
    {
        if (_states.TryGetValue(repoFullName, out var state))
        {
            return state;
        }
        return new SubscriptionState { RepoFullName = repoFullName };
    }

    public void UpdateState(SubscriptionState state)
    {
        _states[state.RepoFullName] = state;
        state.LastCheckedAt = DateTime.UtcNow;
        SaveState();
    }
}