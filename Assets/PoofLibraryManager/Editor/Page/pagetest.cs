using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System;
using PoofLibraryManager.Editor;
using Sirenix.OdinInspector; // Odin Inspector 核心命名空间
using Sirenix.OdinInspector.Editor; // Odin 编辑器功能

public class GitResourceDownloader : OdinEditorWindow
{
    // 状态枚举
    public enum DownloadStatus
    {
        [LabelText("🟢 准备就绪")]
        Ready,
        
        [LabelText("🔄 下载中")]
        Downloading,
        
        [LabelText("✅ 下载成功")]
        Success,
        
        [LabelText("❌ 下载失败")]
        Failed
    }
    

    // 标签页选择
    [PropertyOrder(-10)]
    [TabGroup("功能区")]
    [ShowInInspector]
    [EnumToggleButtons]
    [HideLabel]
    private int selectedTab = 0;
    
    private const int DOWNLOAD_TAB = 0;
    private const int CONNECTION_TAB = 1;

    // GitHub 配置
    [BoxGroup("下载设置/GitHub 仓库配置", centerLabel: true)]
    [HorizontalGroup("下载设置/GitHub 仓库配置/Split", 0.5f)]
    [LabelText("用户名/组织")]
    [LabelWidth(100)]
    public string username = "Unity-Technologies";
    
    [HorizontalGroup("下载设置/GitHub 仓库配置/Split")]
    [LabelText("仓库名")]
    [LabelWidth(60)]
    public string repo = "2d-extras";
    
    [BoxGroup("下载设置/GitHub 仓库配置")]
    [HorizontalGroup("下载设置/GitHub 仓库配置/BranchSplit")]
    [LabelText("分支")]
    [LabelWidth(40)]
    public string branch = "master";
    
    [HorizontalGroup("下载设置/GitHub 仓库配置/BranchSplit")]
    [LabelText("文件路径")]
    [LabelWidth(60)]
    [Sirenix.OdinInspector.FilePath]
    public string filePath = "Assets/Tilemap/Tiles/Animated Tile/Scripts/PlayerController.cs";
    
    [BoxGroup("下载设置/GitHub 仓库配置")]
    [InfoBox("访问令牌(可选):\n• 私有仓库必须提供\n• 避免GitHub速率限制\n• 创建地址: https://github.com/settings/tokens", 
             InfoMessageType.Info)]
    [LabelText("GitHub 令牌")]
    [LabelWidth(80)]
    [HideLabel]
    public string gitToken = "";
    
    // 本地配置
    [BoxGroup("下载设置/本地保存设置", centerLabel: true)]
    [FolderPath(RequireExistingPath = true)]
    [LabelText("保存路径")]
    [LabelWidth(60)]
    public string savePath = "Assets/Scripts/";
    
    [BoxGroup("下载设置/本地保存设置")]
    [LabelText("文件名")]
    [LabelWidth(50)]
    public string fileName = "PlayerController.cs";
    
    // 下载状态
    [BoxGroup("下载状态", centerLabel: true)]
    [ShowInInspector]
    [ReadOnly]
    [ProgressBar(0, 1, Height = 20, ColorMember = "GetProgressBarColor", DrawValueLabel = false)]
    [HideLabel]
    [ShowIf("@selectedTab == DOWNLOAD_TAB")]
    private float downloadProgress;
    
    [BoxGroup("下载状态")]
    [ShowInInspector]
    [ReadOnly]
    [EnumPaging]
    [HideLabel]
    [ShowIf("@selectedTab == DOWNLOAD_TAB")]
    private DownloadStatus downloadStatus = DownloadStatus.Ready;
    
    [BoxGroup("下载状态")]
    [ShowInInspector]
    [ReadOnly]
    [MultiLineProperty(4)]
    [HideLabel]
    [ShowIf("@selectedTab == DOWNLOAD_TAB")]
    private string downloadMessage = "准备下载资源";
    
