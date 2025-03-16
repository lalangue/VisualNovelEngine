using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VisualNovelEngine.Model.Logging;

namespace VisualNovelEngine.Model.Input
{
    /// <summary>
    /// 键盘管理器，负责处理全局键盘快捷键
    /// </summary>
    public static class KeyboardManager
    {
        // 快捷键映射
        private static Dictionary<KeyGesture, KeyboardAction> _keyboardShortcuts = new Dictionary<KeyGesture, KeyboardAction>();

        // 快捷键命令
        private static Dictionary<string, RoutedCommand> _commands = new Dictionary<string, RoutedCommand>();

        // 是否启用键盘导航
        private static bool _isKeyboardNavigationEnabled = true;

        /// <summary>
        /// 初始化键盘管理器
        /// </summary>
        public static void Initialize()
        {
            // 注册默认快捷键
            RegisterDefaultShortcuts();

            Logger.Log("键盘管理器已初始化");
        }

        /// <summary>
        /// 注册默认快捷键
        /// </summary>
        private static void RegisterDefaultShortcuts()
        {
            // 导航快捷键
            RegisterShortcut(new KeyGesture(Key.Right), KeyboardAction.NextPage, "下一页");
            RegisterShortcut(new KeyGesture(Key.Left), KeyboardAction.PreviousPage, "上一页");
            RegisterShortcut(new KeyGesture(Key.Home), KeyboardAction.FirstPage, "第一页");
            RegisterShortcut(new KeyGesture(Key.End), KeyboardAction.LastPage, "最后一页");

            // 编辑快捷键
            RegisterShortcut(new KeyGesture(Key.S, ModifierKeys.Control), KeyboardAction.Save, "保存");
            RegisterShortcut(new KeyGesture(Key.N, ModifierKeys.Control), KeyboardAction.NewPage, "新建页面");
            RegisterShortcut(new KeyGesture(Key.O, ModifierKeys.Control), KeyboardAction.OpenPage, "打开页面");
            RegisterShortcut(new KeyGesture(Key.Delete, ModifierKeys.Control), KeyboardAction.DeletePage, "删除页面");

            // 视图快捷键
            RegisterShortcut(new KeyGesture(Key.F11), KeyboardAction.ToggleFullscreen, "切换全屏");
            RegisterShortcut(new KeyGesture(Key.F5), KeyboardAction.Refresh, "刷新");
            RegisterShortcut(new KeyGesture(Key.T, ModifierKeys.Control), KeyboardAction.ToggleTheme, "切换主题");

            // 控制台快捷键
            RegisterShortcut(new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift), KeyboardAction.OpenConsole, "打开控制台");

            // 跳转快捷键
            RegisterShortcut(new KeyGesture(Key.J, ModifierKeys.Control), KeyboardAction.JumpToPage, "跳转到页面");

            // 自动保存快捷键
            RegisterShortcut(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift), KeyboardAction.ToggleAutoSave, "切换自动保存");

            // 帮助快捷键
            RegisterShortcut(new KeyGesture(Key.F1), KeyboardAction.ShowHelp, "显示帮助");

