using System;
using System.Windows;
using System.Windows.Media;

namespace VisualNovelEngine.Model.Theme
{
    public static class ThemeManager
    {
        public enum ThemeType
        {
            Light,
            Dark
        }

        private static ThemeType _currentTheme = ThemeType.Dark; // 默认使用深色主题
        public static event EventHandler<ThemeType> ThemeChanged;

        public static ThemeType CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ApplyTheme();
                    ThemeChanged?.Invoke(null, value);
                }
            }
        }

        /// <summary>
        /// 获取当前是否为深色主题
        /// </summary>
        public static bool IsDarkTheme => _currentTheme == ThemeType.Dark;

        // 应用主题
        private static void ApplyTheme()
        {
            var resources = Application.Current.Resources;

            if (_currentTheme == ThemeType.Dark)
            {
                resources["BackgroundColor"] = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                resources["ForegroundColor"] = new SolidColorBrush(Colors.White);
                resources["AccentColor"] = new SolidColorBrush(Color.FromRgb(0, 122, 204));
                resources["SecondaryBackgroundColor"] = new SolidColorBrush(Color.FromRgb(45, 45, 48));
                resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(61, 61, 61));
            }
            else
            {
                resources["BackgroundColor"] = new SolidColorBrush(Colors.White);
                resources["ForegroundColor"] = new SolidColorBrush(Colors.Black);
                resources["AccentColor"] = new SolidColorBrush(Color.FromRgb(0, 120, 215));
                resources["SecondaryBackgroundColor"] = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            }
        }

        /// <summary>
        /// 切换主题
        /// </summary>
        public static void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == ThemeType.Light ? ThemeType.Dark : ThemeType.Light;
        }

        /// <summary>
        /// 切换到深色主题
        /// </summary>
        public static void SwitchToDarkTheme()
        {
            CurrentTheme = ThemeType.Dark;
        }

        /// <summary>
        /// 切换到浅色主题
        /// </summary>
        public static void SwitchToLightTheme()
        {
            CurrentTheme = ThemeType.Light;
        }

        /// <summary>
        /// 初始化主题
        /// </summary>
        public static void Initialize()
        {
            ApplyTheme();
        }
    }
}