using GithubKookBot.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GithubKookBot.Services;

public class KookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KookService> _logger;
    private readonly ConfigSettings _settings;
    private const string ApiBase = "https://www.kookapp.cn";
    private string? _baiduAccessToken;
    private DateTime _tokenExpire = DateTime.MinValue;

    public KookService(HttpClient httpClient, ILogger<KookService> logger, ConfigSettings settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", _settings.KookBotToken);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
    }

    private DateTime UtcToBeijing(DateTime utcTime)
    {
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        }
        catch
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai");
        }
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
    }

    #region 消息发送入口

    public async Task<bool> SendTextMessageAsync(string content, string? channelId = null)
        => await SendRawMessageAsync(1, content, channelId);

    public async Task<bool> SendKMarkdownMessageAsync(string content, string? channelId = null)
        => await SendRawMessageAsync(9, content);

    public async Task<bool> SendCardMessageAsync(object singleCardObj, string? channelId = null)
    {
        var cardArray = new List<object> { singleCardObj };
        var jsonOpt = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        string cardJson = JsonSerializer.Serialize(cardArray);
        return await SendRawMessageAsync(10, cardJson, channelId);
    }

    private async Task<bool> SendRawMessageAsync(int msgType, string content, string? channelId = null)
    {
        try
        {
            string targetId = channelId ?? _settings.KookChannelId;
            var reqBody = new
            {
                type = msgType,
                target_id = targetId,
                content = content
            };
            string reqJson = JsonSerializer.Serialize(reqBody, new JsonSerializerOptions { WriteIndented = false });
            var httpContent = new StringContent(reqJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"{ApiBase}/api/v3/message/create", httpContent);
            string responseText = await response.Content.ReadAsStringAsync();
            string infoLog = $"KOOK接口 | MsgType={msgType} StatusCode={(int)response.StatusCode} Resp={responseText}";
            _logger.LogInformation("KOOK接口 | MsgType={MsgType} Code={Code} Resp={Resp}", msgType, (int)response.StatusCode, responseText);
            FileLogger.Info(infoLog);
            using var doc = JsonDocument.Parse(responseText);
            int bizCode = doc.RootElement.GetProperty("code").GetInt32();
            bool sendSuccess = bizCode == 0;
            if (!sendSuccess)
            {
                string errLog = $"KOOK发送失败 业务码={bizCode} 原始返回:{responseText}";
                _logger.LogError(errLog);
                FileLogger.Error(errLog);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            string errLog = $"发送消息异常 MsgType={msgType}";
            _logger.LogError(ex, errLog);
            FileLogger.Error(errLog, ex);
            return false;
        }
    }

    #endregion 消息发送入口

    #region 翻译

    private async Task<string> TranslateEnToCn(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;
        if (string.IsNullOrWhiteSpace(_settings.BaiduApiKey) || string.IsNullOrWhiteSpace(_settings.BaiduSecretKey))
        {
            _logger.LogDebug("未配置百度翻译密钥，跳过汉化，使用原始完整文本");
            return text;
        }
        string? token = await GetBaiduAccessToken();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("百度翻译AccessToken获取失败，跳过汉化");
            return text;
        }

        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        List<string> translatedLines = new List<string>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                translatedLines.Add("");
                continue;
            }
            string singleLineResult = await TranslateSingleLine(token, line);
            translatedLines.Add(singleLineResult);
        }
        return string.Join("\n", translatedLines);
    }

    private async Task<string> TranslateSingleLine(string accessToken, string lineText)
    {
        string url = $"https://aip.baidubce.com/rpc/2.0/mt/texttrans/v1?access_token={accessToken}";
        var reqBody = new { from = "en", to = "zh", q = lineText };
        string jsonReq = JsonSerializer.Serialize(reqBody);
        var jsonContent = new StringContent(jsonReq, Encoding.UTF8, "application/json");
        try
        {
            var resp = await _httpClient.PostAsync(url, jsonContent);
            string respText = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning($"单行翻译接口失败：{respText}，保留原文：{lineText}");
                return lineText;
            }
            var jsonOptions = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
            };
            var result = JsonSerializer.Deserialize<BaiduMtResp>(respText);
            if (result != null && string.IsNullOrEmpty(result.error_code) && result.result?.trans_result?.Any() == true)
            {
                return result.result.trans_result[0].dst;
            }
            string errInfo = result?.error_code ?? "未知";
            _logger.LogWarning($"单行翻译错误码{errInfo}，保留原文：{lineText}");
            return lineText;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"单行翻译异常，保留原文：{lineText}");
            return lineText;
        }
    }

    private List<string> ParseCoAuthors(string commitMsg)
    {
        var coAuthors = new List<string>();
        var lines = commitMsg.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimLine = line.Trim();
            if (trimLine.StartsWith("Co-authored-by:", StringComparison.OrdinalIgnoreCase))
            {
                var namePart = trimLine.Replace("Co-authored-by:", "", StringComparison.OrdinalIgnoreCase).Trim();
                if (namePart.Contains('<'))
                    namePart = namePart.Split('<')[0].Trim();
                if (!string.IsNullOrEmpty(namePart))
                    coAuthors.Add(namePart);
            }
        }
        return coAuthors;
    }

    private async Task<string?> GetBaiduAccessToken()
    {
        if (!string.IsNullOrEmpty(_baiduAccessToken) && DateTime.Now.AddMinutes(5) < _tokenExpire)
            return _baiduAccessToken;
        var formData = new Dictionary<string, string>()
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _settings.BaiduApiKey,
            ["client_secret"] = _settings.BaiduSecretKey
        };
        var formContent = new FormUrlEncodedContent(formData);
        var resp = await _httpClient.PostAsync("https://aip.baidubce.com/oauth/2.0/token", formContent);
        string respText = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            string log = $"获取百度Token失败，响应：{respText}";
            _logger.LogWarning(log);
            FileLogger.Warn(log);
            return null;
        }
        var tokenObj = JsonSerializer.Deserialize<BaiduTokenResp>(respText);
        if (tokenObj == null || string.IsNullOrWhiteSpace(tokenObj.access_token))
        {
            string log = "百度返回access_token为空，请检查API Key/Secret";
            _logger.LogWarning(log);
            FileLogger.Warn(log);
            return null;
        }
        _baiduAccessToken = tokenObj.access_token;
        _tokenExpire = DateTime.Now.AddSeconds(tokenObj.expires_in);
        return _baiduAccessToken;
    }

    private class BaiduTokenResp
    {
        public string access_token { get; set; } = string.Empty;
        public long expires_in { get; set; }
    }

    private class BaiduMtData
    {
        public List<BaiduMtItem> trans_result { get; set; } = new();
    }

    private class BaiduMtItem
    {
        public string src { get; set; } = string.Empty;
        public string dst { get; set; } = string.Empty;
    }

    private class BaiduMtResp
    {
        public BaiduMtData? result { get; set; }
        public long log_id { get; set; }
        public string? error_code { get; set; }
        public string? error_msg { get; set; }
    }

    #endregion 翻译

    #region Release

    public async Task<object> BuildReleaseCardObj(SubscriptionConfig sub, GithubRelease release)
    {
        string displayName = string.IsNullOrEmpty(sub.DisplayName) ? sub.FullName : sub.DisplayName;
        string version = string.IsNullOrEmpty(release.Name) ? release.TagName : release.Name;
        string preText = release.Prerelease ? "【预发布版本】" : "";
        string rawBody = release.Body ?? "暂无更新说明";
        Regex dateRegexAll = new Regex(@"\d{2}/\d{2}(/\d{2})?");
        List<string> dateOriginals = new List<string>();
        string tempBody = dateRegexAll.Replace(rawBody, m =>
        {
            dateOriginals.Add(m.Value);
            return $"《DATE_{dateOriginals.Count - 1}》";
        });
        string zhBody = await TranslateEnToCn(tempBody);
        for (int i = 0; i < dateOriginals.Count; i++)
        {
            zhBody = zhBody.Replace($"《DATE_{i}》", dateOriginals[i]);
        }
        const int MaxReleaseTextLength = 1500;
        if (zhBody.Length > MaxReleaseTextLength)
        {
            zhBody = zhBody[..MaxReleaseTextLength] + "\n……内容过长，前往发布详情页查看完整更新日志";
        }

        KookCard card = new();
        card.theme = release.Prerelease ? "warning" : "success";
        card.modules.Add(new KookHeaderModule
        {
            text = new KookPlainText { content = $"🚀 {displayName} 新版本发布" }
        });
        card.modules.Add(new KookDividerModule());

        if (release.Author != null)
        {
            var ctx = new KookContextModule();
            ctx.elements.Add(new KookMarkdownText { content = "发布者：" });
            ctx.elements.Add(new KookImage { src = release.Author.AvatarUrl, size = "sm" });
            ctx.elements.Add(new KookMarkdownText { content = release.Author.Login });
            card.modules.Add(ctx);
        }
        DateTime beijingPublish = UtcToBeijing(release.PublishedAt);
        card.modules.Add(new KookSection
        {
            text = new KookMarkdownText { content = $"版本：**{preText}{version}**" }
        });
        card.modules.Add(new KookSection
        {
            text = new KookMarkdownText { content = $"发布时间：**{beijingPublish:yyyy-MM-dd HH:mm}**" }
        });
        card.modules.Add(new KookDividerModule());

        card.modules.Add(new KookSection
        {
            text = new KookMarkdownText { content = $"**更新日志：**\n{zhBody}" }
        });
        card.modules.Add(new KookDividerModule());

        KookActionGroup action = new();
        action.elements.Add(new KookButton
        {
            theme = "primary",
            click = "link",
            value = release.HtmlUrl,
            text = new KookPlainText { content = "前往查看发布详情" }
        });
        card.modules.Add(action);
        return card;
    }

    #endregion Release

    #region Commit卡片

    public async Task<object> BuildCommitCardObj(SubscriptionConfig sub, GithubCommitItem commit, GithubRepoInfo? repoInfo, string branchName)
    {
        string displayName = string.IsNullOrEmpty(sub.DisplayName) ? sub.FullName : sub.DisplayName;
        string fullRawMsg = commit.Commit.Message;
        string[] msgLines = fullRawMsg.Split(["\r\n", "\n"], StringSplitOptions.None);
        string shortMsg = msgLines.Length > 0 ? msgLines[0].Trim() : string.Empty;
        string bodyText = string.Empty;
        if (msgLines.Length >= 3)
        {
            bodyText = string.Join("\n", msgLines.Skip(2)).Trim();
        }
        else if (msgLines.Length >= 2 && !string.IsNullOrWhiteSpace(msgLines[1]))
        {
            bodyText = string.Join("\n", msgLines.Skip(1)).Trim();
        }
        string authorName = commit.Author?.Login ?? commit.Commit.Author?.Name ?? "未知";
        string authorAvatar = commit.Author?.AvatarUrl ?? "";
        string commitTime;
        if (commit.Commit.Author != null)
        {
            DateTime utcDate = commit.Commit.Author.Date;
            DateTime bjDate = UtcToBeijing(utcDate);
            commitTime = bjDate.ToString("yyyy/MM/dd HH:mm");
        }
        else
        {
            commitTime = "未知时间";
        }
        string repoUrl = $"https://github.com/{sub.FullName}";
        string commitUrl = commit.HtmlUrl;
        Regex dateRegexAll = new Regex(@"\d{2}/\d{2}(/\d{2})?");
        List<string> dateOriginals = new List<string>();
        string tempShort = dateRegexAll.Replace(shortMsg, m =>
        {
            dateOriginals.Add(m.Value);
            return $"《DATE_{dateOriginals.Count - 1}》";
        });
        string zhTitle = await TranslateEnToCn(tempShort);
        for (int i = 0; i < dateOriginals.Count; i++)
        {
            zhTitle = zhTitle.Replace($"《DATE_{i}》", dateOriginals[i]);
        }
        string zhBody = string.Empty;
        if (!string.IsNullOrWhiteSpace(bodyText))
        {
            List<string> bodyDateMarkers = new List<string>();
            string tempBody = dateRegexAll.Replace(bodyText, m =>
            {
                bodyDateMarkers.Add(m.Value);
                return $"《BDATE_{bodyDateMarkers.Count - 1}》";
            });
            zhBody = await TranslateEnToCn(tempBody);
            for (int i = 0; i < bodyDateMarkers.Count; i++)
            {
                zhBody = zhBody.Replace($"《BDATE_{i}》", bodyDateMarkers[i]);
            }
            const int MaxBodyLength = 1200;
            if (zhBody.Length > MaxBodyLength)
            {
                zhBody = zhBody[..MaxBodyLength] + "\n……内容过长，点击查看完整提交";
            }
        }
        var coAuthors = ParseCoAuthors(fullRawMsg);
        KookCard card = new();
        card.modules.Add(new KookHeaderModule
        {
            text = new KookPlainText { content = $"🎉 {displayName} 有新提交" }
        });
        card.modules.Add(new KookDividerModule());
        var contextModule = new KookContextModule();
        contextModule.elements.Add(new KookMarkdownText { content = $"提交作者：" });
        contextModule.elements.Add(new KookImage { src = authorAvatar, size = "sm" });
        contextModule.elements.Add(new KookMarkdownText { content = authorName });
        card.modules.Add(contextModule);
        card.modules.Add(new KookSection { text = new KookMarkdownText { content = $"仓库：(font){sub.FullName}(font)[danger]" } });
        card.modules.Add(new KookSection { text = new KookMarkdownText { content = $"仓库分支：(font){branchName}(font)[success]" } });
        card.modules.Add(new KookSection { text = new KookMarkdownText { content = $"**更新内容：** {zhTitle}" } });
        if (!string.IsNullOrWhiteSpace(zhBody))
        {
            card.modules.Add(new KookSection { text = new KookMarkdownText { content = $"\n{zhBody}" } });
        }
        if (coAuthors.Any())
        {
            string coText = $"共同作者：{string.Join("、", coAuthors)}";
            card.modules.Add(new KookSection { text = new KookMarkdownText { content = coText } });
        }
        card.modules.Add(new KookSection { text = new KookMarkdownText { content = $"提交时间：**{commitTime}**" } });
        card.modules.Add(new KookDividerModule());
        KookActionGroup actionGroup = new();
        actionGroup.elements.Add(new KookButton
        {
            theme = "primary",
            value = repoUrl,
            text = new KookPlainText { content = "仓库主页" }
        });
        actionGroup.elements.Add(new KookButton
        {
            theme = "danger",
            value = commitUrl,
            text = new KookPlainText { content = "查看提交" }
        });
        card.modules.Add(actionGroup);
        return card;
    }

    #endregion Commit卡片
}