using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

namespace ExOpenSource.Editor
{
    /// <summary>
    /// Git插件管理工具类
    /// </summary>
    public static class GitPluginUtility
    {
        #region 公共接口

        /// <summary>
        /// 从Git仓库下载插件
        /// </summary>
        public static void DownloadPlugin(
            string userName,
            string repoName,
            string branch,
            string remotePath,
            string installPath,
            string token = "")
        {
            GitPluginConfig config = new GitPluginConfig()
            {
                repoUrl =  $"https://github.com/{userName}/{repoName}.git",
                targetBranch = branch,
                relativePath = remotePath,
                installPath = installPath,
                token = token
            };
            ExecuteOperation(() =>
            {
                // 确保目标目录存在
                Directory.CreateDirectory(config.installPath);

                // 执行下载
                DownloadGitRepository(
                    config.repoUrl,
                    config.targetBranch,
                    config.relativePath,
                    config.installPath,
                    config.token
                );

                // 保存安装记录
                AddInstallationRecord(config);
            }, "Downloading Plugin");
        }

        /// <summary>
        /// 更新已安装的插件
        /// </summary>
        /// <param name="config">插件配置</param>
        /// <param name="forceReinstall">是否强制重新安装（即使版本相同）</param>
        public static void UpdatePlugin(
            string userName,
            string repoName,
            string branch,
            string remotePath,
            string installPath,
            string token = "",
            bool forceReinstall = false)
        {
            GitPluginConfig config = new GitPluginConfig()
            {
                repoUrl =  $"https://github.com/{userName}/{repoName}.git",
                targetBranch = branch,
                relativePath = remotePath,
                installPath = installPath,
                token = token
            };
            
            if (!Directory.Exists(config.installPath))
            {
                EditorUtility.DisplayDialog("Update Failed",
                    $"Plugin not found at: {config.installPath}", "OK");
                return;
            }

            // 检查是否需要更新
            if (!forceReinstall && !IsUpdateAvailable(config))
            {
                EditorUtility.DisplayDialog("Plugin Up to Date",
                    "The plugin is already at the latest version.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Confirm Update",
                    $"Update plugin from {config.repoUrl}?\nThis will delete existing files at:\n{config.installPath}",
                    "Update", "Cancel"))
                return;

            ExecuteOperation(() =>
            {
                // 先卸载再重新安装
                UninstallPlugin(config.installPath);
                DownloadPlugin(userName,
                    repoName,
                    branch,
                    remotePath,
                    installPath,
                    token);
            }, "Updating Plugin");
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        /// <param name="installPath">插件安装路径</param>
        public static void UninstallPlugin(string installPath)
        {
            if (!Directory.Exists(installPath))
            {
                EditorUtility.DisplayDialog("Uninstall Failed",
                    $"Directory not found: {installPath}", "OK");
                return;
            }

            ExecuteOperation(() =>
            {
                // 删除安装目录
                Directory.Delete(installPath, true);

                // 删除对应的meta文件
                string metaFile = $"{installPath}.meta";
                if (File.Exists(metaFile))
                    File.Delete(metaFile);

                // 从安装记录中移除
                RemoveInstallationRecord(installPath);

                AssetDatabase.Refresh();
            }, "Uninstalling Plugin");
        }

        #endregion

        #region 核心实现

        private static void DownloadGitRepository(
            string repoUrl,
            string targetBranch,
            string relativePath,
            string installPath,
            string token = "")
        {
            if (string.IsNullOrEmpty(repoUrl))
                throw new ArgumentException("Repository URL cannot be empty");

            if (string.IsNullOrEmpty(targetBranch))
                throw new ArgumentException("Target branch cannot be empty");

            // 添加Token认证
            string authenticatedUrl = ApplyTokenToUrl(repoUrl, token);

            // 创建临时目录
            string tempPath = Path.Combine(Application.temporaryCachePath, "GitTemp_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempPath);

            try
            {
                EditorUtility.DisplayProgressBar("Git Plugin Manager", "Initializing repository...", 0.1f);

                bool isRootPath = string.IsNullOrEmpty(relativePath) || relativePath == "/" || relativePath == "\\";

                if (isRootPath)
                {
                    // 根目录处理：直接克隆整个仓库
                    RunGitCommand($"clone --depth 1 --filter=blob:none \"{authenticatedUrl}\" \"{tempPath}\"");
    
                    // 切换到目标版本
                    if (!string.IsNullOrEmpty(targetBranch))
                        RunGitCommand($"checkout {targetBranch}", tempPath);
                }
                else
                {

                    // 克隆仓库（不检出）
                    RunGitCommand(
                        $"clone --depth 1 --filter=blob:none --no-checkout \"{authenticatedUrl}\" \"{tempPath}\"");

                    // 设置稀疏检出
                    RunGitCommand("sparse-checkout init --cone", tempPath);
                    RunGitCommand($"sparse-checkout set \"{relativePath}\"", tempPath);

                    // 检出指定版本或分支
                    EditorUtility.DisplayProgressBar("Git Plugin Manager", "Checking out files...", 0.5f);
                    string checkoutTarget = targetBranch;
                    RunGitCommand($"checkout {checkoutTarget}", tempPath);
                }

                // 移动文件到目标位置
                EditorUtility.DisplayProgressBar("Git Plugin Manager", "Moving files...", 0.8f);
                string sourceDir = Path.Combine(tempPath, SanitizePath(relativePath));
                if (Directory.Exists(sourceDir))
                {
                    // 确保目标目录存在
                    Directory.CreateDirectory(installPath);

                    // 移动并覆盖文件
                    FileUtil.ReplaceDirectory(sourceDir, installPath);
                    AssetDatabase.Refresh();
                }
                else
                {
                    throw new DirectoryNotFoundException($"Directory not found in repository: {relativePath}");
                }
            }
            finally
            {
                // 清理临时目录
                EditorUtility.DisplayProgressBar("Git Plugin Manager", "Cleaning up...", 0.9f);
                Directory.Delete(tempPath, true);
                EditorUtility.ClearProgressBar();
            }
        }

        private static string ApplyTokenToUrl(string originalUrl, string token)
        {
            if (string.IsNullOrEmpty(token))
                return originalUrl;

            // 处理不同格式的URL
            if (originalUrl.StartsWith("https://"))
            {
                // 插入token: https://token@github.com/user/repo.git
                int startIndex = "https://".Length;
                return originalUrl.Insert(startIndex, $"{token}@");
            }

            if (originalUrl.StartsWith("http://"))
            {
                // 插入token: http://token@github.com/user/repo.git
                int startIndex = "http://".Length;
                return originalUrl.Insert(startIndex, $"{token}@");
            }

            throw new ArgumentException("Token authentication only supported for HTTP/HTTPS URLs");
        }

        private static void RunGitCommand(string command, string workingDir = null)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = command,
                    WorkingDirectory = workingDir ?? Application.dataPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Debug.Log($"Executing: git {command}");

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string fullError = $"Command 'git {command}' failed with error ({process.ExitCode}):\n{error}";
                    throw new InvalidOperationException(fullError);
                }
            }
        }

