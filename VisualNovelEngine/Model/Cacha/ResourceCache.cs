using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.IO;
using System.Windows.Media;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Windows.Threading;
using VisualNovelEngine.Model.Page;
using VisualNovelEngine.Model.Logging;
using System.Windows;

namespace VisualNovelEngine.Model.Cache
{
    /// <summary>
    /// 资源缓存管理器
    /// 负责管理和优化图片、音频、文本和页面信息的缓存
    /// 包含内存监控、资源预加载和缓存清理等功能
    /// </summary>
    public static class ResourceCache
    {
        #region 缓存容器
        /// <summary>
        /// 图片资源缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, CachedResource<BitmapImage>> _imageCache =
            new ConcurrentDictionary<string, CachedResource<BitmapImage>>();

        /// <summary>
        /// 音频资源缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, CachedResource<MediaPlayer>> _audioCache =
            new ConcurrentDictionary<string, CachedResource<MediaPlayer>>();

        /// <summary>
        /// 文本资源缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, CachedResource<FlowDocument>> _textCache =
            new ConcurrentDictionary<string, CachedResource<FlowDocument>>();

        /// <summary>
        /// 页面信息缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, CachedResource<PageJsonInfo>> _pageCache =
            new ConcurrentDictionary<string, CachedResource<PageJsonInfo>>();
        #endregion

        #region 配置参数
        /// <summary>
        /// 最大缓存项数
        /// </summary>
        private static int _maxCacheSize = 100;

        /// <summary>
        /// 缓存过期时间（30分钟）
        /// </summary>
        private static TimeSpan _cacheExpiryTime = TimeSpan.FromMinutes(30);

        /// <summary>
        /// 内存使用阈值（500MB）
        /// </summary>
        private static long _memoryThreshold = 500 * 1024 * 1024;

        /// <summary>
        /// 是否启用内存监控
        /// </summary>
        private static bool _isMemoryMonitoringEnabled = true;

        /// <summary>
        /// 最大并发预加载数
        /// </summary>
        private static int _maxConcurrentPreloads = 3;
        #endregion

        #region 性能监控
        private static readonly Stopwatch _performanceTimer = new Stopwatch();
        private static int _cacheHits = 0;
        private static int _cacheMisses = 0;
        #endregion

        #region 内存监控
        private static DispatcherTimer _memoryMonitorTimer;
        private static int _currentPreloads = 0;
        private static bool _isPreloadingActive = false;
        private static readonly ConcurrentQueue<PreloadRequest> _preloadQueue = new ConcurrentQueue<PreloadRequest>();
        #endregion

        /// <summary>
        /// 初始化资源缓存系统
        /// </summary>
        static ResourceCache()
        {
            InitializeMemoryMonitoring();
            Logger.Log("资源缓存系统已初始化");
        }

        /// <summary>
        /// 初始化内存监控系统
        /// </summary>
        private static void InitializeMemoryMonitoring()
        {
            if (_isMemoryMonitoringEnabled)
            {
                _memoryMonitorTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(1)
                };
                _memoryMonitorTimer.Tick += (s, e) => CheckMemoryUsage();
                _memoryMonitorTimer.Start();

                Logger.Log("内存监控系统已启动");
            }
        }

