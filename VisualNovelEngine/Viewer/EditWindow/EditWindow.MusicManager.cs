using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualNovelEngine.Model.Group;
using VisualNovelEngine.Model.Project;
using VisualNovelEngine.Model.Animation;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using VisualNovelEngine.Model.Page;
using VisualNovelEngine.Model.Logging;

namespace VisualNovelEngine.Viewer.EditWindow
{
    /// <summary>
    /// EditWindow 的音乐管理部分
    /// </summary>
    public partial class EditWindow
    {

        

        /// <summary>
        /// 设置背景音乐
        /// </summary>
        /// <param name="musicPath">音乐文件路径，如果为 null 或空字符串则停止播放</param>
        /// <remarks>
        /// 如果提供的路径与当前播放的音乐相同，则不会重新加载
        /// </remarks>
        public void SetBackgroundMusic(string musicPath)
        {
            try
            {
                // 如果路径为空，停止播放并清除音乐源
                if (string.IsNullOrEmpty(musicPath))
                {
                    MusicPlayer.Stop();
                    MusicPlayer.Source = null;
                    return;
                }

                // 如果音乐源为空或与当前不同，则加载新音乐
                if (MusicPlayer.Source == null || !MusicPlayer.Source.ToString().Equals(musicPath))
                {
                    MusicPlayer.Source = new Uri(musicPath, UriKind.RelativeOrAbsolute);
                    MusicPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                // 记录错误并确保播放器停止
                Logger.LogError($"设置背景音乐失败: {musicPath}", ex);
                MusicPlayer.Stop();
                MusicPlayer.Source = null;
            }
        }

        /// <summary>
        /// 打开音乐选择对话框
        /// </summary>
        /// <returns>选择的音乐文件路径，如果用户取消则返回 null</returns>
        /// <remarks>
        /// 对话框初始目录设置为项目的音乐文件夹
        /// </remarks>
        public string OpenMusicDialogWithInitialPath()
        {
            try
            {
                // 获取音乐文件夹路径
                string musicFolderPath = Path.Combine(ProjectData.ProjectPath, GroupData.MusicFolderPath);

                // 确保目录存在
                if (!Directory.Exists(musicFolderPath))
                {
                    Directory.CreateDirectory(musicFolderPath);
                }

                // 创建并配置文件对话框
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "选择音乐文件",
                    Filter = "音乐文件|*.mp3;*.wav;*.ogg;*.flac",
                    InitialDirectory = musicFolderPath,
                    CheckFileExists = true,
                    Multiselect = false
                };

                // 显示对话框并返回结果
                return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
            }
            catch (Exception ex)
            {
                Logger.LogError("打开音乐对话框失败", ex);
                return null;
            }
        }
    }
}

namespace VisualNovelEngine.Model.Animation
{
    /// <summary>
    /// 过渡效果类型
    /// </summary>
    public enum TransitionType
    {
        /// <summary>无过渡效果</summary>
        None,
        /// <summary>淡入淡出效果</summary>
        Fade,
        /// <summary>滑动效果</summary>
        Slide,
        /// <summary>溶解效果</summary>
        Dissolve,
        /// <summary>缩放效果</summary>
        Zoom,
        /// <summary>翻转效果</summary>
        Flip,
        /// <summary>交叉淡入淡出效果</summary>
        Crossfade
    }

    /// <summary>
    /// 过渡效果管理器，负责处理页面切换动画
    /// </summary>
    public class TransitionManager
    {
        // 默认过渡配置
        private static readonly TransitionConfig _defaultConfig = new TransitionConfig
        {
            TransitionType = TransitionType.Fade,
            Duration = TimeSpan.FromMilliseconds(500),
            EnableTransition = true
        };

        // 当前配置
        private static TransitionConfig _currentConfig;

        // 场景特定配置
        private static readonly Dictionary<string, TransitionConfig> _sceneConfigs = new Dictionary<string, TransitionConfig>();

        // 过渡状态
        private bool _isTransitioning = false;
        private Storyboard _transitionStoryboard;

