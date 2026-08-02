using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GithubKookBot.Models;

public class SubscriptionConfig : INotifyPropertyChanged
{
    private string _owner = string.Empty;
    private string _repo = string.Empty;
    private string _displayName = string.Empty;

    public string Owner
    {
        get => _owner;
        set { _owner = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); }
    }

    public string Repo
    {
        get => _repo;
        set { _repo = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); }
    }

    public string DisplayName
    {
        get => _displayName;
        set { _displayName = value; OnPropertyChanged(); }
    }

    public string FullName => $"{Owner}/{Repo}";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}