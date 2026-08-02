using GithubKookBot.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Text.Json;

namespace GithubKookBot;

public partial class ConfigWindow : Form
{
    private readonly ConfigSettings _globalSettingRef;

    private readonly IServiceProvider _sp;
    private readonly BindingList<SubscriptionConfig> _subBindList = new();
    private readonly BindingSource _subBind = new();

    public ConfigWindow(ConfigSettings sourceRef, IServiceProvider sp)
    {
        InitializeComponent();
        _globalSettingRef = sourceRef;
        _sp = sp;
        dgvSub.AutoGenerateColumns = false;
        colOwner.DataPropertyName = "Owner";
        colRepo.DataPropertyName = "Repo";
        colDisp.DataPropertyName = "DisplayName";

        _subBind.DataSource = _subBindList;
        dgvSub.DataSource = _subBind;

        LoadDataToUi();
    }

    private void LoadDataToUi()
    {
        var s = _globalSettingRef;
        txtGithubToken.Text = s.GithubToken;
        txtKookToken.Text = s.KookBotToken;
        txtKookChannelId.Text = s.KookChannelId;
        nudInterval.Value = s.CheckIntervalMinutes;
        cmbMode.SelectedItem = s.UpdateMode;
        txtBaiduKey.Text = s.BaiduApiKey;
        txtBaiduSec.Text = s.BaiduSecretKey;
        nudFetch.Value = s.FetchCommitCount;

        _subBindList.Clear();
        foreach (var sub in s.Subscriptions)
            _subBindList.Add(sub);
    }

    private void SyncUiToGlobalSettingAndSave()
    {
        _globalSettingRef.GithubToken = txtGithubToken.Text.Trim();
        _globalSettingRef.KookBotToken = txtKookToken.Text.Trim();
        _globalSettingRef.KookChannelId = txtKookChannelId.Text.Trim();
        _globalSettingRef.CheckIntervalMinutes = (int)nudInterval.Value;
        _globalSettingRef.UpdateMode = cmbMode.SelectedItem?.ToString() ?? "Release";
        _globalSettingRef.BaiduApiKey = txtBaiduKey.Text.Trim();
        _globalSettingRef.BaiduSecretKey = txtBaiduSec.Text.Trim();
        _globalSettingRef.FetchCommitCount = (int)nudFetch.Value;

        _globalSettingRef.Subscriptions = _subBindList.ToList();

        SaveConfigToDisk();
        FileLogger.Info("配置已实时保存并同步至主程序内存");
    }

    private void SaveConfigToDisk()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "config.json");
        var root = new
        {
            Logging = new
            {
                LogLevel = new
                {
                    Default = "Information",
                    Microsoft = "Warning"
                }
            },
            Config = _globalSettingRef
        };
        string json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private void btnSaveAll_Click(object sender, EventArgs e)
    {
        SyncUiToGlobalSettingAndSave();
        MessageBox.Show("配置已保存，立即生效！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnAddSub_Click(object sender, EventArgs e)
    {
        _subBindList.Add(new SubscriptionConfig());
        SyncUiToGlobalSettingAndSave();
    }

    private void btnDelSub_Click(object sender, EventArgs e)
    {
        var toDelete = new List<SubscriptionConfig>();
        foreach (DataGridViewRow row in dgvSub.SelectedRows)
        {
            if (row.DataBoundItem is SubscriptionConfig sub)
            {
                toDelete.Add(sub);
            }
        }
        if (toDelete.Count == 0)
        {
            MessageBox.Show("请先选中要删除的订阅行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var confirm = MessageBox.Show($"确定删除选中的 {toDelete.Count} 条订阅？", "删除确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
            return;
        foreach (var item in toDelete)
        {
            _subBindList.Remove(item);
        }
        SyncUiToGlobalSettingAndSave();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        SyncUiToGlobalSettingAndSave();
        base.OnFormClosed(e);
    }
}