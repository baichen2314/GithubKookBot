using System.Net.Http.Headers;
using System.Text.Json;
using GithubKookBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GithubKookBot.Services;

public class GithubService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GithubService> _logger;
    private readonly ConfigSettings _settings;
    private const string ApiBase = "https://api.github.com";

    public GithubService(HttpClient httpClient, ILogger<GithubService> logger, ConfigSettings settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GithubKookBot");
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        if (!string.IsNullOrEmpty(_settings.GithubToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.GithubToken);
        }
    }

    public async Task<GithubRepoInfo?> GetRepoInfoAsync(string owner, string repo)
    {
        try
        {
            var url = $"{ApiBase}/repos/{owner}/{repo}";
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden
                && response.Headers.Contains("X-RateLimit-Remaining")
                && int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()) == 0)
            {
                string resetTs = response.Headers.GetValues("X-RateLimit-Reset").FirstOrDefault() ?? "0";
                long resetTimeStamp = long.TryParse(resetTs, out var t) ? t : 0;
                DateTime resetTime = DateTimeOffset.FromUnixTimeSeconds(resetTimeStamp).LocalDateTime;
                string logMsg = $"【GitHub API限流】{owner}/{repo} 请求超限，重置时间：{resetTime:yyyy-MM-dd HH:mm:ss}";
                _logger.LogWarning(logMsg);
                FileLogger.Warn(logMsg);
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                string logMsg = $"仓库 {owner}/{repo} 不存在";
                _logger.LogInformation(logMsg);
                FileLogger.Info(logMsg);
                return null;
            }
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var repoInfo = JsonSerializer.Deserialize<GithubRepoInfo>(json);
            return repoInfo;
        }
        catch (Exception ex)
        {
            string logMsg = $"获取 {owner}/{repo} 仓库信息失败";
            _logger.LogError(ex, logMsg);
            FileLogger.Error(logMsg, ex);
            return null;
        }
    }

    public async Task<GithubRelease?> GetLatestReleaseAsync(string owner, string repo)
    {
        try
        {
            var url = $"{ApiBase}/repos/{owner}/{repo}/releases/latest";
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden
                && response.Headers.Contains("X-RateLimit-Remaining")
                && int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()) == 0)
            {
                string resetTs = response.Headers.GetValues("X-RateLimit-Reset").FirstOrDefault() ?? "0";
                long resetTimeStamp = long.TryParse(resetTs, out var t) ? t : 0;
                DateTime resetTime = DateTimeOffset.FromUnixTimeSeconds(resetTimeStamp).LocalDateTime;
                string logMsg = $"【GitHub API限流】{owner}/{repo} Release查询超限，重置时间：{resetTime:yyyy-MM-dd HH:mm:ss}";
                _logger.LogWarning(logMsg);
                FileLogger.Warn(logMsg);
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                string logMsg = $"仓库 {owner}/{repo} 暂无 Release";
                _logger.LogInformation(logMsg);
                FileLogger.Info(logMsg);
                return null;
            }
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var release = JsonSerializer.Deserialize<GithubRelease>(json);
            return release;
        }
        catch (Exception ex)
        {
            string logMsg = $"获取 {owner}/{repo} 最新 Release 失败";
            _logger.LogError(ex, logMsg);
            FileLogger.Error(logMsg, ex);
            return null;
        }
    }

    public async Task<List<GithubCommitItem>> GetRecentCommitsAsync(string owner, string repo, int count = 20)
    {
        try
        {
            var url = $"{ApiBase}/repos/{owner}/{repo}/commits?per_page={count}";
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden
                && response.Headers.Contains("X-RateLimit-Remaining")
                && int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()) == 0)
            {
                string resetTs = response.Headers.GetValues("X-RateLimit-Reset").FirstOrDefault() ?? "0";
                long resetTimeStamp = long.TryParse(resetTs, out var t) ? t : 0;
                DateTime resetTime = DateTimeOffset.FromUnixTimeSeconds(resetTimeStamp).LocalDateTime;
                string logMsg = $"【GitHub API限流】{owner}/{repo} Commit查询超限，重置时间：{resetTime:yyyy-MM-dd HH:mm:ss}";
                _logger.LogWarning(logMsg);
                FileLogger.Warn(logMsg);
                return new List<GithubCommitItem>();
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var commits = JsonSerializer.Deserialize<List<GithubCommitItem>>(json);
            return commits ?? new List<GithubCommitItem>();
        }
        catch (Exception ex)
        {
            string logMsg = $"获取 {owner}/{repo} 最近提交失败";
            _logger.LogError(ex, logMsg);
            FileLogger.Error(logMsg);
            return new List<GithubCommitItem>();
        }
    }

    public async Task<GithubCommitItem?> GetLatestCommitAsync(string owner, string repo)
    {
        var commits = await GetRecentCommitsAsync(owner, repo, 1);
        return commits.FirstOrDefault();
    }
}