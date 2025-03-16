using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using VisualNovelEngine.Model.Logging;
using VisualNovelEngine.Model.Project;

namespace VisualNovelEngine.Model.Page
{
    /// <summary>
    /// 自动保存管理器
    /// 负责定时保存当前编辑的页面，管理自动保存版本
    /// </summary>
    public static class AutoSaveManager
    {
        #region 字段
        private static DispatcherTimer _autoSaveTimer;
        private static PageJsonInfo _currentPage;
        private static string _currentPagePath;
        private static bool _isDirty;
        private static bool _isEnabled = true;
        private static TimeSpan _autoSaveInterval = TimeSpan.FromMinutes(5);
        private static int _maxAutoSaveVersions = 5;
        private static string _autoSaveDirectory;
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取或设置自动保存是否启用
        /// </summary>
        public static bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (_autoSaveTimer != null)
                    {
                        if (value)
                        {
                            _autoSaveTimer.Start();
                            Logger.Log($"自动保存已启用，间隔: {_autoSaveInterval.TotalMinutes}分钟");
                        }
                        else
                        {
                            _autoSaveTimer.Stop();
                            Logger.Log("自动保存已禁用");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取或设置自动保存间隔（分钟）
        /// </summary>
        public static int AutoSaveIntervalMinutes
        {
            get => (int)_autoSaveInterval.TotalMinutes;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("自动保存间隔必须大于等于1分钟");
                }
                _autoSaveInterval = TimeSpan.FromMinutes(value);
                if (_autoSaveTimer != null)
                {
                    _autoSaveTimer.Interval = _autoSaveInterval;
                    Logger.Log($"自动保存间隔已更新为: {value}分钟");
                }
            }
        }