            Logger.Log($"已注册 {_keyboardShortcuts.Count} 个默认快捷键");
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="gesture">按键组合</param>
        /// <param name="action">对应的操作</param>
        /// <param name="description">描述</param>
        public static void RegisterShortcut(KeyGesture gesture, KeyboardAction action, string description)
        {
            _keyboardShortcuts[gesture] = action;

            // 创建命令
            string commandName = action.ToString();
            if (!_commands.ContainsKey(commandName))
            {
                var command = new RoutedCommand(commandName, typeof(KeyboardManager));
                command.InputGestures.Add(gesture);
                _commands[commandName] = command;
            }

            Logger.Log($"已注册快捷键: {gesture} => {description}");
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        /// <param name="gesture">按键组合</param>
        public static void UnregisterShortcut(KeyGesture gesture)
        {
            if (_keyboardShortcuts.ContainsKey(gesture))
            {
                KeyboardAction action = _keyboardShortcuts[gesture];
                _keyboardShortcuts.Remove(gesture);

                // 从命令中移除手势
                string commandName = action.ToString();
                if (_commands.ContainsKey(commandName))
                {
                    var command = _commands[commandName];
                    var gestureToRemove = command.InputGestures.OfType<KeyGesture>()
                        .FirstOrDefault(g => g.Key == gesture.Key && g.Modifiers == gesture.Modifiers);

                    if (gestureToRemove != null)
                    {
                        command.InputGestures.Remove(gestureToRemove);
                    }
                }

                Logger.Log($"已注销快捷键: {gesture}");
            }
        }

        /// <summary>
        /// 处理键盘事件
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        /// <returns>是否已处理</returns>
        public static bool HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (!_isKeyboardNavigationEnabled)
            {
                return false;
            }

            // 查找匹配的快捷键
            var matchedGesture = _keyboardShortcuts.Keys.FirstOrDefault(g =>
                g.Key == e.Key && g.Modifiers == Keyboard.Modifiers);

            if (matchedGesture != null)
            {
                KeyboardAction action = _keyboardShortcuts[matchedGesture];

                // 触发操作
                OnKeyboardAction?.Invoke(null, new KeyboardActionEventArgs(action));

                Logger.Log($"触发键盘操作: {action}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取命令
        /// </summary>
        /// <param name="action">键盘操作</param>
        /// <returns>对应的命令</returns>
        public static RoutedCommand GetCommand(KeyboardAction action)
        {
            string commandName = action.ToString();
            return _commands.ContainsKey(commandName) ? _commands[commandName] : null;
        }

        /// <summary>
        /// 获取所有快捷键
        /// </summary>
        /// <returns>快捷键列表</returns>
        public static List<KeyboardShortcut> GetAllShortcuts()
        {
            return _keyboardShortcuts.Select(kv => new KeyboardShortcut
            {
                Gesture = kv.Key,
                Action = kv.Value,
                Description = GetActionDescription(kv.Value)
            }).ToList();
        }

        /// <summary>
        /// 获取操作描述
        /// </summary>
        /// <param name="action">键盘操作</param>
        /// <returns>描述</returns>
        private static string GetActionDescription(KeyboardAction action)
        {
            switch (action)
            {
                case KeyboardAction.NextPage: return "下一页";
                case KeyboardAction.PreviousPage: return "上一页";
                case KeyboardAction.FirstPage: return "第一页";
                case KeyboardAction.LastPage: return "最后一页";
                case KeyboardAction.Save: return "保存";
                case KeyboardAction.NewPage: return "新建页面";
                case KeyboardAction.OpenPage: return "打开页面";
                case KeyboardAction.DeletePage: return "删除页面";
                case KeyboardAction.ToggleFullscreen: return "切换全屏";
                case KeyboardAction.Refresh: return "刷新";
                case KeyboardAction.ToggleTheme: return "切换主题";
                case KeyboardAction.OpenConsole: return "打开控制台";
                case KeyboardAction.JumpToPage: return "跳转到页面";
                case KeyboardAction.ToggleAutoSave: return "切换自动保存";
                case KeyboardAction.ShowHelp: return "显示帮助";
                default: return action.ToString();
            }
        }

        /// <summary>
        /// 启用或禁用键盘导航
        /// </summary>
        /// <param name="enable">是否启用</param>
        public static void EnableKeyboardNavigation(bool enable)
        {
            _isKeyboardNavigationEnabled = enable;
            Logger.Log($"键盘导航已{(_isKeyboardNavigationEnabled ? "启用" : "禁用")}");
        }

        /// <summary>
        /// 键盘操作事件
        /// </summary>
        public static event EventHandler<KeyboardActionEventArgs> OnKeyboardAction;
    }

    /// <summary>
    /// 键盘操作
    /// </summary>
    public enum KeyboardAction
    {
        // 导航操作
        NextPage,
        PreviousPage,
        FirstPage,
        LastPage,

        // 编辑操作
        Save,
        NewPage,
        OpenPage,
        DeletePage,

        // 视图操作
        ToggleFullscreen,
        Refresh,
        ToggleTheme,

        // 控制台操作
        OpenConsole,

        // 跳转操作
        JumpToPage,

        // 自动保存操作
        ToggleAutoSave,

        // 帮助操作
        ShowHelp
    }

    /// <summary>
    /// 键盘操作事件参数
    /// </summary>
    public class KeyboardActionEventArgs : EventArgs
    {
        /// <summary>
        /// 键盘操作
        /// </summary>
        public KeyboardAction Action { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action">键盘操作</param>
        public KeyboardActionEventArgs(KeyboardAction action)
        {
            Action = action;
        }
    }

    /// <summary>
    /// 键盘快捷键
    /// </summary>
    public class KeyboardShortcut
    {
        /// <summary>
        /// 按键组合
        /// </summary>
        public KeyGesture Gesture { get; set; }

        /// <summary>
        /// 对应的操作
        /// </summary>
        public KeyboardAction Action { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 显示文本
        /// </summary>
        public string DisplayText => $"{Gesture} - {Description}";
    }
}