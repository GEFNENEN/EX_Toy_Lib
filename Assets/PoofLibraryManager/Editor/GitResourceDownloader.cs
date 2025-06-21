using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class GitHubDownloader : EditorWindow
{
    private string username = "No78Vino";
    private string repo = "EX_Toy_Lib";
    private string branch = "main";
    private string filePath = "Assets/GameAssets/Skybox/SunView_Gradient.png";
    private string savePath = "Assets/";
    private string fileName = "SunView_Gradient.cs";
    
    private string message;
    private bool isDownloading;
    private float progress;
    private string debugUrl;
    private bool useAlternativeMethod;

    [MenuItem("Tools/GitHub Downloader")]
    public static void ShowWindow()
    {
        GetWindow<GitHubDownloader>("GitHub Downloader");
    }

    void OnEnable()
    {
        // 强制启用现代TLS协议
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        
        // 忽略证书验证错误（仅用于测试）
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
    }

    void OnGUI()
    {
        GUILayout.Label("GitHub Settings", EditorStyles.boldLabel);
        username = EditorGUILayout.TextField("Username", username);
        repo = EditorGUILayout.TextField("Repository", repo);
        branch = EditorGUILayout.TextField("Branch", branch);
        filePath = EditorGUILayout.TextField("File Path", filePath);
        
        GUILayout.Space(10);
        GUILayout.Label("Local Settings", EditorStyles.boldLabel);
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        fileName = EditorGUILayout.TextField("File Name", fileName);
        
        GUILayout.Space(10);
        useAlternativeMethod = EditorGUILayout.Toggle("Use Alternative Method", useAlternativeMethod);
        EditorGUILayout.HelpBox("If UnityWebRequest fails, try this option", MessageType.Info);
        
        GUILayout.Space(20);
        
        EditorGUI.BeginDisabledGroup(isDownloading);
        if (GUILayout.Button("Download from GitHub"))
        {
            StartDownload();
        }
        EditorGUI.EndDisabledGroup();

        if (isDownloading)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(rect, progress, $"Downloading: {progress:P0}");
        }

        if (!string.IsNullOrEmpty(message))
        {
            MessageType msgType = message.Contains("failed") ? MessageType.Error : 
                                 progress < 1 ? MessageType.Warning : MessageType.Info;
            EditorGUILayout.HelpBox(message, msgType);
        }
        
        if (!string.IsNullOrEmpty(debugUrl))
        {
            EditorGUILayout.LabelField("Debug URL:", debugUrl);
            if (GUILayout.Button("Copy URL to Clipboard"))
            {
                GUIUtility.systemCopyBuffer = debugUrl;
            }
            if (GUILayout.Button("Open in Browser"))
            {
                Application.OpenURL(debugUrl);
            }
        }
    }

    private void StartDownload()
    {
        debugUrl = FormatGitHubUrl(username, repo, branch, filePath);
        
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        string fullPath = Path.Combine(savePath, fileName);
        
        if (useAlternativeMethod)
        {
        }
        else
        {
            EditorCoroutine.Start(DownloadWithUnityWebRequest(debugUrl, fullPath));
        }
    }

    private string FormatGitHubUrl(string user, string repo, string branch, string path)
    {
        string encodedPath = path
            .Replace(" ", "%20")
            .Replace("#", "%23");
        
        return $"https://raw.githubusercontent.com/{user}/{repo}/{branch}/{encodedPath}";
    }

    // 方法1: 使用UnityWebRequest（首选）
    private IEnumerator DownloadWithUnityWebRequest(string url, string path)
    {
        isDownloading = true;
        progress = 0;
        message = "Connecting via UnityWebRequest...";
        
        using (UnityWebRequest request = new UnityWebRequest(url))
        {
            request.method = UnityWebRequest.kHttpVerbGET;
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 30;
            
            // 强制设置现代TLS协议
            #if UNITY_2017_1_OR_NEWER
            request.certificateHandler = new BypassCertificateHandler();
            #endif
            
            // 添加浏览器级别的User-Agent
            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                progress = request.downloadProgress;
                message = $"Downloading: {request.downloadedBytes / 1024}KB";
                Repaint();
                yield return null;
            }
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                message = $"UnityWebRequest failed: {request.error}\nError details: {request.downloadHandler?.text}";
                Debug.LogError(message);
            }
            else
            {
                SaveFile(path, request.downloadHandler.data);
            }
        }
        
        isDownloading = false;
    }
    
    private void SaveFile(string path, byte[] data)
    {
        try
        {
            File.WriteAllBytes(path, data);
            AssetDatabase.Refresh();
            message = $"Success! Saved to:\n{path}";
            Debug.Log(message);
        }
        catch (Exception ex)
        {
            message = $"File write error: {ex.Message}";
            Debug.LogException(ex);
        }
    }

    // 证书验证回调（忽略错误）
    private static bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        // 接受所有证书（仅用于测试环境）
        return true;
    }
}

// 证书处理类（绕过验证）
public class BypassCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // 始终返回true以接受所有证书
        return true;
    }
}

// 编辑器协程辅助类
public static class EditorCoroutine
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