    // 连接测试状态
    [BoxGroup("连接状态", centerLabel: true)]
    [ShowInInspector]
    [ReadOnly]
    [EnumPaging]
    [HideLabel]
    //[ShowIf("@selectedTab == CONNECTION_TAB")]
    private ConnectionStatus connectionStatus = ConnectionStatus.Pending;
    
    [BoxGroup("连接状态")]
    [ShowInInspector]
    [ReadOnly]
    [MultiLineProperty(4)]
    [HideLabel]
    [ShowIf("@selectedTab == CONNECTION_TAB")]
    private string connectionMessage = "准备测试连接";
    
    [BoxGroup("连接状态")]
    [ShowInInspector]
    [ReadOnly]
    [LabelText("响应时间")]
    [LabelWidth(60)]
    [ShowIf("@selectedTab == CONNECTION_TAB && connectionStatus != ConnectionStatus.Pending")]
    private string responseTime = "0 ms";
    
    // 调试信息
    [BoxGroup("调试信息", centerLabel: true)]
    [ShowInInspector]
    [ReadOnly]
    [MultiLineProperty(2)]
    [HideLabel]
    private string debugUrl;
    
    // 私有变量
    private bool isDownloading;
    private bool isTestingConnection;
    private double connectionStartTime;
    private double downloadStartTime;
    
    // 打开窗口
    [MenuItem("Tools/Git 资源下载工具")]
    public static void ShowWindow()
    {
        var window = GetWindow<GitResourceDownloader>();
        window.titleContent = new GUIContent("Git 资源下载工具");
        window.minSize = new Vector2(500, 450);
        window.Show();
    }
    
    // 进度条颜色
    private Color GetProgressBarColor()
    {
        return downloadStatus switch
        {
            DownloadStatus.Ready => Color.gray,
            DownloadStatus.Downloading => new Color(0.2f, 0.6f, 1f),
            DownloadStatus.Success => new Color(0.2f, 0.8f, 0.2f),
            DownloadStatus.Failed => new Color(1f, 0.3f, 0.3f),
            _ => Color.white
        };
    }

    // 下载按钮
    [BoxGroup("操作")]
    [Button(ButtonSizes.Large, Name = "下载资源")]
    [GUIColor(0.4f, 0.8f, 1f)]
    [PropertyOrder(10)]
    [EnableIf("@!isDownloading && selectedTab == DOWNLOAD_TAB")]
    private void StartDownload()
    {
        if (isDownloading) return;
        
        downloadStatus = DownloadStatus.Downloading;
        downloadMessage = "正在连接到 GitHub 仓库...";
        downloadProgress = 0f;
        downloadStartTime = EditorApplication.timeSinceStartup;
        
        // 生成 URL
        debugUrl = FormatGitHubUrl(username, repo, branch, filePath, gitToken);
        
        // 验证保存路径
        if (string.IsNullOrWhiteSpace(savePath))
        {
            downloadMessage = "错误：保存路径不能为空！";
            downloadStatus = DownloadStatus.Failed;
            return;
        }
        
        // 确保目录存在
        try
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }
        catch (Exception ex)
        {
            downloadMessage = $"创建目录失败：{ex.Message}";
            downloadStatus = DownloadStatus.Failed;
            return;
        }
        
        // 验证文件名
        if (string.IsNullOrWhiteSpace(fileName))
        {
            downloadMessage = "错误：文件名不能为空！";
            downloadStatus = DownloadStatus.Failed;
            return;
        }
        
        string fullPath = Path.Combine(savePath, fileName);
        
