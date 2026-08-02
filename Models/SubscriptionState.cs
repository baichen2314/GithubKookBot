namespace GithubKookBot.Models;

public class SubscriptionState
{
    public string RepoFullName { get; set; } = string.Empty;
    public string LastReleaseTag { get; set; } = string.Empty;
    public string LastCommitSha { get; set; } = string.Empty;
    public DateTime LastCheckedAt { get; set; } = DateTime.MinValue;
}