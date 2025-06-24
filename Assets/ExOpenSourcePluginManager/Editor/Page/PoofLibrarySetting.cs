using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Unity.EditorCoroutines.Editor;
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

    [PLFilePath(PoofLibraryConstParam.SETTING_ASSET_PATH)]
    public class PoofLibrarySetting : PLScriptableSingleton<PoofLibrarySetting>
    {
        [BoxGroup(PoofLibraryConstParam.REPO_TOKEN_GROUP)]
        [InfoBox(PoofLibraryConstParam.REPO_TOKEN_INTRO)]
        [LabelText(PoofLibraryConstParam.REPO_TOKEN_FILE_PATH)]
        [LabelWidth(150)]
        [Sirenix.OdinInspector.FilePath(RequireExistingPath = true, Extensions = "txt", AbsolutePath = true,
            IncludeFileExtension = true)]
        [InlineButton(nameof(ReadToken), SdfIconType.Upload, "加载令牌")]
        public string TokenFilePath = "";

        [Title(PoofLibraryConstParam.REPO_CONNECTION_TITLE)]
        [VerticalGroup(PoofLibraryConstParam.REPO_SETTING)]
        [LabelText(" ")]
        [PropertyOrder(10)]
        [Space(15)]
        [ListDrawerSettings]
        public RepoInfo[] repoInfos = PoofLibraryConstParam.OfficialRepoInfos;

        //[Title("网络状态", Bold = false)] 
        [BoxGroup(PoofLibraryConstParam.REPO_SETTING_GROUP_SUB_1)]
        [GUIColor(0.8f, 1f, 0.8f)]
        [ShowInInspector]
        [HideLabel]
        [ReadOnly]
        [TextArea(1, 20)]
        [PropertyOrder(1)]
        private string connectionMessage = "准备测试连接";

        private double connectionStartTime;
        private ConnectionStatus connectionStatus = ConnectionStatus.Pending;
        private string debugUrl;
        private bool isTestingConnection;
        private double responseTime;

        [BoxGroup(PoofLibraryConstParam.REPO_TOKEN_GROUP)]
        [ShowInInspector]
        [DisplayAsString(EnableRichText = true)]
        [LabelText(PoofLibraryConstParam.REPO_TOKEN)]
        [LabelWidth(150)]
        private string showToken = "";

        [HorizontalGroup(PoofLibraryConstParam.REPO_SETTING_GROUP_SUB_CONNECTION)]
        [PropertyOrder(3)]
        [ShowInInspector]
        [DisplayAsString(EnableRichText = true)]
        [HideLabel]
        private string connectionInfo => $"<color=orange>状态:{connectionStatus}</color>  响应时间:{responseTime:0.00}ms";

        private void OnDisable()
        {
            Save();
        }

        private void OnDestroy()
        {
            Save();
        }

        private void OnValidate()
        {
            Save();
        }


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
            debugUrl = FormatGitHubUrl(PoofLibraryConstParam.GIT_REPO_RAW_URL, PoofLibraryConstParam.DEFAULT_MENU_PATH);

            isTestingConnection = true;
            EditorCoroutineUtility.StartCoroutineOwnerless(TestConnectionCoroutine());
        }


        // 测试连接协程
        private IEnumerator TestConnectionCoroutine()
        {
            using (var request = UnityWebRequest.Head(debugUrl))
            {
                // 设置请求参数
                request.timeout = 10;
                if (!string.IsNullOrEmpty(ReadToken()))
                    request.SetRequestHeader("Authorization", $"token {ReadToken()}");
                else
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
                        connectionMessage = $"测试地址:{debugUrl}\n" +
                                            "连接成功！\n" +
                                            "HTTP 状态: 200 OK\n" +
                                            $"响应时间: {totalTime:0} ms";
                    else
                        connectionMessage = $"测试地址:{debugUrl}\n" +
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
                401 or 403 => $"测试地址:{debugUrl}\n" +
                              $"访问被拒绝 (HTTP {request.responseCode})\n" +
                              "• 私有仓库？请添加 GitHub 令牌",

                404 => $"测试地址:{debugUrl}\n" +
                       "文件未找到/GitHub令牌错误 (HTTP 404)\n" +
                       $"• 检查文件路径: {PoofLibraryConstParam.DEFAULT_MENU_PATH}\n" +
                       "• 检查GitHub令牌是否输入正确，或者令牌已过期",

                429 => $"测试地址:{debugUrl}\n" +
                       "\u26a0GitHub 速率限制\n" +
                       "• 添加 GitHub 令牌可提高限制",

                _ when request.result == UnityWebRequest.Result.ConnectionError =>
                    $"测试地址:{debugUrl}\n" +
                    "网络连接失败\n" +
                    "请检查网络连接，DNS是否正常。",
                //$"• 尝试在浏览器中打开: {sanitizedUrl}",

                _ => $"测试地址:{debugUrl}\n" + $"连接失败: {request.error} (HTTP {request.responseCode})"
            };
        }

        // 格式化 GitHub URL
        private string FormatGitHubUrl(string repoUrl, string path)
        {
            var encodedPath = path.Replace(" ", "%20");
            encodedPath = UnityWebRequest.EscapeURL(encodedPath)
                .Replace("%3A", ":")
                .Replace("%2F", "/")
                .Replace("%5C", "/");

            var url = $"{repoUrl}/{encodedPath}";

            return url;
        }

        /// <summary>
        ///     从存储令牌的txt文件中读取令牌
        /// </summary>
        /// <returns></returns>
        public string ReadToken()
        {
            if (string.IsNullOrEmpty(TokenFilePath))
            {
                Debug.LogWarning("令牌文件路径未设置");
                EditorWindow.focusedWindow.ShowNotification(new GUIContent("令牌文件路径未设置"));
                EditorWindow.focusedWindow.Repaint();
                showToken = "--";
                return string.Empty;
            }

            try
            {
                if (File.Exists(TokenFilePath))
                {
                    EditorWindow.focusedWindow.Repaint();
                    var t = File.ReadAllText(TokenFilePath).Trim();
                    showToken = $"<color=orange>{t}</color>";
                    return t;
                }

                Debug.LogWarning($"令牌文件不存在: {TokenFilePath}");
                EditorWindow.focusedWindow.ShowNotification(new GUIContent($"令牌文件不存在: {TokenFilePath}"));
                EditorWindow.focusedWindow.Repaint();
                showToken = "--";
                return string.Empty;
            }
            catch (Exception e)
            {
                Debug.LogError($"读取令牌失败: {e.Message}");
                EditorWindow.focusedWindow.ShowNotification(new GUIContent($"读取令牌失败: {e.Message}"));
                EditorWindow.focusedWindow.Repaint();
                showToken = "--";
                return string.Empty;
            }
        }
    }
}