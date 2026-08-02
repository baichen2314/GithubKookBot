using GithubKookBot.Models;
using Microsoft.Extensions.Logging;

namespace GithubKookBot.Services;

public class UpdateChecker
{
    private readonly GithubService _githubService;
    private readonly KookService _kookService;
    private readonly StateStore _stateStore;
    private readonly ILogger<UpdateChecker> _logger;
    private readonly ConfigSettings _settings;

    public UpdateChecker(
        GithubService githubService,
        KookService kookService,
        StateStore stateStore,
        ILogger<UpdateChecker> logger,
        ConfigSettings settings)
    {
        _githubService = githubService;
        _kookService = kookService;
        _stateStore = stateStore;
        _logger = logger;
        _settings = settings;
    }

    private async Task SendHistoryAsync(SubscriptionConfig sub, int fetchLimit, bool forceReload = false, GithubRepoInfo? repoInfo = null)
    {
        var mode = _settings.UpdateMode.ToLower();
        string logLine = $"[历史推送] Repo={sub.FullName} Mode={mode} Limit={fetchLimit} Force={forceReload}";
        _logger.LogInformation("[历史推送] Repo={Repo} Mode={Mode} Limit={Limit} Force={Force}", sub.FullName, mode, fetchLimit, forceReload);
        FileLogger.Info(logLine);
        if (mode == "release" || mode == "both")
        {
            var release = await _githubService.GetLatestReleaseAsync(sub.Owner, sub.Repo);
            if (release != null)
            {
                object releaseCard = await _kookService.BuildReleaseCardObj(sub, release);
                await _kookService.SendCardMessageAsync(releaseCard);
                await Task.Delay(200);
            }
        }
        if (mode == "commit" || mode == "both")
        {
            var commits = await _githubService.GetRecentCommitsAsync(sub.Owner, sub.Repo, fetchLimit);
            string logCount = $"[历史推送] Repo={sub.FullName} 获取提交总数={commits.Count}";
            _logger.LogInformation("[历史推送] Repo={Repo} Count={Count}", sub.FullName, commits.Count);
            FileLogger.Info(logCount);
            if (commits.Count == 0)
            {
                string warn = $"[历史推送] Repo={sub.FullName} 无任何提交";
                _logger.LogWarning("[历史推送] Repo={Repo} 无任何提交", sub.FullName);
                FileLogger.Warn(warn);
                return;
            }
            var orderedCommits = new List<GithubCommitItem>(commits);
            orderedCommits.Reverse();
            foreach (var item in orderedCommits)
            {
                try
                {
                    object cardObj = await _kookService.BuildCommitCardObj(sub, item, repoInfo);
                    bool sendRes = await _kookService.SendCardMessageAsync(cardObj);
                    string shortSha = item.Sha.Length >= 7 ? item.Sha[..7] : item.Sha;
                    string cardLog = $"[历史卡片] Repo={sub.FullName} Sha={shortSha} 发送={sendRes}";
                    _logger.LogInformation("[历史卡片] Repo={Repo} Sha={Sha} 发送={Res}", sub.FullName, shortSha, sendRes);
                    FileLogger.Info(cardLog);
                }
                catch (Exception ex)
                {
                    string err = $"历史卡片异常 Sha={item.Sha}";
                    _logger.LogError(ex, err);
                    FileLogger.Error(err, ex);
                }
                await Task.Delay(300);
            }
        }
    }

    public async Task CheckAllAsync(bool isFirstLaunch = false, bool forceHistoryReload = false, int fetchLimit = 10)
    {
        string startLog = $"==== 批量检测开始 First={isFirstLaunch} ForceHistory={forceHistoryReload} FetchLimit={fetchLimit} ====";
        _logger.LogInformation("==== 批量检测开始 First={First} Force={Force} FetchLimit={Limit} ====", isFirstLaunch, forceHistoryReload, fetchLimit);
        FileLogger.Info(startLog);
        foreach (var sub in _settings.Subscriptions)
        {
            try
            {
                await CheckSubscriptionAsync(sub, isFirstLaunch, forceHistoryReload, fetchLimit);
            }
            catch (Exception ex)
            {
                string err = $"仓库 {sub.FullName} 整体检测异常";
                _logger.LogError(ex, "仓库整体异常 Repo={Repo}", sub.FullName);
                FileLogger.Error(err, ex);
            }
            await Task.Delay(500);
        }
        string endLog = "==== 批量检测完成 ====";
        _logger.LogInformation(endLog);
        FileLogger.Info(endLog);
    }

