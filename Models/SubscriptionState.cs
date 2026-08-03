namespace GithubKookBot.Models;

public class SubscriptionState
{
    public string RepoFullName { get; set; } = string.Empty;
    public string LastReleaseTag { get; set; } = string.Empty;
    public Dictionary<string, string> BranchLastCommitSha { get; set; } = new();
    public DateTime LastCheckedAt { get; set; } = DateTime.MinValue;
}