using GithubKookBot.Models;
using GithubKookBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GithubKookBot;

public partial class Main : Form
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly ConfigSettings? _appSettings;
    private UpdateChecker? _updateChecker;
    private GithubService? _githubService;
    private KookService? _kookService;
    private StateStore? _stateStore;
    private bool _isTimerRunning = false;
    private DateTime _nextCheckTime;

    public Main()
    {
        InitializeComponent();
        uiRefreshTimer!.Start();
        FileLogger.OnLogOutput = WriteLogToUi;
    }

    public Main(IServiceProvider sp) : this()
    {
        _serviceProvider = sp;
        _appSettings = sp.GetRequiredService<ConfigSettings>();
    }

    private void LogInfo(string msg)
    {
        FileLogger.Info(msg);
    }

    private void LogError(string msg, Exception? ex = null)
    {
        FileLogger.Error(msg, ex);
    }

    private void WriteLogToUi(string text)
    {
        if (rtbLog.InvokeRequired)
        {
            rtbLog.Invoke(() => WriteLogToUi(text));
            return;
        }
        rtbLog.AppendText(text + Environment.NewLine);
        rtbLog.ScrollToCaret();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        if (DesignMode) return;
        try
        {
            _updateChecker = _serviceProvider!.GetRequiredService<UpdateChecker>();
            _githubService = _serviceProvider!.GetRequiredService<GithubService>();
            _kookService = _serviceProvider!.GetRequiredService<KookService>();
            _stateStore = _serviceProvider!.GetRequiredService<StateStore>();
            LogInfo("程序启动成功！");
            LogInfo($"当前订阅仓库数量: {_appSettings!.Subscriptions.Count}");
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(_appSettings!.KookBotToken) && !string.IsNullOrEmpty(_appSettings!.KookChannelId) && _appSettings!.Subscriptions.Any())
                    {
                        bool sendOk = await _kookService!.SendKMarkdownMessageAsync("🤖 GitHub 同步机器人已启动");
                        LogInfo($"启动欢迎消息推送：{(sendOk ? "成功" : "失败")}");
                        await _updateChecker!.CheckAllAsync(true, false, _appSettings!.FetchCommitCount);
                        LogInfo("首次初始化仓库检测完成");
                    }
                    else
                    {
                        LogInfo("配置缺失/无订阅，跳过首次检测");
                    }
                }
                catch (Exception ex)
                {
                    LogError("后台初始化任务异常", ex);
                }
            });
        }
        catch (Exception ex)
        {
            LogError("窗体初始化失败", ex);
            MessageBox.Show("启动失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnOpenConfig_Click(object sender, EventArgs e)
    {
        using var cfgWin = new ConfigWindow(_appSettings!, _serviceProvider!);
        cfgWin.ShowDialog();
        LogInfo("设置窗口已关闭，配置已自动同步");
        if (_isTimerRunning)
        {
            int intervalMs = _appSettings!.CheckIntervalMinutes * 60 * 1000;
            timerCheck.Interval = intervalMs;
            _nextCheckTime = DateTime.Now.AddMinutes(_appSettings!.CheckIntervalMinutes);
            UpdateTimerLabel();
            LogInfo("已重载定时检查间隔：" + _appSettings!.CheckIntervalMinutes + "分钟");
        }
    }

    private async void btnCheckNow_Click(object sender, EventArgs e)
    {
        var confirm = MessageBox.Show("手动推送会一次性大量调用GitHub API，频繁操作会触发403限流，是否继续？",
            "API限流风险提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
            return;

        if (_updateChecker == null) return;
        btnCheckNow.Enabled = false;
        lblStatus.Text = "执行测试推送...";
        LogInfo("手动触发全仓库历史推送检测");
        try
        {
            await _updateChecker.CheckAllAsync(false, true, _appSettings!.FetchCommitCount);
            lblStatus.Text = "测试推送完成";
            LogInfo("手动推送执行完成");
        }
        catch (Exception ex)
        {
            LogError("手动检测异常", ex);
            lblStatus.Text = "推送失败";
        }
        finally
        {
            btnCheckNow.Enabled = true;
            UpdateTimerLabel();
        }
    }

    private void btnToggleTimer_Click(object sender, EventArgs e)
    {
        if (_isTimerRunning) StopTimer();
        else StartTimer();
    }

    private void StartTimer()
    {
        if (string.IsNullOrWhiteSpace(_appSettings!.KookBotToken)
            || string.IsNullOrWhiteSpace(_appSettings!.KookChannelId))
        {
            MessageBox.Show("请先打开设置配置KOOK信息", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!_appSettings!.Subscriptions.Any())
        {
            MessageBox.Show("请在设置中添加订阅仓库", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        int intervalMs = _appSettings!.CheckIntervalMinutes * 60 * 1000;
        timerCheck.Interval = intervalMs;
        timerCheck.Start();
        _isTimerRunning = true;
        _nextCheckTime = DateTime.Now.AddMinutes(_appSettings!.CheckIntervalMinutes);
        btnToggleTimer.Text = "停止定时检查";
        lblStatus.Text = "定时运行中";
        UpdateTimerLabel();
        LogInfo($"定时任务启动，轮间隔 {_appSettings!.CheckIntervalMinutes} 分钟({intervalMs}ms)");
    }

    private void StopTimer()
    {
        timerCheck.Stop();
        _isTimerRunning = false;
        btnToggleTimer.Text = "启动定时检查";
        lblStatus.Text = "定时已停止";
        lblNextCheck.Text = "";
        LogInfo("定时任务已停止");
    }

    private async void timerCheck_Tick(object sender, EventArgs e)
    {
        timerCheck.Stop();
        lblStatus.Text = "定时轮询检测中...";
        LogInfo("定时轮询开始执行");
        try
        {
            await _updateChecker!.CheckAllAsync(false, false, _appSettings!.FetchCommitCount);
            LogInfo("定时轮询执行完毕");
        }
        catch (Exception ex)
        {
            LogError("定时轮询异常", ex);
        }
        finally
        {
            if (_isTimerRunning)
            {
                int intervalMs = _appSettings!.CheckIntervalMinutes * 60 * 1000;
                timerCheck.Interval = intervalMs;
                _nextCheckTime = DateTime.Now.AddMinutes(_appSettings!.CheckIntervalMinutes);
                timerCheck.Start();
            }
            UpdateTimerLabel();
        }
    }

    private void UiRefreshTimer_Tick(object sender, EventArgs e)
    {
        UpdateTimerLabel();
    }

    private void UpdateTimerLabel()
    {
        if (!_isTimerRunning)
        {
            lblNextCheck.Text = "";
            return;
        }
        var diff = _nextCheckTime - DateTime.Now;
        if (diff.TotalSeconds <= 0)
            lblNextCheck.Text = "即将检测";
        else
            lblNextCheck.Text = $"下次：{_nextCheckTime:HH:mm:ss} 剩余{diff.Minutes}分{diff.Seconds}秒";
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_isTimerRunning) StopTimer();
        uiRefreshTimer?.Dispose();
        if (_stateStore != null)
        {
            _stateStore!.SaveState();
            _stateStore!.Dispose();
        }
    }
}