    private async Task CheckSubscriptionAsync(SubscriptionConfig sub, bool isFirstLaunch, bool forceHistoryReload, int fetchLimit)
    {
        var state = _stateStore.GetState(sub.FullName);
        bool isNewState = state.LastCheckedAt == DateTime.MinValue;
        var mode = _settings.UpdateMode.ToLower();
        var repoInfo = await _githubService.GetRepoInfoAsync(sub.Owner, sub.Repo);

        if (forceHistoryReload)
        {
            await SendHistoryAsync(sub, fetchLimit, forceHistoryReload, repoInfo);
            if (mode == "commit" || mode == "both")
            {
                var latestCommit = await _githubService.GetLatestCommitAsync(sub.Owner, sub.Repo);
                if (latestCommit != null)
                    state.LastCommitSha = latestCommit.Sha;
            }
            if (mode == "release" || mode == "both")
            {
                var latestRelease = await _githubService.GetLatestReleaseAsync(sub.Owner, sub.Repo);
                if (latestRelease != null)
                    state.LastReleaseTag = latestRelease.TagName;
            }
        }

        if (isFirstLaunch && isNewState)
        {
            var latestCommit = await _githubService.GetLatestCommitAsync(sub.Owner, sub.Repo);
            if (latestCommit != null)
            {
                string shortSha = latestCommit.Sha.Length >= 7 ? latestCommit.Sha[..7] : latestCommit.Sha;
                state.LastCommitSha = latestCommit.Sha;
                string initLog = $"[启动初始化缓存] Repo={sub.FullName} 记录初始Sha={shortSha}";
                _logger.LogInformation("[启动初始化缓存] Repo={Repo} Sha={Sha}", sub.FullName, shortSha);
                FileLogger.Info(initLog);
            }
        }

        if (mode == "release" || mode == "both")
            await CheckReleaseUpdateAsync(sub, state);
        if (mode == "commit" || mode == "both")
            await CheckCommitUpdateAsync(sub, isNewState, state, repoInfo, fetchLimit);

        _stateStore.UpdateState(state);
    }

    private async Task CheckReleaseUpdateAsync(SubscriptionConfig sub, SubscriptionState state)
    {
        var latest = await _githubService.GetLatestReleaseAsync(sub.Owner, sub.Repo);
        if (latest == null) return;
        if (latest.TagName != state.LastReleaseTag)
        {
            object releaseCard = await _kookService.BuildReleaseCardObj(sub, latest);
            bool sendOk = await _kookService.SendCardMessageAsync(releaseCard);
            await Task.Delay(200);
            state.LastReleaseTag = latest.TagName;
            string log = $"[Release更新] Repo={sub.FullName} 新版本={latest.TagName} 卡片发送成功={sendOk}";
            _logger.LogInformation("[Release更新] Repo={Repo} Tag={Tag} SendCard={Send}", sub.FullName, latest.TagName, sendOk);
            FileLogger.Info(log);
        }
    }

    private async Task CheckCommitUpdateAsync(SubscriptionConfig sub, bool isNewState, SubscriptionState state, GithubRepoInfo? repoInfo = null, int fetchLimit = 10)
    {
        var allCommits = await _githubService.GetRecentCommitsAsync(sub.Owner, sub.Repo, fetchLimit);
        string pullLog = $"[增量检测] Repo={sub.FullName} 拉取提交数={allCommits.Count}";
        _logger.LogInformation("[增量检测] Repo={Repo} 拉取提交数={Count}", sub.FullName, allCommits.Count);
        FileLogger.Info(pullLog);
        if (allCommits.Count == 0) return;
        var latestItem = allCommits[0];
        if (isNewState)
        {
            string shortSha = latestItem.Sha.Length >= 7 ? latestItem.Sha[..7] : latestItem.Sha;
            state.LastCommitSha = latestItem.Sha;
            string initLog = $"[全新缓存] Repo={sub.FullName} 记录初始Sha={shortSha}";
            _logger.LogInformation("[全新缓存] Repo={Repo} 记录初始Sha={Sha}", sub.FullName, shortSha);
            FileLogger.Info(initLog);
            return;
        }
        List<GithubCommitItem> newCommits = new();
        foreach (var c in allCommits)
        {
            if (c.Sha == state.LastCommitSha) break;
            newCommits.Add(c);
        }
        if (newCommits.Count == 0)
        {
            string emptyLog = $"[增量检测] Repo={sub.FullName} 无新提交";
            _logger.LogInformation("[增量检测] Repo={Repo} 无新提交", sub.FullName);
            FileLogger.Info(emptyLog);
            return;
        }
        string newCountLog = $"[增量检测] Repo={sub.FullName} 新增{newCommits.Count}条提交";
        _logger.LogInformation("[增量检测] Repo={Repo} 新增{Count}条提交", sub.FullName, newCommits.Count);
        FileLogger.Info(newCountLog);
        newCommits.Reverse();
        foreach (var c in newCommits)
        {
            try
            {
                object card = await _kookService.BuildCommitCardObj(sub, c, repoInfo);
                bool ok = await _kookService.SendCardMessageAsync(card);
                string shortSha = c.Sha.Length >= 7 ? c.Sha[..7] : c.Sha;
                string sendLog = $"[增量卡片] Repo={sub.FullName} Sha={shortSha} 发送={ok}";
                _logger.LogInformation("[增量卡片] Repo={Repo} Sha={Sha} 发送={Res}", sub.FullName, shortSha, ok);
                FileLogger.Info(sendLog);
            }
            catch (Exception ex)
            {
                string err = $"增量卡片异常 Sha={c.Sha}";
                _logger.LogError(ex, err);
                FileLogger.Error(err, ex);
            }
            await Task.Delay(300);
        }
        state.LastCommitSha = latestItem.Sha;
    }
}