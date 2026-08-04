using GithubKookBot.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GithubKookBot.Services;

public class StateStore
{
    private readonly ReaderWriterLockSlim _rwLock = new();
    private readonly ILogger<StateStore> _logger;
    private readonly string _stateFilePath;
    private Dictionary<string, SubscriptionState> _states = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public StateStore(ILogger<StateStore> logger)
    {
        _logger = logger;
        _stateFilePath = Path.Combine(AppContext.BaseDirectory, "subscription_state.json");
        LoadState();
    }

    private void LoadState()
    {
        _rwLock.EnterWriteLock();
        try
        {
            if (!File.Exists(_stateFilePath))
            {
                string logs = "状态文件不存在，将创建新的状态文件";
                _logger.LogInformation(logs);
                FileLogger.Info(logs);
                return;
            }

            string json = File.ReadAllText(_stateFilePath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, SubscriptionState>>(json, _jsonOptions);
            _states = loaded ?? new Dictionary<string, SubscriptionState>();

            string logMsg = $"已加载 {_states.Count} 个订阅状态";
            _logger.LogInformation(logMsg);
            FileLogger.Info(logMsg);
        }
        catch (Exception ex)
        {
            string logMsg = "加载状态文件失败，将使用空状态";
            _logger.LogError(ex, logMsg);
            FileLogger.Error(logMsg, ex);
            _states = new Dictionary<string, SubscriptionState>();
        }
        finally
        {
            if (_rwLock.IsWriteLockHeld)
                _rwLock.ExitWriteLock();
        }
    }

    public async Task LoadStateAsync()
    {
        await Task.Run(() =>
        {
            _rwLock.EnterWriteLock();
            try
            {
                if (!File.Exists(_stateFilePath))
                {
                    string logs = "状态文件不存在，将创建新的状态文件";
                    _logger.LogInformation(logs);
                    FileLogger.Info(logs);
                    return;
                }

                string json = File.ReadAllText(_stateFilePath);
                var loaded = JsonSerializer.Deserialize<Dictionary<string, SubscriptionState>>(json, _jsonOptions);
                _states = loaded ?? new Dictionary<string, SubscriptionState>();

                string logMsg = $"已加载 {_states.Count} 个订阅状态";
                _logger.LogInformation(logMsg);
                FileLogger.Info(logMsg);
            }
            catch (Exception ex)
            {
                string logMsg = "异步加载状态文件失败，将使用空状态";
                _logger.LogError(ex, logMsg);
                FileLogger.Error(logMsg, ex);
                _states = new Dictionary<string, SubscriptionState>();
            }
            finally
            {
                if (_rwLock.IsWriteLockHeld)
                    _rwLock.ExitWriteLock();
            }
        });
    }

    public async Task SaveStateAsync()
    {
        _rwLock.EnterReadLock();
        Dictionary<string, SubscriptionState> snapshot;
        try
        {
            snapshot = new Dictionary<string, SubscriptionState>(_states);
        }
        finally
        {
            if (_rwLock.IsReadLockHeld)
                _rwLock.ExitReadLock();
        }

        try
        {
            string json = JsonSerializer.Serialize(snapshot, _jsonOptions);
            string tempPath = _stateFilePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json);
            if (File.Exists(_stateFilePath))
                File.Replace(tempPath, _stateFilePath, null);
            else
                File.Move(tempPath, _stateFilePath);
        }
        catch (Exception ex)
        {
            string logMsg = "异步保存状态文件失败";
            _logger.LogError(ex, logMsg);
            FileLogger.Error(logMsg, ex);
        }
    }

    public SubscriptionState GetState(string repoFullName)
    {
        _rwLock.EnterReadLock();
        try
        {
            if (_states.TryGetValue(repoFullName, out var state))
            {
                return new SubscriptionState
                {
                    RepoFullName = state.RepoFullName,
                    LastReleaseTag = state.LastReleaseTag,
                    LastCheckedAt = state.LastCheckedAt,
                    BranchLastCommitSha = new Dictionary<string, string>(state.BranchLastCommitSha)
                };
            }
            return new SubscriptionState { RepoFullName = repoFullName };
        }
        finally
        {
            if (_rwLock.IsReadLockHeld)
                _rwLock.ExitReadLock();
        }
    }

    public void UpdateStateMemoryOnly(SubscriptionState state)
    {
        _rwLock.EnterWriteLock();
        try
        {
            state.LastCheckedAt = DateTime.UtcNow;
            _states[state.RepoFullName] = state;
        }
        finally
        {
            if (_rwLock.IsWriteLockHeld)
                _rwLock.ExitWriteLock();
        }
    }

    public void UpdateState(SubscriptionState state)
    {
        UpdateStateMemoryOnly(state);
        _ = SaveStateAsync();
    }

    public void SaveState()
    {
        _rwLock.EnterReadLock();
        Dictionary<string, SubscriptionState> snapshot;
        try
        {
            snapshot = new Dictionary<string, SubscriptionState>(_states);
        }
        finally
        {
            if (_rwLock.IsReadLockHeld)
                _rwLock.ExitReadLock();
        }

        try
        {
            string json = JsonSerializer.Serialize(snapshot, _jsonOptions);
            string tempPath = _stateFilePath + ".tmp";
            File.WriteAllText(tempPath, json);
            if (File.Exists(_stateFilePath))
                File.Replace(tempPath, _stateFilePath, null);
            else
                File.Move(tempPath, _stateFilePath);
        }
        catch (Exception ex)
        {
            string logMsg = "同步保存状态文件失败";
            _logger.LogError(ex, logMsg);
            FileLogger.Error(logMsg, ex);
        }
    }

    public void Dispose()
    {
        _rwLock.Dispose();
    }
}