using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Text;

namespace PoofLibraryManager.Editor
{
    /// <summary>
    /// Git 下载工具类 - 提供文件和文件夹下载功能
    /// </summary>
    public static class PoofLibNetworkHelper
    {
        #region 公共接口

        /// <summary>
        /// 下载单个文件
        /// </summary>
        /// <param name="config">下载配置</param>
        public static void DownloadFile(GitFileDownloadConfig config)
        {
            EditorCoroutineHelper.Start(DownloadFileCoroutine(config));
        }

        /// <summary>
        /// 下载整个文件夹（包括子文件夹）
        /// </summary>
        /// <param name="config">下载配置</param>
        public static void DownloadFolder(GitFolderDownloadConfig config)
        {
            EditorCoroutineHelper.Start(DownloadFolderCoroutine(config));
        }

        #endregion

        #region 下载状态事件

        /// <summary>
        /// 下载状态变更事件
        /// </summary>
        public static event Action<PLDownloadState> OnDownloadStateChange;

        /// <summary>
        /// 文件下载进度事件
        /// </summary>
        public static event Action<PLFileProgress> OnFileProgress;

        /// <summary>
        /// 文件夹下载进度事件
        /// </summary>
        public static event Action<PLFolderProgress> OnFolderProgress;

        /// <summary>
        /// 下载完成事件
        /// </summary>
        public static event Action<PLDownloadResult> OnDownloadComplete;

        #endregion

        #region 核心实现

        // 当前下载状态
        private static PLDownloadState currentState = PLDownloadState.Ready;

        // 文件夹下载上下文
        private static FolderDownloadContext folderContext;

        // 下载单个文件
        private static IEnumerator DownloadFileCoroutine(GitFileDownloadConfig config)
        {
            // 初始化状态
            currentState = PLDownloadState.DownloadingFile;
            NotifyStateChange();

            // 创建结果对象
            var fileResult = new PLFileResult
            {
                FileName = config.FileName,
                RemotePath = config.RemoteFilePath,
                LocalPath = Path.Combine(config.LocalSavePath, config.FileName)
            };

            var startTime = EditorApplication.timeSinceStartup;
            double lastUpdateTime = startTime;

            // 构建下载URL
            string url = FormatGitHubUrl(config.Username, config.Repository,
                config.Branch, config.RemoteFilePath, config.GitToken);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // 设置请求头
                request.timeout = 45;
                request.SetRequestHeader("User-Agent", "UnityEditor/" + Application.unityVersion);

                // 开始下载
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                // 更新进度
                while (!operation.isDone)
                {
                    // 计算下载速度
                    double currentTime = EditorApplication.timeSinceStartup;
                    double elapsed = currentTime - lastUpdateTime;

                    if (elapsed > 0.1f) // 限制更新频率
                    {
                        double downloadSpeed = request.downloadedBytes / (currentTime - startTime);

                        // 通知进度
                        NotifyFileProgress(new PLFileProgress
                        {
                            FileName = config.FileName,
                            Progress = request.downloadProgress,
                            DownloadedBytes = (long)request.downloadedBytes,
                            TotalBytes = (long)(request.downloadedBytes / Math.Max(0.01f, request.downloadProgress)),
                            Speed = downloadSpeed
                        });

                        lastUpdateTime = currentTime;
                    }

                    yield return null;
                }

                // 计算下载时间
                double totalTime = EditorApplication.timeSinceStartup - startTime;
                fileResult.DownloadTime = totalTime;

                // 处理结果
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        // 确保目录存在
                        string directory = Path.GetDirectoryName(fileResult.LocalPath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // 写入文件
                        File.WriteAllBytes(fileResult.LocalPath, request.downloadHandler.data);
                        fileResult.FileSize = request.downloadHandler.data.Length;
                        fileResult.IsSuccess = true;

                        // 通知完成
                        NotifyDownloadComplete(new PLDownloadResult
                        {
                            IsSuccess = true,
                            Message = $"文件下载成功: {config.FileName}",
                            TotalFiles = 1,
                            SuccessCount = 1,
                            TotalTime = totalTime,
                            FileResults = new List<PLFileResult> { fileResult }
                        });
                    }
                    catch (Exception ex)
                    {
                        fileResult.IsSuccess = false;
                        fileResult.ErrorMessage = $"文件写入失败: {ex.Message}";

                        NotifyDownloadComplete(new PLDownloadResult
                        {
                            IsSuccess = false,
                            Message = $"文件下载失败: {ex.Message}",
                            TotalFiles = 1,
                            FailCount = 1,
                            TotalTime = totalTime,
                            FileResults = new List<PLFileResult> { fileResult }
                        });
                    }
                }
                else
                {
                    fileResult.IsSuccess = false;
                    fileResult.ErrorMessage = request.error;

                    NotifyDownloadComplete(new PLDownloadResult
                    {
                        IsSuccess = false,
                        Message = $"文件下载失败: {request.error}",
                        TotalFiles = 1,
                        FailCount = 1,
                        TotalTime = totalTime,
                        FileResults = new List<PLFileResult> { fileResult }
                    });
                }
            }

