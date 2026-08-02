using System.Text.Json.Serialization;

namespace GithubKookBot.Models;

public class GithubRepoInfo
{
    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; } = string.Empty;
}