        /// <summary>
        /// 静态构造函数，初始化默认配置
        /// </summary>
        static TransitionManager()
        {
            _currentConfig = _defaultConfig.Clone();
        }

        /// <summary>
        /// 设置全局过渡配置
        /// </summary>
        /// <param name="config">要应用的过渡配置</param>
        public static void SetGlobalConfig(TransitionConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _currentConfig = config.Clone();
            Logger.Log($"已设置全局过渡配置: {config.TransitionType}, 持续时间: {config.Duration.TotalMilliseconds}ms");
        }

        /// <summary>
        /// 设置场景特定过渡配置
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="config">要应用的过渡配置</param>
        public static void SetSceneConfig(string sceneName, TransitionConfig config)
        {
            if (string.IsNullOrEmpty(sceneName))
                throw new ArgumentNullException(nameof(sceneName));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _sceneConfigs[sceneName] = config.Clone();
            Logger.Log($"已为场景 '{sceneName}' 设置过渡配置: {config.TransitionType}, 持续时间: {config.Duration.TotalMilliseconds}ms");
        }

        /// <summary>
        /// 获取场景配置（如果没有则返回全局配置）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>过渡配置</returns>
        public static TransitionConfig GetConfigForScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || !_sceneConfigs.ContainsKey(sceneName))
            {
                return _currentConfig;
            }
            return _sceneConfigs[sceneName];
        }

        /// <summary>
        /// 应用过渡效果
        /// </summary>
        /// <param name="oldElement">旧元素</param>
        /// <param name="newElement">新元素</param>
        /// <param name="sceneName">场景名称（可选）</param>
        public void ApplyTransition(FrameworkElement oldElement, FrameworkElement newElement, string sceneName = null)
        {
            // 验证参数
            if (oldElement == null || newElement == null)
            {
                Logger.LogWarning("应用过渡效果失败: 元素为空");
                return;
            }

            // 在编辑模式下不应用过渡
            if (!PageData.IsLatestVersion)
            {
                oldElement.Visibility = Visibility.Collapsed;
                newElement.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                // 获取配置
                var config = sceneName != null ? GetConfigForScene(sceneName) : _currentConfig;

                // 如果禁用过渡，直接切换
                if (!config.EnableTransition || config.TransitionType == TransitionType.None)
                {
                    oldElement.Visibility = Visibility.Collapsed;
                    newElement.Visibility = Visibility.Visible;
                    return;
                }

                // 如果已经在过渡中，取消当前过渡
                if (_isTransitioning && _transitionStoryboard != null)
                {
                    _transitionStoryboard.Stop();
                }

                // 初始化过渡
                _isTransitioning = true;

                // 创建故事板
                _transitionStoryboard = new Storyboard();
                _transitionStoryboard.Completed += (s, e) =>
                {
                    _isTransitioning = false;
                    oldElement.Visibility = Visibility.Collapsed;
                    Logger.Log($"过渡效果完成: {config.TransitionType}");
                };

                // 根据过渡类型创建动画
                switch (config.TransitionType)
                {
                    case TransitionType.Fade:
                        CreateFadeTransition(oldElement, newElement, config.Duration);
                        break;
                    case TransitionType.Slide:
                        // 未实现的过渡类型使用淡入淡出代替
                        CreateFadeTransition(oldElement, newElement, config.Duration);
                        Logger.LogWarning("滑动过渡效果尚未实现，使用淡入淡出代替");
                        break;
                    case TransitionType.Dissolve:
                        // 未实现的过渡类型使用淡入淡出代替
                        CreateFadeTransition(oldElement, newElement, config.Duration);
                        Logger.LogWarning("溶解过渡效果尚未实现，使用淡入淡出代替");
                        break;
                    case TransitionType.Zoom:
                        // 未实现的过渡类型使用淡入淡出代替
                        CreateFadeTransition(oldElement, newElement, config.Duration);
                        Logger.LogWarning("缩放过渡效果尚未实现，使用淡入淡出代替");
                        break;
                    case TransitionType.Flip:
                        // 未实现的过渡类型使用淡入淡出代替
                        CreateFadeTransition(oldElement, newElement, config.Duration);
                        Logger.LogWarning("翻转过渡效果尚未实现，使用淡入淡出代替");
                        break;
                    case TransitionType.Crossfade:
                        // 未实现的过渡类型使用淡入淡出代替
                        CreateFadeTransition(oldElement, newElement, config.Duration);
                        Logger.LogWarning("交叉淡入淡出过渡效果尚未实现，使用淡入淡出代替");
                        break;
                    default:
                        CreateFadeTransition(oldElement, newElement, config.Duration);
                        break;
                }

                // 开始过渡
                _transitionStoryboard.Begin();
                Logger.Log($"开始过渡效果: {config.TransitionType}, 持续时间: {config.Duration.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                // 发生错误时，直接切换元素
                _isTransitioning = false;
                oldElement.Visibility = Visibility.Collapsed;
                newElement.Visibility = Visibility.Visible;
                Logger.LogError("应用过渡效果失败", ex);
            }
        }

        /// <summary>
        /// 创建淡入淡出过渡
        /// </summary>
        /// <param name="oldElement">旧元素</param>
        /// <param name="newElement">新元素</param>
        /// <param name="duration">过渡持续时间</param>
        private void CreateFadeTransition(FrameworkElement oldElement, FrameworkElement newElement, TimeSpan duration)
        {
            // 设置初始状态
            oldElement.Opacity = 1;
            newElement.Opacity = 0;
            newElement.Visibility = Visibility.Visible;

            // 创建旧元素淡出动画
            var fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(duration)
            };
            Storyboard.SetTarget(fadeOutAnimation, oldElement);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));
            _transitionStoryboard.Children.Add(fadeOutAnimation);

            // 创建新元素淡入动画
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(duration)
            };
            Storyboard.SetTarget(fadeInAnimation, newElement);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));
            _transitionStoryboard.Children.Add(fadeInAnimation);
        }

        // 注意：以下方法是未实现的过渡效果的占位符
        // 实际项目中应该实现这些方法或删除它们

        /// <summary>
        /// 创建滑动过渡（未实现）
        /// </summary>
        private void CreateSlideTransition(FrameworkElement oldElement, FrameworkElement newElement, TimeSpan duration)
        {
            // TODO: 实现滑动过渡效果
        }

        /// <summary>
        /// 创建溶解过渡（未实现）
        /// </summary>
        private void CreateDissolveTransition(FrameworkElement oldElement, FrameworkElement newElement, TimeSpan duration)
        {
            // TODO: 实现溶解过渡效果
        }

        /// <summary>
        /// 创建缩放过渡（未实现）
        /// </summary>
        private void CreateZoomTransition(FrameworkElement oldElement, FrameworkElement newElement, TimeSpan duration)
        {
            // TODO: 实现缩放过渡效果
        }

        /// <summary>
        /// 创建翻转过渡（未实现）
        /// </summary>
        private void CreateFlipTransition(FrameworkElement oldElement, FrameworkElement newElement, TimeSpan duration)
        {
            // TODO: 实现翻转过渡效果
        }

        /// <summary>
        /// 创建交叉淡入淡出过渡（未实现）
        /// </summary>
        private void CreateCrossfadeTransition(FrameworkElement oldElement, FrameworkElement newElement, TimeSpan duration)
        {
            // TODO: 实现交叉淡入淡出过渡效果
        }
    }

    /// <summary>
    /// 过渡效果配置
    /// </summary>
    public class TransitionConfig
    {
        /// <summary>
        /// 过渡类型
        /// </summary>
        public TransitionType TransitionType { get; set; }

        /// <summary>
        /// 过渡持续时间
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 是否启用过渡
        /// </summary>
        public bool EnableTransition { get; set; }

        /// <summary>
        /// 创建配置副本
        /// </summary>
        /// <returns>配置副本</returns>
        public TransitionConfig Clone()
        {
            return new TransitionConfig
            {
                TransitionType = this.TransitionType,
                Duration = this.Duration,
                EnableTransition = this.EnableTransition
            };
        }
    }
}

