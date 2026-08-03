# GithubKookBot

项目：GithubKookBot

### 简要说明：
这是一个基于 .NET 10 的机器人项目（GithubKookBot），用于将添加的GitHub项目相关更新或发布内容转发到Kook响应频道。

## 前置条件
- .NET 10 SDK
- KookBotToken（在 [KOOK 开发者平台](https://developer.kookapp.cn/) 创建机器人获取），KookChannelId需要选中机器人所在的频道，获取频道ID。
- 机器人需要拥有 **管理角色** 权限
- 如果需要百度翻译进行汉化内容，准备ApiKey和SecretKey，需要在 百度 [大模型文本翻译](https://login.bce.baidu.com/ )网站进行申请开发者注册，创建应用后获取。

## 快速开始
1. 克隆仓库：

   git clone https://github.com/baichen2314/GithubKookBot.git
   cd GithubKookBot

2. `config.json `配置（示例）
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning"
       }
     },
     "Config": {
       "GithubToken": "",
       "KookBotToken": "xxxxxxxxxxxxxxxxxxx",
       "KookChannelId": "xxxxxxxxxxxxxxxxxx",
       "CheckIntervalMinutes": 1,
       "UpdateMode": "Commit",
       "Subscriptions": [
         {
           "Owner": "xx",
           "Repo": "xxxx",
           "DisplayName": "xxxx",
           "FullName": "xx/xxxx"
         }
       ],
       "BaiduApiKey": "",
       "BaiduSecretKey": "",
       "FetchCommitCount": 2
     }
   }

  | 字段 | 说明 |
  |------|------|
  | `GithubToken` | 用于访问 GitHub API 的个人访问令牌（Personal Access Token）""可不填写"" |
  | `KookBotToken` | Kook 机器人在开发者控制台创建后得到的 Bot Token，用于通过 API 发送消息给频道 |
  | `KookChannelId` | 机器人发送目标频道的 ID（字符串或数字，获取方法见前置条件） |
  | `CheckIntervalMinutes` | 检查更新的时间间隔（以分钟为单位），例如 1 表示每分钟检查一次 |
  | `UpdateMode` | 更新检测模式|
  | `Subscriptions` | 订阅仓库列表，每项为一个对象，字段说明： |
  |   `Owner` | 仓库所有者（用户名或组织名），例如 "dotnet"。 |
  |   `Repo` | 仓库名称，例如 "runtime"。 |
  |   `DisplayName` | 自己设置的备注，用于区分同一账号下的多个仓库。 |
  |   `FullName` | 完整仓库名，通常为 "Owner/Repo"|
  | `BaiduApiKey`| 用于调用百度翻译的 API Key（可选）。若不需要翻译功能，可留空。|
  | `BaiduSecretKey` | 用于调用百度翻译的Secret Key（可选）。若不需要翻译功能，可留空。 |
  | `FetchCommitCount` | 每次检查时抓取的提交数量上限（用于在消息中显示最近若干条提交）。 |
  
### 构建与发布
- 发布可执行文件：dotnet publish -c Release -o ./publish

### 应用界面展示
![9f1z083j](https://github.com/user-attachments/assets/7f001e12-925b-4939-bf79-dc7e32e2210f)
![9f1z083j](https://github.com/user-attachments/assets/c356e5b2-cd63-4545-8021-5d7e3dc4d5e6)
![9f1z083j](https://github.com/user-attachments/assets/c356e5b2-cd63-4545-8021-5d7e3dc4d5e6)
### 贡献
- 欢迎提交 issue 或 PR。请遵循标准的 GitHub 工作流：fork -> feature branch -> PR -> Code review。