        // 开始下载
        isDownloading = true;
        EditorCoroutineHelper.Start(DownloadFile(debugUrl, fullPath));
    }
    
    // 测试连接按钮
    [BoxGroup("操作")]
    [Button(ButtonSizes.Large, Name = "测试连接")]
    [GUIColor(0.5f, 1f, 0.7f)]
    [PropertyOrder(10)]
    private void TestConnection()
    {
        if (isTestingConnection) return;
        
        connectionStatus = ConnectionStatus.Checking;
        connectionMessage = "正在测试连接...";
        connectionStartTime = EditorApplication.timeSinceStartup;
        
        // 生成 URL
        debugUrl = FormatGitHubUrl(username, repo, branch, filePath, gitToken);
        
        isTestingConnection = true;
        EditorCoroutineHelper.Start(TestConnectionCoroutine());
    }
    
    // 下载文件协程
    private IEnumerator DownloadFile(string url, string path)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // 设置请求参数
            request.timeout = 45;
            request.SetRequestHeader("User-Agent", "UnityEditor/" + Application.unityVersion);
            
            // 开始请求
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            
            // 更新进度
            while (!operation.isDone)
            {
                downloadProgress = request.downloadProgress;
                
                // 显示下载速度
                double elapsed = EditorApplication.timeSinceStartup - downloadStartTime;
                double downloadSpeed = request.downloadedBytes / (elapsed > 0 ? elapsed : 1);
                downloadMessage = $"下载中: {FormatBytes((long)request.downloadedBytes)} " +
                                 $"({FormatBytes((long)downloadSpeed)}/s)";
                
                yield return null;
            }
            
            // 计算响应时间
            double totalTime = (EditorApplication.timeSinceStartup - downloadStartTime) * 1000;
            
            // 处理结果
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // 确保目录存在
                    string directory = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    // 写入文件
                    File.WriteAllBytes(path, request.downloadHandler.data);
                    AssetDatabase.Refresh();
                    
                    downloadStatus = DownloadStatus.Success;
                    downloadMessage = $"✅ 下载成功！\n" +
                                    $"保存到: {path}\n" +
                                    $"文件大小: {FormatBytes(request.downloadHandler.data.Length)}\n" +
                                    $"耗时: {totalTime:0} ms";
                }
                catch (Exception ex)
                {
                    downloadStatus = DownloadStatus.Failed;
                    downloadMessage = $"❌ 文件写入错误: {ex.Message}";
                }
            }
            else
            {
                downloadStatus = DownloadStatus.Failed;
                downloadMessage = HandleDownloadError(request, url, totalTime);
            }
            
            downloadProgress = 1f;
            isDownloading = false;
        }
    }
    
    // 测试连接协程
    private IEnumerator TestConnectionCoroutine()
    {
        using (UnityWebRequest request = UnityWebRequest.Head(debugUrl))
        {
            // 设置请求参数
            request.timeout = 10;
            request.SetRequestHeader("User-Agent", "UnityEditor/" + Application.unityVersion);
            
            // 开始请求
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            
            // 更新状态
            while (!operation.isDone)
            {
                connectionMessage = $"正在连接... ({Mathf.FloorToInt(request.downloadProgress * 100)}%)";
                yield return null;
            }
            
            // 计算响应时间
            double totalTime = (EditorApplication.timeSinceStartup - connectionStartTime) * 1000;
            responseTime = $"{totalTime:0.00} ms";
            
            // 处理结果
            if (request.result == UnityWebRequest.Result.Success)
            {
                connectionStatus = ConnectionStatus.Success;
                
                if (request.responseCode == 200)
                {
                    connectionMessage = $"✅ 连接成功！\n" +
                                      $"HTTP 状态: 200 OK\n" +
                                      $"响应时间: {totalTime:0} ms";
                }
                else
                {
                    connectionMessage = $"⚠️ 连接成功但文件可能不存在\n" +
                                      $"HTTP 状态: {request.responseCode}\n" +
                                      $"响应时间: {totalTime:0} ms";
                }
            }
            else
            {
                connectionStatus = ConnectionStatus.Failed;
                connectionMessage = HandleConnectionError(request, debugUrl, totalTime);
            }
            
            isTestingConnection = false;
        }
    }
    
    // 处理下载错误
    private string HandleDownloadError(UnityWebRequest request, string url, double responseTime)
    {
        string sanitizedUrl = SanitizeUrl(url);
        
        return request.responseCode switch
        {
            401 or 403 => $"🚫 访问被拒绝 (HTTP {request.responseCode})\n" +
                         $"• 私有仓库？请添加 GitHub 令牌\n" +
                         $"• 令牌可能无效或已过期\n" +
                         $"• URL: {sanitizedUrl}\n" +
                         $"• 响应时间: {responseTime:0} ms",
            
            404 => $"🔍 文件未找到 (HTTP 404)\n" +
                  $"• 检查文件路径: {filePath}\n" +
                  $"• 验证分支名称: {branch}\n" +
                  $"• URL: {sanitizedUrl}\n" +
                  $"• 响应时间: {responseTime:0} ms",
            
            429 => $"⚠️ GitHub 速率限制\n" +
                  $"• 添加 GitHub 令牌可提高限制\n" +
                  $"• 等待 60 分钟后重试\n" +
                  $"• URL: {sanitizedUrl}\n" +
                  $"• 响应时间: {responseTime:0} ms",
            
            _ when request.result == UnityWebRequest.Result.ConnectionError => 
                  $"🌐 网络连接失败\n" +
                  $"• 检查网络连接\n" +
                  $"• 验证防火墙/代理设置\n" +
                  $"• 尝试在浏览器中打开: {sanitizedUrl}\n" +
                  $"• 响应时间: {responseTime:0} ms",
            
            _ => $"❌ 下载失败: {request.error} (HTTP {request.responseCode})\n" +
                $"URL: {sanitizedUrl}\n" +
                $"响应时间: {responseTime:0} ms"
        };
    }
    
    // 处理连接错误
    private string HandleConnectionError(UnityWebRequest request, string url, double responseTime)
    {
        string sanitizedUrl = SanitizeUrl(url);
        
        return request.responseCode switch
        {
            401 or 403 => $"🚫 访问被拒绝 (HTTP {request.responseCode})\n" +
                         $"• 私有仓库？请添加 GitHub 令牌",
            
            404 => $"🔍 文件未找到 (HTTP 404)\n" +
                  $"• 检查文件路径: {filePath}",
            
            429 => $"⚠️ GitHub 速率限制\n" +
                  $"• 添加 GitHub 令牌可提高限制",
            
            _ when request.result == UnityWebRequest.Result.ConnectionError => 
                  $"🌐 网络连接失败\n" +
                  $"• 尝试在浏览器中打开: {sanitizedUrl}",
            
            _ => $"❌ 连接失败: {request.error} (HTTP {request.responseCode})"
        };
    }
    
    // 格式化 GitHub URL
    private string FormatGitHubUrl(string user, string repo, string branch, string path, string token = "")
    {
        // 替换空格为%20
        string encodedPath = path.Replace(" ", "%20");
        
        // 处理其他特殊字符
        encodedPath = UnityWebRequest.EscapeURL(encodedPath)
            .Replace("%3A", ":")
            .Replace("%2F", "/")
            .Replace("%5C", "/");
        
        // 构建基础URL
        string url = $"https://raw.githubusercontent.com/{user}/{repo}/{branch}/{encodedPath}";
        
        // 添加令牌参数（如果提供）
        if (!string.IsNullOrWhiteSpace(token))
        {
            url += $"?token={token}";
        }
        
        return url;
    }
    
    // 清理URL（隐藏令牌）
    private string SanitizeUrl(string url)
    {
        return url.Contains("?token=") ? 
            url.Substring(0, url.IndexOf("?token=")) + "?token=[REDACTED]" : 
            url;
    }
    
    // 格式化字节大小
    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double len = bytes;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
}

// 编辑器协程辅助类
public static class EditorCoroutineHelper
{
    public static void Start(IEnumerator routine)
    {
        EditorApplication.CallbackFunction update = null;
        update = () =>
        {
            try
            {
                if (!routine.MoveNext())
                {
                    EditorApplication.update -= update;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.update -= update;
            }
        };
        EditorApplication.update += update;
    }
}