namespace VisualNovelEngine.Model.Resource
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum ResourceType
    {
        /// <summary>图像资源</summary>
        Image,
        /// <summary>音频资源</summary>
        Audio,
        /// <summary>文本资源</summary>
        Text,
        /// <summary>视频资源</summary>
        Video,
        /// <summary>其他资源</summary>
        Other
    }

    /// <summary>
    /// 资源项，表示项目中的单个资源文件
    /// </summary>
    public class ResourceItem
    {
        /// <summary>资源名称</summary>
        public string Name { get; set; }

        /// <summary>资源路径</summary>
        public string Path { get; set; }

        /// <summary>资源类型</summary>
        public ResourceType Type { get; set; }

        /// <summary>资源标签</summary>
        public string[] Tags { get; set; }

        /// <summary>最后修改时间</summary>
        public DateTime LastModified { get; set; }

        /// <summary>文件大小（字节）</summary>
        public long Size { get; set; }

        /// <summary>资源是否正在使用</summary>
        public bool IsInUse { get; set; }

        /// <summary>缩略图（仅图片资源）</summary>
        public BitmapImage Thumbnail { get; set; }
    }

    /// <summary>
    /// 资源管理器，负责管理项目资源
    /// </summary>
    public class ResourceManager
    {
        // 资源集合
        private ObservableCollection<ResourceItem> _resources = new ObservableCollection<ResourceItem>();

        // 资源目录
        private string _resourceDirectory;

        /// <summary>
        /// 资源变更事件
        /// </summary>
        public event EventHandler<ResourceChangedEventArgs> ResourceChanged;

        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        /// <param name="projectPath">项目路径</param>
        public void Initialize(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                Logger.LogWarning("初始化资源管理器失败: 项目路径为空");
                return;
            }

            try
            {
                // 设置资源目录
                _resourceDirectory = Path.Combine(projectPath, "content");

                // 确保目录存在
                if (!Directory.Exists(_resourceDirectory))
                {
                    Directory.CreateDirectory(_resourceDirectory);
                    Logger.Log($"已创建资源目录: {_resourceDirectory}");
                }

                // 扫描资源
                ScanResources();

                Logger.Log($"资源管理器已初始化，资源目录: {_resourceDirectory}");
            }
            catch (Exception ex)
            {
                Logger.LogError("初始化资源管理器失败", ex);
            }
        }

        /// <summary>
        /// 扫描资源
        /// </summary>
        public void ScanResources()
        {
            try
            {
                _resources.Clear();

                if (string.IsNullOrEmpty(_resourceDirectory) || !Directory.Exists(_resourceDirectory))
                {
                    Logger.LogWarning("扫描资源失败: 资源目录不存在");
                    return;
                }

                // 扫描图片
                ScanResourcesByType(Path.Combine(_resourceDirectory, "images"), ResourceType.Image, "*.jpg", "*.png", "*.gif", "*.bmp");

                // 扫描音频
                ScanResourcesByType(Path.Combine(_resourceDirectory, "audio"), ResourceType.Audio, "*.mp3", "*.wav", "*.ogg");

                // 扫描文本
                ScanResourcesByType(Path.Combine(_resourceDirectory, "text"), ResourceType.Text, "*.txt", "*.rtf");

                // 扫描视频
                ScanResourcesByType(Path.Combine(_resourceDirectory, "video"), ResourceType.Video, "*.mp4", "*.avi", "*.mov");

                // 触发资源变更事件
                ResourceChanged?.Invoke(this, new ResourceChangedEventArgs { Action = ResourceAction.Refresh });

                Logger.Log($"资源扫描完成，共发现 {_resources.Count} 个资源");
            }
            catch (Exception ex)
            {
                Logger.LogError("扫描资源失败", ex);
            }
        }

        /// <summary>
        /// 扫描特定类型的资源
        /// </summary>
        /// <param name="directory">资源目录</param>
        /// <param name="type">资源类型</param>
        /// <param name="patterns">文件模式</param>
        private void ScanResourcesByType(string directory, ResourceType type, params string[] patterns)
        {
            try
            {
                // 确保目录存在
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Logger.Log($"已创建资源子目录: {directory}");
                    return;
                }

                int count = 0;

                // 扫描每种文件模式
                foreach (string pattern in patterns)
                {
                    foreach (string filePath in Directory.GetFiles(directory, pattern, SearchOption.AllDirectories))
                    {
                        var fileInfo = new FileInfo(filePath);

                        var item = new ResourceItem
                        {
                            Name = Path.GetFileName(filePath),
                            Path = filePath,
                            Type = type,
                            LastModified = fileInfo.LastWriteTime,
                            Size = fileInfo.Length,
                            Tags = new string[0]
                        };

                        // 为图片生成缩略图
                        if (type == ResourceType.Image)
                        {
                            try
                            {
                                var thumbnail = new BitmapImage();
                                thumbnail.BeginInit();
                                thumbnail.UriSource = new Uri(filePath);
                                thumbnail.DecodePixelWidth = 100;
                                thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                                thumbnail.EndInit();
                                thumbnail.Freeze();

                                item.Thumbnail = thumbnail;
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError($"生成缩略图失败: {filePath}", ex);
                            }
                        }

                        _resources.Add(item);
                        count++;
                    }
                }

                if (count > 0)
                {
                    Logger.Log($"已扫描 {type} 类型资源: {count} 个");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"扫描 {type} 类型资源失败", ex);
            }
        }

        /// <summary>
        /// 获取所有资源
        /// </summary>
        /// <returns>资源集合</returns>
        public ObservableCollection<ResourceItem> GetAllResources()
        {
            return _resources;
        }

        /// <summary>
        /// 获取特定类型的资源
        /// </summary>
        /// <param name="type">资源类型</param>
        /// <returns>指定类型的资源集合</returns>
        public IEnumerable<ResourceItem> GetResourcesByType(ResourceType type)
        {
            return _resources.Where(r => r.Type == type);
        }

        /// <summary>
        /// 获取带有特定标签的资源
        /// </summary>
        /// <param name="tag">标签</param>
        /// <returns>带有指定标签的资源集合</returns>
        public IEnumerable<ResourceItem> GetResourcesByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return _resources;

            return _resources.Where(r => r.Tags != null && r.Tags.Contains(tag));
        }

        /// <summary>
        /// 搜索资源
        /// </summary>
        /// <param name="query">搜索查询</param>
        /// <returns>匹配查询的资源集合</returns>
        public IEnumerable<ResourceItem> SearchResources(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return _resources;
            }

            return _resources.Where(r =>
                r.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (r.Tags != null && r.Tags.Any(t => t.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
            );
        }

        /// <summary>
        /// 添加资源（未实现）
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="type">资源类型</param>
        /// <returns>添加的资源项</returns>
        public ResourceItem AddResource(string sourcePath, ResourceType type)
        {
            // TODO: 实现添加资源的功能
            Logger.LogWarning("AddResource 方法尚未实现");
            return null;
        }
    }

    /// <summary>
    /// 资源操作类型
    /// </summary>
    public enum ResourceAction
    {
        /// <summary>刷新所有资源</summary>
        Refresh,
        /// <summary>添加资源</summary>
        Add,
        /// <summary>删除资源</summary>
        Remove,
        /// <summary>更新资源</summary>
        Update
    }

    /// <summary>
    /// 资源变更事件参数
    /// </summary>
    public class ResourceChangedEventArgs : EventArgs
    {
        /// <summary>资源操作类型</summary>
        public ResourceAction Action { get; set; }

        /// <summary>相关资源项（可选）</summary>
        public ResourceItem Item { get; set; }
    }
}
