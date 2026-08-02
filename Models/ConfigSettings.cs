using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GithubKookBot.Models;

public class ConfigSettings : INotifyPropertyChanged
{
    private string _githubToken = string.Empty;
    private string _kookBotToken = string.Empty;
    private string _kookChannelId = string.Empty;
    private int _checkIntervalMinutes = 30;
    private string _updateMode = "Release";
    private List<SubscriptionConfig> _subscriptions = new();
    private string _baiduApiKey = string.Empty;
    private string _baiduSecretKey = string.Empty;
    private int _fetchCommitCount = 10;

    public string GithubToken
    {
        get => _githubToken;
        set { _githubToken = value; OnPropertyChanged(); }
    }

    public string KookBotToken
    {
        get => _kookBotToken;
        set { _kookBotToken = value; OnPropertyChanged(); }
    }

    public string KookChannelId
    {
        get => _kookChannelId;
        set { _kookChannelId = value; OnPropertyChanged(); }
    }

    public int CheckIntervalMinutes
    {
        get => _checkIntervalMinutes;
        set { _checkIntervalMinutes = value; OnPropertyChanged(); }
    }

    public string UpdateMode
    {
        get => _updateMode;
        set { _updateMode = value; OnPropertyChanged(); }
    }

    public List<SubscriptionConfig> Subscriptions
    {
        get => _subscriptions;
        set { _subscriptions = value; OnPropertyChanged(); }
    }

    public string BaiduApiKey
    {
        get => _baiduApiKey;
        set { _baiduApiKey = value; OnPropertyChanged(); }
    }

    public string BaiduSecretKey
    {
        get => _baiduSecretKey;
        set { _baiduSecretKey = value; OnPropertyChanged(); }
    }

    public int FetchCommitCount
    {
        get => _fetchCommitCount;
        set { _fetchCommitCount = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
