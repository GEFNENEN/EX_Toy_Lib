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

    public class PoofLibrarySettingPage
    {
        private double connectionStartTime;
        private ConnectionStatus connectionStatus = ConnectionStatus.Pending;
        private string debugUrl;
        private bool isTestingConnection;
        private double responseTime;
        
        [Title("网络状态", Bold = false)]
        [VerticalGroup(PoofLibraryConstParam.SETTING_GROUP_SUB_1)]
        [ShowInInspector]
        [HideLabel]
        [ReadOnly]
        [TextArea(1, 20)]
        [PropertyOrder(1)]
        private string connectionMessage = "准备测试连接";
        

        [BoxGroup(PoofLibraryConstParam.SETTING_GROUP)]
        [ShowInInspector]
        [LabelText("连接仓库地址")]
        [LabelWidth(150)]
        [DisplayAsString(EnableRichText = true)]
        private string GitRepoUrl = $"<color=white>{PoofLibraryConstParam.DEFAULT_GIT_REPO_URL}</color>";

        [BoxGroup(PoofLibraryConstParam.SETTING_GROUP)]
        [InfoBox("访问令牌(可选):\n• 私有仓库必须提供\n• 避免GitHub速率限制\n• 创建地址: https://github.com/settings/tokens")]
        [ShowInInspector]
        [LabelText("GitHub 令牌")]
        [LabelWidth(150)]
        private string gitToken = "";

        [HorizontalGroup(PoofLibraryConstParam.SETTING_GROUP_SUB_CONNECTION)]
        [PropertyOrder(3)]
        [ShowInInspector]
        [DisplayAsString(EnableRichText = true)]
        [HideLabel]
        private string connectionInfo => $"<color=orange>状态:{connectionStatus}</color>  响应时间:{responseTime:0.00}ms";

        // 当前poof lib目录状态：
        // 1.目录版本号; 2.按钮：下载目录/打开目录文件夹
        [Title("目录", Bold = false)]
        [BoxGroup(PoofLibraryConstParam.SETTING_GROUP)]
        [ShowInInspector]
        [DisplayAsString]
        [PropertyOrder(6)]
        [LabelText("目录路径")]
        [LabelWidth(150)]
        public string Message => ExistMenuJson ? PoofLibraryConstParam.DEFAULT_MENU_PATH : "未找到配置文件";

        [BoxGroup(PoofLibraryConstParam.SETTING_GROUP)]
        [ShowInInspector]
        [DisplayAsString]
        [LabelText("目录版本")]
        [LabelWidth(150)]
        [PropertyOrder(7)]
        [ShowIf(nameof(ExistMenuJson))]
        public string DefaultMenuVersion => PoofLibraryConstParam.DEFAULT_MENU_PATH;

        private bool ExistMenuJson =>
            File.Exists(Path.Combine(Application.dataPath, "../", PoofLibraryConstParam.DEFAULT_MENU_PATH));

        [VerticalGroup(PoofLibraryConstParam.SETTING_GROUP_SUB_1)]
        [HorizontalGroup(PoofLibraryConstParam.SETTING_GROUP_SUB_CONNECTION, 150)]
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
                gitToken);

            isTestingConnection = true;
            EditorCoroutineHelper.Start(TestConnectionCoroutine());
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
                                            $"连接成功！\n" +
                                            "HTTP 状态: 200 OK\n" +
                                            $"响应时间: {totalTime:0} ms";
                    else
                        connectionMessage = $"测试地址[{debugUrl}]\n" +
                                            $"连接成功但文件可能不存在\n" +
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

                _ => $"测试地址[{debugUrl}]\n" +$"连接失败: {request.error} (HTTP {request.responseCode})"
            };
        }

        [BoxGroup(PoofLibraryConstParam.SETTING_GROUP)]
        [HideIf(nameof(ExistMenuJson))]
        [PropertyOrder(8)]
        [Button(ButtonSizes.Large, Name = "下载目录")]
        [GUIColor(0.4f, 0.8f, 1f)]
        public void DownloadMenuJson()
        {
            var directory = Path.Combine(Application.dataPath, "_PoofLibrary");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            var fullPath = Path.Combine(directory, "menu.json");

            if (!File.Exists(fullPath))
            {
                var config = new PoofLibraryManagerWindow.MenuConfig
                {
                    menuItems = new List<PoofLibraryManagerWindow.MenuItem>
                    {
                        new()
                        {
                            menuPath = "示例/角色模型",
                            assetPath = "Assets/Characters/Hero.prefab",
                            description = "主角角色模型"
                        },
                        new()
                        {
                            menuPath = "示例/武器资源",
                            assetPath = "Assets/Weapons/Sword.prefab",
                            description = "基础武器预制体"
                        }
                    }
                };

                File.WriteAllText(fullPath, JsonUtility.ToJson(config, true));
                AssetDatabase.Refresh();
                Debug.Log($"已创建示例配置: {fullPath}");
            }
            else
            {
                Debug.LogWarning("配置文件已存在");
            }
        }



        [Title("连接仓库配置",Bold = false)]
        public PoofLibrarySetting Setting;
    }
}