        /// <summary>
        /// 检查内存使用情况并在必要时进行清理
        /// </summary>
        private static void CheckMemoryUsage()
        {
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                long memoryUsed = currentProcess.PrivateMemorySize64;

                Logger.Log($"当前内存使用: {memoryUsed / (1024 * 1024)}MB");

                if (memoryUsed > _memoryThreshold)
                {
                    Logger.Log("内存使用超过阈值，开始清理缓存...");
                    TrimCache();
                    GC.Collect();
                    Logger.Log($"缓存清理完成，当前内存使用: {Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024)}MB");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"内存监控出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理缓存，移除最旧和过期的资源
        /// </summary>
        private static void TrimCache()
        {
            try
            {
                int targetSize = _maxCacheSize / 4;
                RemoveOldestItems(_imageCache, targetSize);
                RemoveOldestItems(_audioCache, targetSize);
                RemoveOldestItems(_textCache, targetSize);
                RemoveOldestItems(_pageCache, targetSize);

                RemoveExpiredItems(_imageCache);
                RemoveExpiredItems(_audioCache);
                RemoveExpiredItems(_textCache);
                RemoveExpiredItems(_pageCache);

                Logger.Log("缓存清理完成");
            }
            catch (Exception ex)
            {
                Logger.LogError($"缓存清理出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        public static CacheStatistics GetStatistics()
        {
            return new CacheStatistics
            {
                ImageCacheCount = _imageCache.Count,
                AudioCacheCount = _audioCache.Count,
                TextCacheCount = _textCache.Count,
                PageCacheCount = _pageCache.Count,
                CacheHits = _cacheHits,
                CacheMisses = _cacheMisses,
                MemoryUsage = Process.GetCurrentProcess().PrivateMemorySize64
            };
        }

        /// <summary>
        /// 预加载页面资源
        /// </summary>
        /// <param name="page">要预加载的页面</param>
        public static void PreloadPage(PageJsonInfo page)
        {
            if (page == null) return;

            _performanceTimer.Restart();

            // 缓存页面JSON
            CachePage(page);

            // 预加载当前页面资源
            if (!string.IsNullOrEmpty(page.ImagePath))
            {
                CacheImage(page.GetAbsoluteImagePath());
            }

            if (!string.IsNullOrEmpty(page.MusicPath))
            {
                CacheAudio(page.GetAbsoluteMusicPath());
            }

            if (!string.IsNullOrEmpty(page.TextPath))
            {
                CacheText(page.GetAbsoluteTextPath());
            }

            _performanceTimer.Stop();
            Logger.Log($"当前页面资源加载完成，耗时: {_performanceTimer.ElapsedMilliseconds}ms");

            // 异步预加载下一页和前一页
            Task.Run(() => PreloadAdjacentPages(page));
        }

        /// <summary>
        /// 预加载相邻页面
        /// </summary>
        /// <param name="currentPage">当前页面</param>
        private static async Task PreloadAdjacentPages(PageJsonInfo currentPage)
        {
            try
            {
                // 预加载下一页
                if (!string.IsNullOrEmpty(currentPage.NextPagePath))
                {
                    string nextPagePath = Path.Combine(
                        Path.GetDirectoryName(currentPage.GetAbsoluteTextPath()),
                        "..",
                        $"{currentPage.NextPagePath}.json");

                    if (File.Exists(nextPagePath))
                    {
                        EnqueuePreloadRequest(nextPagePath, PreloadPriority.High);
                    }
                }

                // 预加载前一页
                if (!string.IsNullOrEmpty(currentPage.PreviousPagePath))
                {
                    string prevPagePath = Path.Combine(
                        Path.GetDirectoryName(currentPage.GetAbsoluteTextPath()),
                        "..",
                        $"{currentPage.PreviousPagePath}.json");

                    if (File.Exists(prevPagePath))
                    {
                        EnqueuePreloadRequest(prevPagePath, PreloadPriority.Medium);
                    }
                }

                // 预加载跳转点目标页面
                if (currentPage.JumpPoints != null)
                {
                    foreach (var jumpPoint in currentPage.JumpPoints)
                    {
                        if (!string.IsNullOrEmpty(jumpPoint.TargetPage))
                        {
                            string targetGroup = jumpPoint.TargetGroup ?? Path.GetFileName(Path.GetDirectoryName(currentPage.GetAbsoluteTextPath()));
                            string jumpPagePath = Path.Combine(
                                Path.GetDirectoryName(Path.GetDirectoryName(currentPage.GetAbsoluteTextPath())),
                                targetGroup,
                                $"{jumpPoint.TargetPage}.json");

                            if (File.Exists(jumpPagePath))
                            {
                                EnqueuePreloadRequest(jumpPagePath, PreloadPriority.Low);
                            }
                        }
                    }
                }

                // 开始处理预加载队列
                await ProcessPreloadQueue();
            }
            catch (Exception ex)
            {
                Logger.LogError("预加载相邻页面失败", ex);
            }
        }

        /// <summary>
        /// 将预加载请求加入队列
        /// </summary>
        /// <param name="pagePath">页面路径</param>
        /// <param name="priority">预加载优先级</param>
        private static void EnqueuePreloadRequest(string pagePath, PreloadPriority priority)
        {
            _preloadQueue.Enqueue(new PreloadRequest { Path = pagePath, Priority = priority });

            // 如果预加载未激活，启动预加载处理
            if (!_isPreloadingActive)
            {
                Task.Run(() => ProcessPreloadQueue());
            }
        }

        /// <summary>
        /// 处理预加载队列
        /// </summary>
        private static async Task ProcessPreloadQueue()
        {
            if (_isPreloadingActive) return;

            _isPreloadingActive = true;

            try
            {
                while (_preloadQueue.Count > 0 && _currentPreloads < _maxConcurrentPreloads)
                {
                    if (_preloadQueue.TryDequeue(out PreloadRequest request))
                    {
                        _currentPreloads++;

                        // 异步预加载页面
                        await Task.Run(() =>
                        {
                            try
                            {
                                if (File.Exists(request.Path))
                                {
                                    var pageInfo = PageJsonInfo.LoadFromFile(request.Path);
                                    CachePage(pageInfo);

                                    // 根据优先级决定是否预加载资源
                                    if (request.Priority <= PreloadPriority.Medium)
                                    {
                                        if (!string.IsNullOrEmpty(pageInfo.ImagePath))
                                        {
                                            CacheImage(pageInfo.GetAbsoluteImagePath());
                                        }
                                    }

                                    if (request.Priority <= PreloadPriority.High)
                                    {
                                        if (!string.IsNullOrEmpty(pageInfo.TextPath))
                                        {
                                            CacheText(pageInfo.GetAbsoluteTextPath());
                                        }
                                    }

                                    Logger.Log($"预加载页面完成: {Path.GetFileName(request.Path)}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError($"预加载页面失败: {request.Path}", ex);
                            }
                            finally
                            {
                                _currentPreloads--;
                            }
                        });
                    }
                }
            }
            finally
            {
                _isPreloadingActive = false;

                // 如果队列中还有项目且有可用的预加载槽，继续处理
                if (_preloadQueue.Count > 0 && _currentPreloads < _maxConcurrentPreloads)
                {
                    await ProcessPreloadQueue();
                }
            }
        }

        public static void ClearCache()
        {
            lock (_imageCache)
            {
                _imageCache.Clear();
            }

            lock (_textCache)
            {
                _textCache.Clear();
            }
        }


        /// <summary>
        /// 缓存页面
        /// </summary>
        /// <param name="page">页面信息</param>
        private static void CachePage(PageJsonInfo page)
        {
            if (page == null) return;

            string key = page.GetAbsoluteTextPath();
            _pageCache[key] = new CachedResource<PageJsonInfo>(page);
        }

        /// <summary>
        /// 缓存图片
        /// </summary>
        /// <param name="absolutePath">图片绝对路径</param>
        /// <returns>缓存的图片</returns>
        public static BitmapImage CacheImage(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
            {
                return null;
            }

            if (_imageCache.TryGetValue(absolutePath, out CachedResource<BitmapImage> cachedResource))
            {
                _cacheHits++;
                cachedResource.LastAccessTime = DateTime.Now;
                return cachedResource.Resource;
            }

            _cacheMisses++;

            try
            {
                _performanceTimer.Restart();

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(absolutePath);
                image.EndInit();
                image.Freeze();

                _performanceTimer.Stop();

                _imageCache[absolutePath] = new CachedResource<BitmapImage>(image);

                Logger.Log($"图片已缓存: {Path.GetFileName(absolutePath)}, 大小: {GetFileSize(absolutePath)}KB, 加载时间: {_performanceTimer.ElapsedMilliseconds}ms");

                return image;
            }
            catch (Exception ex)
            {
                Logger.LogError($"缓存图片失败: {absolutePath}", ex);
                return null;
            }
        }

        /// <summary>
        /// 缓存音频
        /// </summary>
        /// <param name="absolutePath">音频绝对路径</param>
        /// <returns>缓存的音频播放器</returns>
        public static MediaPlayer CacheAudio(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
            {
                return null;
            }

            if (_audioCache.TryGetValue(absolutePath, out CachedResource<MediaPlayer> cachedResource))
            {
                _cacheHits++;
                cachedResource.LastAccessTime = DateTime.Now;
                return cachedResource.Resource;
            }

            _cacheMisses++;

            try
            {
                _performanceTimer.Restart();

                var player = new MediaPlayer();
                player.Open(new Uri(absolutePath));

                _performanceTimer.Stop();

                _audioCache[absolutePath] = new CachedResource<MediaPlayer>(player);

                Logger.Log($"音频已缓存: {Path.GetFileName(absolutePath)}, 大小: {GetFileSize(absolutePath)}KB, 加载时间: {_performanceTimer.ElapsedMilliseconds}ms");

                return player;
            }
            catch (Exception ex)
            {
                Logger.LogError($"缓存音频失败: {absolutePath}", ex);
                return null;
            }
        }

        /// <summary>
        /// 缓存文本
        /// </summary>
        /// <param name="absolutePath">文本绝对路径</param>
        /// <returns>缓存的流文档</returns>
        public static FlowDocument CacheText(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
            {
                return null;
            }

            if (_textCache.TryGetValue(absolutePath, out CachedResource<FlowDocument> cachedResource))
            {
                _cacheHits++;
                cachedResource.LastAccessTime = DateTime.Now;
                return cachedResource.Resource;
            }

            _cacheMisses++;

            try
            {
                _performanceTimer.Restart();

                var document = new FlowDocument();
                using (FileStream fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read))
                {
                    var textRange = new TextRange(document.ContentStart, document.ContentEnd);
                    textRange.Load(fileStream, DataFormats.Rtf);
                }

                _performanceTimer.Stop();

                _textCache[absolutePath] = new CachedResource<FlowDocument>(document);

                Logger.Log($"文本已缓存: {Path.GetFileName(absolutePath)}, 大小: {GetFileSize(absolutePath)}KB, 加载时间: {_performanceTimer.ElapsedMilliseconds}ms");

                return document;
            }
            catch (Exception ex)
            {
                Logger.LogError($"缓存文本失败: {absolutePath}", ex);
                return null;
            }
        }

        /// <summary>
        /// 获取缓存的图片
        /// </summary>
        /// <param name="absolutePath">图片绝对路径</param>
        /// <returns>缓存的图片</returns>
        public static BitmapImage GetCachedImage(string absolutePath)
        {
            return CacheImage(absolutePath);
        }

        /// <summary>
        /// 获取缓存的音频
        /// </summary>
        /// <param name="absolutePath">音频绝对路径</param>
        /// <returns>缓存的音频播放器</returns>
        public static MediaPlayer GetCachedAudio(string absolutePath)
        {
            return CacheAudio(absolutePath);
        }

        /// <summary>
        /// 获取缓存的文本
        /// </summary>
        /// <param name="absolutePath">文本绝对路径</param>
        /// <returns>缓存的流文档</returns>
        public static FlowDocument GetCachedText(string absolutePath)
        {
            return CacheText(absolutePath);
        }

        /// <summary>
        /// 获取缓存的页面
        /// </summary>
        /// <param name="absolutePath">页面绝对路径</param>
        /// <returns>缓存的页面信息</returns>
        public static PageJsonInfo GetCachedPage(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
            {
                return null;
            }

            if (_pageCache.TryGetValue(absolutePath, out CachedResource<PageJsonInfo> cachedResource))
            {
                _cacheHits++;
                cachedResource.LastAccessTime = DateTime.Now;
                return cachedResource.Resource;
            }

            _cacheMisses++;

            try
            {
                var page = PageJsonInfo.LoadFromFile(absolutePath);
                _pageCache[absolutePath] = new CachedResource<PageJsonInfo>(page);
                return page;
            }
            catch (Exception ex)
            {
                Logger.LogError($"加载页面失败: {absolutePath}", ex);
                return null;
            }
        }

        /// <summary>
        /// 获取文件大小（KB）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件大小（KB）</returns>
        private static long GetFileSize(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length / 1024;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 移除最旧的项
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="cache">缓存字典</param>
        /// <param name="keepCount">保留的项数</param>
        private static void RemoveOldestItems<T>(ConcurrentDictionary<string, CachedResource<T>> cache, int keepCount)
        {
            var oldestItems = cache.OrderBy(x => x.Value.LastAccessTime)
                                  .Take(Math.Max(0, cache.Count - keepCount))
                                  .Select(x => x.Key)
                                  .ToList();

            foreach (var key in oldestItems)
            {
                if (cache.TryRemove(key, out CachedResource<T> removed) &&
                    removed.Resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// 移除过期项
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="cache">缓存字典</param>
        private static void RemoveExpiredItems<T>(ConcurrentDictionary<string, CachedResource<T>> cache)
        {
            var now = DateTime.Now;
            var expiredItems = cache.Where(x => (now - x.Value.LastAccessTime) > _cacheExpiryTime)
                                   .Select(x => x.Key)
                                   .ToList();

            foreach (var key in expiredItems)
            {
                if (cache.TryRemove(key, out CachedResource<T> removed) &&
                    removed.Resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// 设置缓存配置
        /// </summary>
        /// <param name="maxCacheSize">最大缓存项数</param>
        /// <param name="cacheExpiryTime">缓存过期时间</param>
        /// <param name="memoryThreshold">内存阈值</param>
        public static void SetCacheConfiguration(int maxCacheSize, TimeSpan cacheExpiryTime, long memoryThreshold)
        {
            _maxCacheSize = maxCacheSize;
            _cacheExpiryTime = cacheExpiryTime;
            _memoryThreshold = memoryThreshold;

            Logger.Log($"缓存配置已更新: 最大缓存项数={maxCacheSize}, 过期时间={cacheExpiryTime}, 内存阈值={memoryThreshold / (1024 * 1024)}MB");
        }
    }

    /// <summary>
    /// 缓存资源包装类
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    public class CachedResource<T>
    {
        /// <summary>
        /// 资源对象
        /// </summary>
        public T Resource { get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; }

        /// <summary>
        /// 最后访问时间
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="resource">资源对象</param>
        public CachedResource(T resource)
        {
            Resource = resource;
            CreationTime = DateTime.Now;
            LastAccessTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 预加载请求
    /// </summary>
    public class PreloadRequest
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 预加载优先级
        /// </summary>
        public PreloadPriority Priority { get; set; }
    }

    /// <summary>
    /// 预加载优先级
    /// </summary>
    public enum PreloadPriority
    {
        /// <summary>高优先级，立即需要的资源</summary>
        High = 0,

        /// <summary>中优先级，可能很快需要的资源</summary>
        Medium = 1,

        /// <summary>低优先级，可能需要的资源</summary>
        Low = 2
    }

    /// <summary>
    /// 缓存统计信息类
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>图片缓存数量</summary>
        public int ImageCacheCount { get; set; }

        /// <summary>音频缓存数量</summary>
        public int AudioCacheCount { get; set; }

        /// <summary>文本缓存数量</summary>
        public int TextCacheCount { get; set; }

        /// <summary>页面缓存数量</summary>
        public int PageCacheCount { get; set; }

        /// <summary>缓存命中次数</summary>
        public int CacheHits { get; set; }

        /// <summary>缓存未命中次数</summary>
        public int CacheMisses { get; set; }

        /// <summary>内存使用量（MB）</summary>
        public long MemoryUsage { get; set; }
    }
}