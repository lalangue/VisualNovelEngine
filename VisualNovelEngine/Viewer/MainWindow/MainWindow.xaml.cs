using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Windows.Media.Animation;
using VisualNovelEngine.Model;
using VisualNovelEngine.Model.Project;
using VisualNovelEngine.Model.Group;
using VisualNovelEngine.Model.Cache;
using VisualNovelEngine.Model.Navigation;
using VisualNovelEngine.Model.Theme;
using VisualNovelEngine.Model.Logging;
using VisualNovelEngine.Model.Page;
using VisualNovelEngine.Viewer.ConsoleWindow;

namespace VisualNovelEngine.Viewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private PageJsonInfo _currentPage;
        private ConsoleWindow.ConsoleWindow _consoleWindow;
        private bool _isDarkTheme = true;

        public MainWindow()
        {
            InitializeComponent();
            InitializeComponents();

            // 播放淡入动画
            Storyboard fadeIn = (Storyboard)FindResource("FadeIn");
            fadeIn.Begin(this);
        }

        private void InitializeComponents()
        {
            // 初始化主题
            ThemeManager.Initialize();
            ThemeManager.ThemeChanged += (s, e) => UpdateThemeButton();

            // 设置版本号
            VersionText.Text = $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

            // 初始化导航事件
            NavigationManager.PageChanged += OnPageChanged;

            // 禁用导航按钮
            UpdateNavigationButtons();

            // 初始化富文本框
            TextDisplay.IsReadOnly = true;
            TextDisplay.TextChanged += TextDisplay_TextChanged;
        }

        private void UpdateThemeButton()
        {
            _isDarkTheme = ThemeManager.IsDarkTheme;
            ThemeToggle.Content = _isDarkTheme ? "🌙" : "☀️";
            ThemeToggle.ToolTip = _isDarkTheme ? "切换到亮色主题" : "切换到暗色主题";
        }

        private void UpdateNavigationButtons()
        {
            BackButton.IsEnabled = NavigationManager.CanGoBack;
            ForwardButton.IsEnabled = NavigationManager.CanGoForward;
        }

        private void OnPageChanged(object sender, Model.Navigation.NavigationEventArgs e)
        {
            LoadPage(e.NewPage);
            UpdateNavigationButtons();
        }

        private void LoadPage(string pagePath)
        {
            try
            {
                _currentPage = PageJsonInfo.LoadFromFile(pagePath);

                // 预加载页面资源
                ResourceCache.PreloadPage(_currentPage);

                // 加载背景图片
                if (!string.IsNullOrEmpty(_currentPage.ImagePath))
                {
                    BackgroundImage.Source = ResourceCache.GetCachedImage(_currentPage.GetAbsoluteImagePath());
                }

                // 加载文本
                if (!string.IsNullOrEmpty(_currentPage.TextPath))
                {
                    var document = ResourceCache.GetCachedText(_currentPage.GetAbsoluteTextPath());
                    TextDisplay.Document = document;

                    // 高亮显示跳转点
                    JumpManager.HighlightJumpPoints(TextDisplay, _currentPage);
                }

                // 更新状态
                StatusText.Text = $"当前页面：{System.IO.Path.GetFileName(pagePath)}";
                Logger.Log($"加载页面：{pagePath}");
            }
            catch (Exception ex)
            {
                Logger.LogError("加载页面失败", ex);
                System.Windows.MessageBox.Show($"加载页面失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextDisplay_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 检查跳转标记
            var textRange = new TextRange(TextDisplay.Document.ContentStart, TextDisplay.Document.ContentEnd);
            var jumpTargets = NavigationManager.ExtractJumpTargets(textRange.Text);

            // 高亮显示跳转点
            JumpManager.HighlightJumpPoints(TextDisplay, _currentPage);
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "选择项目位置";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string projectPath = dialog.SelectedPath;
                    ProjectData.ProjectPath = projectPath;

                    // 创建必要的目录结构
                    Directory.CreateDirectory(System.IO.Path.Combine(projectPath, "content"));

                    Logger.Initialize(projectPath);
                    Logger.Log($"创建新项目：{projectPath}");

                    StatusText.Text = $"已创建新项目：{System.IO.Path.GetFileName(projectPath)}";
                }
                catch (Exception ex)
                {
                    Logger.LogError("创建项目失败", ex);
                    System.Windows.MessageBox.Show($"创建项目失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "选择项目文件夹";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string projectPath = dialog.SelectedPath;
                    ProjectData.ProjectPath = projectPath;

                    Logger.Initialize(projectPath);
                    Logger.Log($"打开项目：{projectPath}");

                    // 清除缓存和导航历史
                    ResourceCache.ClearCache();
                    NavigationManager.ClearHistory();

                    StatusText.Text = $"已打开项目：{System.IO.Path.GetFileName(projectPath)}";
                }
                catch (Exception ex)
                {
                    Logger.LogError("打开项目失败", ex);
                    System.Windows.MessageBox.Show($"打开项目失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.GoBack();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.GoForward();
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // 更新主题图标
            ThemeToggle.Content = _isDarkTheme ? "🌙" : "☀️";
            ThemeToggle.ToolTip = _isDarkTheme ? "切换到亮色主题" : "切换到暗色主题";

            // 应用主题资源
            var resources = System.Windows.Application.Current.Resources;

            if (_isDarkTheme)
            {
                // 应用深色主题
                resources["WindowBackgroundBrush"] = resources["DarkBackgroundBrush"];
                resources["TextBackgroundBrush"] = resources["DarkTextBackgroundBrush"];
                resources["ForegroundBrush"] = resources["DarkForegroundBrush"];
                resources["AccentBrush"] = resources["DarkAccentBrush"];

                // 调用主题管理器切换到深色主题
                ThemeManager.SwitchToDarkTheme();
            }
            else
            {
                // 应用浅色主题
                resources["WindowBackgroundBrush"] = resources["LightBackgroundBrush"];
                resources["TextBackgroundBrush"] = resources["LightTextBackgroundBrush"];
                resources["ForegroundBrush"] = resources["LightForegroundBrush"];
                resources["AccentBrush"] = resources["LightAccentBrush"];

                // 调用主题管理器切换到浅色主题
                ThemeManager.SwitchToLightTheme();
            }

            // 记录主题切换
            Logger.Log($"已切换到{(_isDarkTheme ? "深色" : "浅色")}主题");
        }

        private void OpenConsole_Click(object sender, RoutedEventArgs e)
        {
            if (_consoleWindow == null || !_consoleWindow.IsVisible)
            {
                _consoleWindow = new ConsoleWindow.ConsoleWindow();
                _consoleWindow.Owner = this;
                _consoleWindow.Show();
            }
            else
            {
                _consoleWindow.Activate();
            }
        }

        #region 窗口控制

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaximizeButton.ToolTip = "最大化";
                ((TextBlock)MaximizeButton.Content).Text = "□";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaximizeButton.ToolTip = "还原";
                ((TextBlock)MaximizeButton.Content).Text = "❐";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
