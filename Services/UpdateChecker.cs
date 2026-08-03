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
            var allBranches = await _githubService.GetAllBranchesAsync(sub.Owner, sub.Repo);
            foreach (var branch in allBranches)
            {
                var commits = await _githubService.GetRecentCommitsAsync(sub.Owner, sub.Repo, fetchLimit, branch.Name);
                string logCount = $"[历史推送] Repo={sub.FullName} 分支={branch.Name} 获取提交总数={commits.Count}";
                _logger.LogInformation("[历史推送] Repo={Repo} Branch={Branch} Count={Count}", sub.FullName, branch.Name, commits.Count);
                FileLogger.Info(logCount);

                if (commits.Count == 0)
                {
                    string warn = $"[历史推送] Repo={sub.FullName} 分支{branch.Name} 无任何提交";
                    _logger.LogWarning("[历史推送] Repo={Repo} Branch={Branch} 无任何提交", sub.FullName, branch.Name);
                    FileLogger.Warn(warn);
                    continue;
                }

                var orderedCommits = new List<GithubCommitItem>(commits);
                orderedCommits.Reverse();
                foreach (var item in orderedCommits)
                {
                    try
                    {
                        object cardObj = await _kookService.BuildCommitCardObj(sub, item, repoInfo, branch.Name);
                        bool sendRes = await _kookService.SendCardMessageAsync(cardObj);
                        string shortSha = item.Sha.Length >= 7 ? item.Sha[..7] : item.Sha;
                        string cardLog = $"[历史卡片] Repo={sub.FullName} Branch={branch.Name} Sha={shortSha} 发送={sendRes}";
                        _logger.LogInformation("[历史卡片] Repo={Repo} Branch={Branch} Sha={Sha} 发送={Res}", sub.FullName, branch.Name, shortSha, sendRes);
                        FileLogger.Info(cardLog);
                    }
                    catch (Exception ex)
                    {
                        string err = $"历史卡片异常 分支={branch.Name} Sha={item.Sha}";
                        _logger.LogError(ex, err);
                        FileLogger.Error(err, ex);
                    }
                    await Task.Delay(300);
                }
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
        var allBranches = await _githubService.GetAllBranchesAsync(sub.Owner, sub.Repo);

        if (forceHistoryReload)
        {
            await SendHistoryAsync(sub, fetchLimit, forceHistoryReload, repoInfo);
            if (mode == "commit" || mode == "both")
            {
                foreach (var br in allBranches)
                {
                    var latestCommit = await _githubService.GetLatestCommitAsync(sub.Owner, sub.Repo, br.Name);
                    if (latestCommit != null)
                        state.BranchLastCommitSha[br.Name] = latestCommit.Sha;
                }
            }
            if (mode == "release" || mode == "both")
            {
                var latestRelease = await _githubService.GetLatestReleaseAsync(sub.Owner, sub.Repo);
                if (latestRelease != null)
                    state.LastReleaseTag = latestRelease.TagName;
            }
        }
        else
        {
            if (mode == "commit" || mode == "both")
                await CheckAllBranchesCommitUpdateAsync(sub, isNewState, state, repoInfo, allBranches);
        }

        if (mode == "release" || mode == "both")
            await CheckReleaseUpdateAsync(sub, state);

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

    private async Task CheckAllBranchesCommitUpdateAsync(
        SubscriptionConfig sub, bool isNewState, SubscriptionState state,
        GithubRepoInfo? repoInfo, List<GithubBranch> allBranches)
    {
        foreach (var branch in allBranches)
        {
            await CheckSingleBranchCommitAsync(sub, isNewState, state, repoInfo, branch.Name);
        }
    }

    private async Task CheckSingleBranchCommitAsync(
        SubscriptionConfig sub, bool isNewState, SubscriptionState state,
        GithubRepoInfo? repoInfo, string branchName)
    {
        var allCommits = await _githubService.GetRecentCommitsAsync(sub.Owner, sub.Repo, 10, branchName);
        string pullLog = $"[增量检测] Repo={sub.FullName} 分支={branchName} 拉取提交数={allCommits.Count}";
        _logger.LogInformation("[增量检测] Repo={Repo} Branch={Branch} 拉取提交数={Count}", sub.FullName, branchName, allCommits.Count);
        FileLogger.Info(pullLog);

        if (allCommits.Count == 0) return;
        var latestItem = allCommits[0];

        if (isNewState)
        {
            state.BranchLastCommitSha[branchName] = latestItem.Sha;
            string shortSha = latestItem.Sha.Length >= 7 ? latestItem.Sha[..7] : latestItem.Sha;
            string initLog = $"[首次启动初始化缓存] Repo={sub.FullName} 分支={branchName} 基准Sha={shortSha}，不推送历史提交";
            _logger.LogInformation("[首次启动初始化缓存] Repo={Repo} Branch={Branch} Sha={Sha}，跳过推送", sub.FullName, branchName, shortSha);
            FileLogger.Info(initLog);
            return;
        }

        string lastSavedSha = state.BranchLastCommitSha.TryGetValue(branchName, out var saved) ? saved : string.Empty;
        if (string.IsNullOrEmpty(lastSavedSha))
        {
            state.BranchLastCommitSha[branchName] = latestItem.Sha;
            string log = $"[新增分支缓存] Repo={sub.FullName} 分支={branchName} 首次记录Sha={latestItem.Sha[..7]}";
            _logger.LogInformation(log);
            FileLogger.Info(log);
            return;
        }

        List<GithubCommitItem> newCommits = new();
        foreach (var c in allCommits)
        {
            if (c.Sha == lastSavedSha) break;
            newCommits.Add(c);
        }

        if (newCommits.Count == 0)
        {
            string emptyLog = $"[增量检测] Repo={sub.FullName} 分支={branchName} 无新提交";
            _logger.LogInformation("[增量检测] Repo={Repo} Branch={Branch} 无新提交", sub.FullName, branchName);
            FileLogger.Info(emptyLog);
            return;
        }

        string newCountLog = $"[增量检测] Repo={sub.FullName} 分支={branchName} 新增{newCommits.Count}条提交";
        _logger.LogInformation("[增量检测] Repo={Repo} Branch={Branch} 新增{Count}条提交", sub.FullName, branchName, newCommits.Count);
        FileLogger.Info(newCountLog);

        newCommits.Reverse();
        foreach (var c in newCommits)
        {
            try
            {
                object card = await _kookService.BuildCommitCardObj(sub, c, repoInfo, branchName);
                bool ok = await _kookService.SendCardMessageAsync(card);
                string shortSha = c.Sha.Length >= 7 ? c.Sha[..7] : c.Sha;
                string sendLog = $"[增量卡片] Repo={sub.FullName} Branch={branchName} Sha={shortSha} 发送={ok}";
                _logger.LogInformation("[增量卡片] Repo={Repo} Branch={Branch} Sha={Sha} 发送={Res}", sub.FullName, branchName, shortSha, ok);
                FileLogger.Info(sendLog);
            }
            catch (Exception ex)
            {
                string err = $"增量卡片异常 分支={branchName} Sha={c.Sha}";
                _logger.LogError(ex, err);
                FileLogger.Error(err, ex);
            }
            await Task.Delay(300);
        }

        state.BranchLastCommitSha[branchName] = latestItem.Sha;
    }
}