            // 恢复就绪状态
            currentState = PLDownloadState.Ready;
            NotifyStateChange();
        }

        // 下载整个文件夹
        private static IEnumerator DownloadFolderCoroutine(GitFolderDownloadConfig config)
        {
            // 初始化状态
            currentState = PLDownloadState.ScanningFolder;
            NotifyStateChange();

            // 创建文件夹上下文
            folderContext = new FolderDownloadContext(config);
            folderContext.StartTime = EditorApplication.timeSinceStartup;

            // 开始扫描文件夹
            yield return ScanFolderRecursive(folderContext);

            // 开始下载文件
            currentState = PLDownloadState.DownloadingFile;
            NotifyStateChange();

            yield return DownloadFiles(folderContext);

            // 完成处理
            currentState = folderContext.FailCount > 0 ? PLDownloadState.Failed : PLDownloadState.Completed;

            NotifyStateChange();

            // 刷新资源数据库
            AssetDatabase.Refresh();

            // 通知最终结果
            NotifyDownloadComplete(new PLDownloadResult
            {
                IsSuccess = folderContext.FailCount == 0,
                Message = folderContext.FailCount == 0 ? "文件夹下载成功" : "文件夹下载部分失败",
                TotalFiles = folderContext.TotalFiles,
                SuccessCount = folderContext.SuccessCount,
                FailCount = folderContext.FailCount,
                TotalTime = EditorApplication.timeSinceStartup - folderContext.StartTime,
                FileResults = folderContext.FileResults
            });

            // 重置状态
            currentState = PLDownloadState.Ready;
            folderContext = null;
            NotifyStateChange();
        }

        // 递归扫描文件夹
        private static IEnumerator ScanFolderRecursive(FolderDownloadContext context)
        {
            // 构建API URL
            string apiUrl =
                $"https://api.github.com/repos/{context.Config.Username}/{context.Config.Repository}/contents/{context.Config.RemoteFolderPath}?ref={context.Config.Branch}";

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                // 设置请求头
                request.timeout = 15;
                request.SetRequestHeader("User-Agent", "UnityEditor/" + Application.unityVersion);

                if (!string.IsNullOrWhiteSpace(context.Config.GitToken))
                {
                    request.SetRequestHeader("Authorization", $"token {context.Config.GitToken}");
                }

                // 发送请求
                yield return request.SendWebRequest();

                // 处理结果
                if (request.result != UnityWebRequest.Result.Success)
                {
                    context.ErrorMessage = $"扫描文件夹失败: {request.error}";
                    yield break;
                }

                // 解析JSON响应
                // try
                // {
                    string json = request.downloadHandler.text;
                    var items = ParseGitHubItems(json);

                    if (items == null || items.Length == 0)
                    {
                        context.ErrorMessage = "文件夹为空";
                        yield break;
                    }

                    // 处理扫描结果
                    foreach (var item in items)
                    {
                        if (item.type == "file")
                        {
                            // 添加到文件列表
                            var fileInfo = new PLFileResult
                            {
                                FileName = item.name,
                                RemotePath = item.path,
                                LocalPath = Path.Combine(context.Config.LocalSavePath, item.name),
                                FileSize = item.size
                            };

                            context.FileResults.Add(fileInfo);
                            context.TotalFiles++;
                        }
                        else if (item.type == "dir" && context.Config.IncludeSubfolders)
                        {
                            // 递归扫描子文件夹
                            context.Config.RemoteFolderPath = item.path;
                            yield return ScanFolderRecursive(context);
                        }
                    }
                //}
                // catch (Exception ex)
                // {
                //     context.ErrorMessage = $"解析API响应失败: {ex.Message}";
                // }
            }
        }

        // 下载文件列表
        private static IEnumerator DownloadFiles(FolderDownloadContext context)
        {
            context.StartDownloadTime = EditorApplication.timeSinceStartup;

            for (int i = 0; i < context.FileResults.Count; i++)
            {
                if (context.IsCancelled) break;

                var file = context.FileResults[i];
                context.CurrentFileIndex = i;

                // 更新文件夹进度
                NotifyFolderProgress(new PLFolderProgress
                {
                    TotalFiles = context.TotalFiles,
                    DownloadedFiles = context.SuccessCount + context.FailCount,
                    OverallProgress = (float)(context.SuccessCount + context.FailCount) / context.TotalFiles,
                    CurrentFile = file.FileName
                });

                // 构建下载URL
                string downloadUrl = FormatGitHubUrl(context.Config.Username, context.Config.Repository,
                    context.Config.Branch, file.RemotePath, context.Config.GitToken);

                file.DownloadTime = EditorApplication.timeSinceStartup;

                using (UnityWebRequest request = UnityWebRequest.Get(downloadUrl))
                {
                    request.timeout = 45;
                    request.SetRequestHeader("User-Agent", "UnityEditor/" + Application.unityVersion);

                    // 发送请求
                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                    // 更新进度
                    while (!operation.isDone && !context.IsCancelled)
                    {
                        // 更新文件进度
                        NotifyFileProgress(new PLFileProgress
                        {
                            FileName = file.FileName,
                            Progress = request.downloadProgress,
                            DownloadedBytes = (long)request.downloadedBytes,
                            TotalBytes = file.FileSize,
                            Speed = request.downloadedBytes / Math.Max(0.01,
                                EditorApplication.timeSinceStartup - file.DownloadTime)
                        });

                        yield return null;
                    }

                    // 处理结果
                    if (context.IsCancelled)
                    {
                        file.IsSuccess = false;
                        file.ErrorMessage = "下载已取消";
                        context.FailCount++;
                    }
                    else if (request.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            // 确保目录存在
                            string directory = Path.GetDirectoryName(file.LocalPath);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }

                            // 写入文件
                            File.WriteAllBytes(file.LocalPath, request.downloadHandler.data);
                            file.IsSuccess = true;
                            context.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            file.IsSuccess = false;
                            file.ErrorMessage = $"文件写入失败: {ex.Message}";
                            context.FailCount++;
                        }
                    }
                    else
                    {
                        file.IsSuccess = false;
                        file.ErrorMessage = request.error;
                        context.FailCount++;
                    }

                    // 更新下载时间
                    file.DownloadTime = EditorApplication.timeSinceStartup - file.DownloadTime;

                    // 添加延迟防止卡顿
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        #endregion

        #region 辅助方法

        // 格式化 GitHub URL
        private static string FormatGitHubUrl(string user, string repo, string branch, string path, string token = "")
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

        // 解析GitHub API响应
        private static GitHubItem[] ParseGitHubItems(string json)
        {
            try
            {
                // GitHub API返回的是数组，需要特殊处理
                StringBuilder sb = new StringBuilder();
                sb.Append("{\"items\":");
                sb.Append(json);
                sb.Append("}");

                Wrapper wrapper = JsonUtility.FromJson<Wrapper>(sb.ToString());
                return wrapper.items;
            }
            catch
            {
                // 尝试直接解析
                try
                {
                    return JsonUtility.FromJson<GitHubItem[]>(json);
                }
                catch
                {
                    return null;
                }
            }
        }

        // 通知状态变化
        private static void NotifyStateChange()
        {
            OnDownloadStateChange?.Invoke(currentState);
        }

        // 通知文件进度
        private static void NotifyFileProgress(PLFileProgress progress)
        {
            OnFileProgress?.Invoke(progress);
        }

        // 通知文件夹进度
        private static void NotifyFolderProgress(PLFolderProgress progress)
        {
            OnFolderProgress?.Invoke(progress);
        }

        // 通知下载完成
        private static void NotifyDownloadComplete(PLDownloadResult result)
        {
            OnDownloadComplete?.Invoke(result);
        }

        #endregion

        #region 内部类和数据结构

        // 文件夹下载上下文
        private class FolderDownloadContext
        {
            public GitFolderDownloadConfig Config;
            public double StartTime;
            public double StartDownloadTime;
            public int TotalFiles;
            public int SuccessCount;
            public int FailCount;
            public int CurrentFileIndex;
            public bool IsCancelled;
            public string ErrorMessage;
            public List<PLFileResult> FileResults = new List<PLFileResult>();

            public FolderDownloadContext(GitFolderDownloadConfig config)
            {
                Config = config;
            }
        }

        // GitHub API返回的数据结构
        [Serializable]
        private class GitHubItem
        {
            public string name;
            public string path;
            public string type; // "file" or "dir"
            public int size;
            public string download_url;
        }

        // 包装类用于解析JSON数组
        [Serializable]
        private class Wrapper
        {
            public GitHubItem[] items;
        }

        #endregion
    }
    
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
}