        /// <summary>
        /// 获取或设置最大自动保存版本数
        /// </summary>
        public static int MaxAutoSaveVersions
        {
            get => _maxAutoSaveVersions;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("最大自动保存版本数必须大于等于1");
                }
                _maxAutoSaveVersions = value;
                Logger.Log($"最大自动保存版本数已更新为: {value}");
            }
        }
        #endregion

        #region 初始化方法
        /// <summary>
        /// 初始化自动保存管理器
        /// </summary>
        public static void Initialize()
        {
            try
            {
                _autoSaveTimer = new DispatcherTimer
                {
                    Interval = _autoSaveInterval
                };
                _autoSaveTimer.Tick += (s, e) => PerformAutoSave();

                if (_isEnabled)
                {
                    _autoSaveTimer.Start();
                    Logger.Log($"自动保存已启用，间隔: {_autoSaveInterval.TotalMinutes}分钟");
                }

                EnsureAutoSaveDirectoryExists();
            }
            catch (Exception ex)
            {
                Logger.LogError($"初始化自动保存管理器时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 确保自动保存目录存在
        /// </summary>
        private static void EnsureAutoSaveDirectoryExists()
        {
            try
            {
                if (string.IsNullOrEmpty(ProjectData.ProjectPath))
                {
                    throw new InvalidOperationException("项目路径未设置");
                }

                _autoSaveDirectory = Path.Combine(ProjectData.ProjectPath, "AutoSave");
                if (!Directory.Exists(_autoSaveDirectory))
                {
                    Directory.CreateDirectory(_autoSaveDirectory);
                    Logger.Log($"已创建自动保存目录: {_autoSaveDirectory}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"创建自动保存目录时出错: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 页面管理方法
        /// <summary>
        /// 设置当前编辑的页面
        /// </summary>
        /// <param name="page">页面信息</param>
        /// <param name="pagePath">页面路径</param>
        public static void SetCurrentPage(PageJsonInfo page, string pagePath)
        {
            try
            {
                if (page == null)
                {
                    throw new ArgumentNullException(nameof(page));
                }

                if (string.IsNullOrEmpty(pagePath))
                {
                    throw new ArgumentNullException(nameof(pagePath));
                }

                _currentPage = page;
                _currentPagePath = pagePath;
                _isDirty = false;

                Logger.Log($"已设置当前页面: {Path.GetFileName(pagePath)}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"设置当前页面时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 标记当前页面已修改
        /// </summary>
        public static void MarkDirty()
        {
            _isDirty = true;
        }
        #endregion

        #region 自动保存方法
        /// <summary>
        /// 执行自动保存
        /// </summary>
        public static void PerformAutoSave()
        {
            if (!ValidateAutoSaveConditions())
            {
                return;
            }

            try
            {
                EnsureAutoSaveDirectoryExists();

                var autoSavePath = CreateAutoSavePath();
                SavePageAndResources(autoSavePath);
                CleanupOldAutoSaves(Path.GetFileNameWithoutExtension(_currentPagePath));

                _isDirty = false;
                Logger.Log($"自动保存完成: {Path.GetFileName(autoSavePath)}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"自动保存失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 验证自动保存条件
        /// </summary>
        private static bool ValidateAutoSaveConditions()
        {
            return _isEnabled && _isDirty && _currentPage != null && !string.IsNullOrEmpty(_currentPagePath);
        }

        /// <summary>
        /// 创建自动保存文件路径
        /// </summary>
        private static string CreateAutoSavePath()
        {
            string fileName = Path.GetFileNameWithoutExtension(_currentPagePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(_autoSaveDirectory, $"{fileName}_{timestamp}.json");
        }

        /// <summary>
        /// 保存页面和相关资源
        /// </summary>
        private static void SavePageAndResources(string autoSavePath)
        {
            // 保存页面JSON
            _currentPage.SaveToFile(autoSavePath);

            // 保存关联的文本文件
            if (!string.IsNullOrEmpty(_currentPage.TextPath))
            {
                SaveAssociatedTextFile(autoSavePath);
            }
        }

        /// <summary>
        /// 保存关联的文本文件
        /// </summary>
        private static void SaveAssociatedTextFile(string autoSavePath)
        {
            string originalTextPath = _currentPage.GetAbsoluteTextPath();
            if (File.Exists(originalTextPath))
            {
                string autoSaveTextPath = Path.Combine(
                    _autoSaveDirectory,
                    $"{Path.GetFileNameWithoutExtension(_currentPage.TextPath)}_{Path.GetFileNameWithoutExtension(autoSavePath).Split('_').Last()}.rtf"
                );
                File.Copy(originalTextPath, autoSaveTextPath, true);
            }
        }

        /// <summary>
        /// 清理旧的自动保存版本
        /// </summary>
        private static void CleanupOldAutoSaves(string baseFileName)
        {
            try
            {
                var autoSaveFiles = Directory.GetFiles(_autoSaveDirectory, $"{baseFileName}_*.json")
                    .OrderByDescending(f => f)
                    .ToArray();

                if (autoSaveFiles.Length > _maxAutoSaveVersions)
                {
                    for (int i = _maxAutoSaveVersions; i < autoSaveFiles.Length; i++)
                    {
                        DeleteAutoSaveVersion(autoSaveFiles[i]);
                    }

                    Logger.Log($"已清理旧的自动保存版本: {autoSaveFiles.Length - _maxAutoSaveVersions}个");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"清理旧的自动保存版本失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除自动保存版本及其关联文件
        /// </summary>
        private static void DeleteAutoSaveVersion(string jsonFile)
        {
            try
            {
                File.Delete(jsonFile);

                string rtfFile = Path.ChangeExtension(jsonFile, ".rtf");
                if (File.Exists(rtfFile))
                {
                    File.Delete(rtfFile);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"删除自动保存文件失败: {ex.Message}");
            }
        }
        #endregion

        #region 版本管理方法
        /// <summary>
        /// 获取自动保存的版本列表
        /// </summary>
        /// <param name="baseFileName">基础文件名</param>
        /// <returns>自动保存版本列表</returns>
        public static List<AutoSaveVersion> GetAutoSaveVersions(string baseFileName)
        {
            var versions = new List<AutoSaveVersion>();

            try
            {
                EnsureAutoSaveDirectoryExists();

                var autoSaveFiles = Directory.GetFiles(_autoSaveDirectory, $"{baseFileName}_*.json");
                foreach (string file in autoSaveFiles)
                {
                    var version = ParseAutoSaveVersion(file);
                    if (version != null)
                    {
                        versions.Add(version);
                    }
                }

                return versions.OrderByDescending(v => v.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取自动保存版本列表失败: {ex.Message}");
                return versions;
            }
        }

        /// <summary>
        /// 解析自动保存版本信息
        /// </summary>
        private static AutoSaveVersion ParseAutoSaveVersion(string filePath)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] parts = fileName.Split('_');

                if (parts.Length >= 3)
                {
                    string dateStr = parts[parts.Length - 2];
                    string timeStr = parts[parts.Length - 1];

                    if (DateTime.TryParseExact(
                        $"{dateStr}_{timeStr}",
                        "yyyyMMdd_HHmmss",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime timestamp))
                    {
                        return new AutoSaveVersion
                        {
                            FilePath = filePath,
                            Timestamp = timestamp
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"解析自动保存版本信息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 恢复自动保存的版本
        /// </summary>
        /// <param name="version">要恢复的版本</param>
        /// <returns>恢复的页面信息</returns>
        public static PageJsonInfo RestoreAutoSaveVersion(AutoSaveVersion version)
        {
            try
            {
                if (version == null)
                {
                    throw new ArgumentNullException(nameof(version));
                }

                if (!File.Exists(version.FilePath))
                {
                    throw new FileNotFoundException("自动保存文件不存在", version.FilePath);
                }

                var pageInfo = PageJsonInfo.LoadFromFile(version.FilePath);
                RestoreAssociatedTextFile(version, pageInfo);

                Logger.Log($"已恢复自动保存版本: {Path.GetFileName(version.FilePath)}");
                return pageInfo;
            }
            catch (Exception ex)
            {
                Logger.LogError($"恢复自动保存版本失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 恢复关联的文本文件
        /// </summary>
        private static void RestoreAssociatedTextFile(AutoSaveVersion version, PageJsonInfo pageInfo)
        {
            string rtfPath = Path.ChangeExtension(version.FilePath, ".rtf");
            if (File.Exists(rtfPath) && !string.IsNullOrEmpty(pageInfo.TextPath))
            {
                string originalTextPath = pageInfo.GetAbsoluteTextPath();
                File.Copy(rtfPath, originalTextPath, true);
            }
        }
        #endregion
    }

    /// <summary>
    /// 自动保存版本信息类
    /// </summary>
    public class AutoSaveVersion
    {
        /// <summary>
        /// 自动保存文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 保存时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName => $"{Path.GetFileNameWithoutExtension(FilePath)} ({Timestamp:yyyy-MM-dd HH:mm:ss})";
    }
}