using System;
using System.Collections.Generic;
using System.Linq;
using VisualNovelEngine.Model.Page;
using VisualNovelEngine.Model.Logging;

namespace VisualNovelEngine.Model.Navigation
{
    /// <summary>
    /// 导航管理器
    /// 负责管理页面导航、历史记录和跳转目标的提取
    /// </summary>
    public static class NavigationManager
    {
        #region 字段
        /// <summary>
        /// 后退历史记录栈
        /// </summary>
        private static readonly Stack<string> _backStack = new Stack<string>();

        /// <summary>
        /// 前进历史记录栈
        /// </summary>
        private static readonly Stack<string> _forwardStack = new Stack<string>();

        /// <summary>
        /// 当前页面路径
        /// </summary>
        private static string _currentPage;
        #endregion

        #region 事件
        /// <summary>
        /// 页面变更事件
        /// </summary>
        public static event EventHandler<NavigationEventArgs> PageChanged;
        #endregion

        #region 公共属性
        /// <summary>
        /// 是否可以返回上一页
        /// </summary>
        public static bool CanGoBack => _backStack.Count > 0;

        /// <summary>
        /// 是否可以前进到下一页
        /// </summary>
        public static bool CanGoForward => _forwardStack.Count > 0;

        /// <summary>
        /// 获取当前页面路径
        /// </summary>
        public static string CurrentPage => _currentPage;
        #endregion

        #region 导航方法
        /// <summary>
        /// 导航到指定页面
        /// </summary>
        /// <param name="pagePath">目标页面路径</param>
        /// <param name="addToStack">是否添加到导航历史</param>
        public static void NavigateTo(string pagePath, bool addToStack = true)
        {
            try
            {
                if (string.IsNullOrEmpty(pagePath))
                {
                    throw new ArgumentNullException(nameof(pagePath));
                }

                if (addToStack && !string.IsNullOrEmpty(_currentPage))
                {
                    _backStack.Push(_currentPage);
                    _forwardStack.Clear();
                }

                string oldPage = _currentPage;
                _currentPage = pagePath;

                Logger.Log($"导航从 {oldPage ?? "起始页"} 到 {pagePath}");
                PageChanged?.Invoke(null, new NavigationEventArgs(oldPage, pagePath));
            }
            catch (Exception ex)
            {
                Logger.LogError($"导航失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 返回上一页
        /// </summary>
        public static void GoBack()
        {
            try
            {
                if (!CanGoBack) return;

                _forwardStack.Push(_currentPage);
                string previousPage = _backStack.Pop();
                NavigateTo(previousPage, false);
                Logger.Log($"返回到页面: {previousPage}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"返回上一页失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 前进到下一页
        /// </summary>
        public static void GoForward()
        {
            try
            {
                if (!CanGoForward) return;

                _backStack.Push(_currentPage);
                string nextPage = _forwardStack.Pop();
                NavigateTo(nextPage, false);
                Logger.Log($"前进到页面: {nextPage}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"前进到下一页失败: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 工具方法
        /// <summary>
        /// 从文本中提取跳转目标
        /// </summary>
        /// <param name="text">包含跳转标记的文本</param>
        /// <returns>跳转目标数组</returns>
        public static string[] ExtractJumpTargets(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return Array.Empty<string>();
                }

                var targets = new List<string>();
                int startIndex = 0;

                while ((startIndex = text.IndexOf("[[", startIndex)) != -1)
                {
                    int endIndex = text.IndexOf("]]", startIndex);
                    if (endIndex == -1) break;

                    string jumpText = text.Substring(startIndex + 2, endIndex - startIndex - 2);
                    if (!string.IsNullOrWhiteSpace(jumpText))
                    {
                        targets.Add(jumpText);
                    }
                    startIndex = endIndex + 2;
                }

                return targets.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogError($"提取跳转目标失败: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// 清除导航历史
        /// </summary>
        public static void ClearHistory()
        {
            _backStack.Clear();
            _forwardStack.Clear();
            _currentPage = null;
            Logger.Log("导航历史已清除");
        }
        #endregion
    }

    /// <summary>
    /// 导航事件参数类
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        /// <summary>
        /// 原页面路径
        /// </summary>
        public string OldPage { get; }

        /// <summary>
        /// 新页面路径
        /// </summary>
        public string NewPage { get; }

        /// <summary>
        /// 初始化导航事件参数
        /// </summary>
        /// <param name="oldPage">原页面路径</param>
        /// <param name="newPage">新页面路径</param>
        public NavigationEventArgs(string oldPage, string newPage)
        {
            OldPage = oldPage;
            NewPage = newPage;
        }
    }
}