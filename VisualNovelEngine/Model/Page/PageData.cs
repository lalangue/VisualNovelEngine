using System;
using VisualNovelEngine.Model.Logging;

namespace VisualNovelEngine.Model.Page
{
    /// <summary>
    /// 页面数据管理类
    /// 负责管理当前页面的路径和版本信息
    /// </summary>
    public static class PageData
    {
        #region 事件
        /// <summary>
        /// 页面变更事件
        /// 当页面路径发生变化时触发
        /// </summary>
        public static event EventHandler<PageChangedEventArgs> PageChanged;
        #endregion

        #region 字段
        private static string _pagePath;
        private static bool _isLatestVersion = true;
        #endregion

        #region 属性
        /// <summary>
        /// 获取或设置当前页面的路径
        /// </summary>
        public static string PagePath
        {
            get => _pagePath;
            set
            {
                try
                {
                    if (_pagePath == value) return;

                    string oldPath = _pagePath;
                    _pagePath = value;

                    Logger.Log($"页面路径从 {oldPath ?? "无"} 更改为 {value ?? "无"}");
                    OnPageChanged(oldPath, value);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"设置页面路径时出错: {ex.Message}");
                    throw;
                }
            }
        }


        /// <summary>
        /// 获取或设置页面是否为最新版本
        /// </summary>
        public static bool IsLatestVersion
        {
            get => _isLatestVersion;
            set
            {
                if (_isLatestVersion != value)
                {
                    _isLatestVersion = value;
                    Logger.Log($"页面版本状态更改为: {(value ? "最新" : "已修改")}");
                }
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 触发页面变更事件
        /// </summary>
        private static void OnPageChanged(string oldPath, string newPath)
        {
            try
            {
                PageChanged?.Invoke(
                    null, // 发件人可以设置为 null 或者本类型
                    new PageChangedEventArgs(oldPath, newPath) // 用新的参数调用
                );
            }
            catch (Exception ex)
            {
                Logger.LogError($"触发页面变更事件时出错: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// 页面变更事件参数类
    /// </summary>
    public class PageChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 原页面路径
        /// </summary>
        public string OldPath { get; }

        /// <summary>
        /// 新页面路径
        /// </summary>
        public string NewPath { get; }

        /// <summary>
        /// 初始化页面变更事件参数
        /// </summary>
        /// <param name="oldPath">原页面路径</param>
        /// <param name="newPath">新页面路径</param>
        public PageChangedEventArgs(string oldPath, string newPath)
        {
            OldPath = oldPath;
            NewPath = newPath;
        }
    }
}