        #endregion

        #region 辅助功能

        private static string SanitizePath(string path)
        {
            return path.Trim().TrimEnd('/', '\\');
        }

        private static void ExecuteOperation(Action operation, string title)
        {
            try
            {
                operation.Invoke();
                Debug.Log($"{title} completed successfully!");
                //EditorUtility.DisplayDialog("Success", $"{title} completed successfully!", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{title} Error: {ex}");
                //EditorUtility.DisplayDialog("Operation Failed", $"{title} failed: {ex.Message}", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #endregion

        #region 安装记录管理

        private const string INSTALL_RECORD_PATH = "Assets/GitPluginInstallRecords.asset";

        [Serializable]
        private class InstallationRecord
        {
            public List<GitPluginConfig> installedPlugins = new List<GitPluginConfig>();
        }

        private static void AddInstallationRecord(GitPluginConfig config)
        {
            var records = LoadInstallationRecords();

            // 移除旧记录（如果有）
            records.installedPlugins.RemoveAll(p => p.installPath == config.installPath);

            // 添加新记录
            records.installedPlugins.Add(config);
            SaveInstallationRecords(records);
        }

        private static void RemoveInstallationRecord(string installPath)
        {
            var records = LoadInstallationRecords();
            int removed = records.installedPlugins.RemoveAll(p => p.installPath == installPath);

            if (removed > 0)
                SaveInstallationRecords(records);
        }

        private static InstallationRecord LoadInstallationRecords()
        {
            if (File.Exists(INSTALL_RECORD_PATH))
            {
                string json = File.ReadAllText(INSTALL_RECORD_PATH);
                return JsonUtility.FromJson<InstallationRecord>(json);
            }

            return new InstallationRecord();
        }

        private static void SaveInstallationRecords(InstallationRecord records)
        {
            string json = JsonUtility.ToJson(records, true);
            File.WriteAllText(INSTALL_RECORD_PATH, json);
            AssetDatabase.ImportAsset(INSTALL_RECORD_PATH);
        }

        private static bool IsUpdateAvailable(GitPluginConfig config)
        {
            // 在实际应用中，这里应该实现版本检查逻辑
            // 例如：比较本地记录的commit hash和远程最新版本
            // 此处简化实现总是返回true，表示需要更新
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Git插件配置
    /// </summary>
    [Serializable]
    public class GitPluginConfig
    {
        public string repoUrl;
        public string targetBranch;
        public string relativePath;
        public string installPath;
        public string token;

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(repoUrl) &&
                   !string.IsNullOrEmpty(relativePath) &&
                   !string.IsNullOrEmpty(installPath);
        }
    }
}