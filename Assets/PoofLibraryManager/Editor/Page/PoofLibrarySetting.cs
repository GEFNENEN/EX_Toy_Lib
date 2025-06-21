using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace PoofLibraryManager.Editor
{
    public enum ConnectionStatus
    {
        [LabelText("待检测")] Pending,
        [LabelText("检测中")] Checking,
        [LabelText("连接成功")] Success,
        [LabelText("连接失败")] Failed
    }
    
    [FilePath(PoofLibraryConstParam.SETTING_ASSET_PATH)]
    public class PoofLibrarySetting : PLScriptableSingleton<PoofLibrarySetting>
    {
        [VerticalGroup(PoofLibraryConstParam.REPO_SETTING)]
        [InfoBox(PoofLibraryConstParam.REPO_TOKEN_INTRO)]
        [ShowInInspector]
        [LabelText(PoofLibraryConstParam.REPO_TOKEN)]
        [LabelWidth(150)]
        public string Token = "";

        [Title(PoofLibraryConstParam.REPO_CONNECTION_TITLE)]
        [VerticalGroup(PoofLibraryConstParam.REPO_SETTING)]
        [LabelText(" ")]
        [PropertyOrder(10)]
        [Space(15)]
        [ListDrawerSettings]
        public List<RepoInfo> repoInfos = new()
        {
            new RepoInfo
            {
                gitRepoUrl = PoofLibraryConstParam.DEFAULT_GIT_REPO_URL,
                menuJsonPath = PoofLibraryConstParam.DEFAULT_MENU_PATH,
                localMenuJsonPath = PoofLibraryConstParam.DEFAULT_MENU_PATH
            }
        };

        //[Title("网络状态", Bold = false)] 
        [BoxGroup(PoofLibraryConstParam.REPO_SETTING_GROUP_SUB_1)]
        [GUIColor(0.8f, 1f, 0.8f)] 
        [ShowInInspector] [HideLabel] [ReadOnly] [TextArea(1, 20)] [PropertyOrder(1)]
        private string connectionMessage = "准备测试连接";

        private double connectionStartTime;
        private ConnectionStatus connectionStatus = ConnectionStatus.Pending;
        private string debugUrl;
        private bool isTestingConnection;
        private double responseTime;

        
        [BoxGroup(PoofLibraryConstParam.REPO_SETTING_GROUP_SUB_1)]
        [HorizontalGroup(PoofLibraryConstParam.REPO_SETTING_GROUP_SUB_CONNECTION, 150)]
        [PropertyOrder(2)]
        [Button("检测网络连接", ButtonSizes.Medium)]
        public void CheckConnectionToGitRepo()
        {
            if (isTestingConnection) return;

            connectionStatus = ConnectionStatus.Checking;
            connectionMessage = "正在测试连接...";
            connectionStartTime = EditorApplication.timeSinceStartup;

            // 生成 URL
            debugUrl = FormatGitHubUrl(PoofLibraryConstParam.GIT_REPO_RAW_URL, PoofLibraryConstParam.DEFAULT_MENU_PATH,
                Token);

            isTestingConnection = true;
            EditorCoroutineHelper.Start(TestConnectionCoroutine());
        }

        [HorizontalGroup(PoofLibraryConstParam.REPO_SETTING_GROUP_SUB_CONNECTION)]
        [PropertyOrder(3)]
        [ShowInInspector]
        [DisplayAsString(EnableRichText = true)]
        [HideLabel]
        private string connectionInfo => $"<color=orange>状态:{connectionStatus}</color>  响应时间:{responseTime:0.00}ms";

        
        // 测试连接协程
        private IEnumerator TestConnectionCoroutine()
        {
            using (var request = UnityWebRequest.Head(debugUrl))
            {
                // 设置请求参数
                request.timeout = 10;
                request.SetRequestHeader("User-Agent", "UnityEditor/" + Application.unityVersion);

                // 开始请求
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    connectionMessage = $"正在连接... ({Mathf.FloorToInt(request.downloadProgress * 100)}%)";
                    yield return null;
                }

                // 计算响应时间
                var totalTime = (EditorApplication.timeSinceStartup - connectionStartTime) * 1000;
                responseTime = totalTime;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    connectionStatus = ConnectionStatus.Success;

                    if (request.responseCode == 200)
                        connectionMessage = $"测试地址[{debugUrl}]\n" +
                                            "连接成功！\n" +
                                            "HTTP 状态: 200 OK\n" +
                                            $"响应时间: {totalTime:0} ms";
                    else
                        connectionMessage = $"测试地址[{debugUrl}]\n" +
                                            "连接成功但文件可能不存在\n" +
                                            $"HTTP 状态: {request.responseCode}\n" +
                                            $"响应时间: {totalTime:0} ms";
                }
                else
                {
                    connectionStatus = ConnectionStatus.Failed;
                    connectionMessage = HandleConnectionError(request, debugUrl);
                }

                isTestingConnection = false;
            }
        }

        // 处理连接错误
        private string HandleConnectionError(UnityWebRequest request, string url)
        {
            return request.responseCode switch
            {
                401 or 403 => $"测试地址[{debugUrl}]\n" +
                              $"访问被拒绝 (HTTP {request.responseCode})\n" +
                              "• 私有仓库？请添加 GitHub 令牌",

                404 => $"测试地址[{debugUrl}]\n" +
                       "文件未找到 (HTTP 404)\n" +
                       $"• 检查文件路径: {PoofLibraryConstParam.DEFAULT_MENU_PATH}",

                429 => $"测试地址[{debugUrl}]\n" +
                       "\u26a0GitHub 速率限制\n" +
                       "• 添加 GitHub 令牌可提高限制",

                _ when request.result == UnityWebRequest.Result.ConnectionError =>
                    $"测试地址[{debugUrl}]\n" +
                    "网络连接失败\n" +
                    "请检查网络连接，DNS是否正常。",
                //$"• 尝试在浏览器中打开: {sanitizedUrl}",

                _ => $"测试地址[{debugUrl}]\n" + $"连接失败: {request.error} (HTTP {request.responseCode})"
            };
        }

        // 格式化 GitHub URL
        private string FormatGitHubUrl(string repoUrl, string path, string token = "")
        {
            var encodedPath = path.Replace(" ", "%20");
            encodedPath = UnityWebRequest.EscapeURL(encodedPath)
                .Replace("%3A", ":")
                .Replace("%2F", "/")
                .Replace("%5C", "/");

            var url = $"{repoUrl}/{encodedPath}";

            if (!string.IsNullOrWhiteSpace(token))
                url += $"?token={token}";

            return url;
        }
    }

    [Serializable]
    public struct RepoInfo
    {
        [LabelText("仓库地址")] [LabelWidth(150)][PropertyOrder(1)] 
        public string gitRepoUrl;

        [HideLabel]
        [ShowInInspector]
        [DisplayAsString(EnableRichText = true)]
        [PropertyOrder(1)]
        public string rawContentGitRepoUrl =>
            $"<color=white>下载内容地址(Raw URL):{gitRepoUrl.Replace("github.com", "raw.githubusercontent.com")}</color>";

        [Space] [LabelText("远端菜单路径")] [LabelWidth(150)]
        [PropertyOrder(2)] 
        public string menuJsonPath;

        [LabelText("本地菜单路径")] [LabelWidth(150)]
        [PropertyOrder(3)] 
        public string localMenuJsonPath;

        [ShowIf(nameof(ExistMenuJson))]
        [ShowInInspector]
        [DisplayAsString(EnableRichText = true)]
        [HideLabel]
        [PropertyOrder(4)] 
        private string MenuVersion
        {
            get
            {
                var version = "1.0";
                return $"菜单目录版本: {version}";
            }
        }

        [Button("下载菜单")]
        [HideIf(nameof(ExistMenuJson))]
        [PropertyOrder(5)] 
        public void LoadMenu()
        {
        }

        [Button("更新菜单")]
        [ShowIf(nameof(ExistMenuJson))]
        [PropertyOrder(5)] 
        public void UpdateMenu()
        {
            LoadMenu();
        }

        private bool ExistMenuJson()
        {
            var isExist = File.Exists(Path.Combine(Application.dataPath, "../", localMenuJsonPath));
            return isExist;
        }
    }
}