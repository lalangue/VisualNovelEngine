// EditWindow.xaml.cs
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Animation;
using System.Windows.Documents;
using VisualNovelEngine.Model.Page;
using VisualNovelEngine.Model.Project;
using VisualNovelEngine.Model.Group;
using VisualNovelEngine.Model.Logging;
using VisualNovelEngine.Model.Navigation;
using Microsoft.Extensions.Configuration.UserSecrets;
using VisualNovelEngine.Model.PathGetter;

namespace VisualNovelEngine.Viewer.EditWindow
{
    public partial class EditWindow : Window
    {
        private bool _isFullscreen;
        private bool _isDarkTheme = true;
        private bool _isNavigationPanelVisible = false;
        private Point _lastMousePosition;
        private bool _isWindowDragging = false;

        
        public EditWindow()
        {
            InitializeComponent();
            InitializeWindowChrome();

            

            StateChanged += Window_StateChanged;
            Loaded += Window_Loaded;
            PreviewMouseDown += Window_PreviewMouseDown;
            KeyDown += Window_KeyDown;
            MusicPlayer.MediaEnded += MusicPlayer_MediaEnded;
            PageData.PageChanged += OnPagePathChanged;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            CommandBindings.AddRange(new[]
            {
                new CommandBinding(_commands[0], ExecuteNewPage, CanExecutePageCommand),
                new CommandBinding(_commands[1], ExecuteDeletePage, CanExecutePageCommand),
                new CommandBinding(_commands[2], ExecuteNextPage, CanExecutePageCommand),
                new CommandBinding(_commands[3], ExecutePrevPage, CanExecutePageCommand),
                new CommandBinding(_commands[4], ExecuteVersionCommand, CanExecutePageCommand),
                new CommandBinding(_commands[5], ExecuteImageCommand, CanExecutePageCommand),
                new CommandBinding(_commands[6], ExecuteMusicCommand, CanExecutePageCommand),
            });

            InputBindings.Add(new InputBinding(
                new RoutedUICommand("ExitFullscreen", "ExitFullscreen", typeof(EditWindow)),
                new KeyGesture(Key.Escape)));
        }

        private void InitializeWindowChrome()
        {
            var chrome = new WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(4),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(this, chrome);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化主题
            ApplyTheme(_isDarkTheme);

            // 加载页面缩略图
            LoadPageThumbnails();
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                _lastMousePosition = e.GetPosition(this);
                _isWindowDragging = true;
                this.CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isWindowDragging)
            {
                Point currentPosition = e.GetPosition(this);
                double deltaX = currentPosition.X - _lastMousePosition.X;
                double deltaY = currentPosition.Y - _lastMousePosition.Y;

                this.Left += deltaX;
                this.Top += deltaY;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                _isWindowDragging = false;
                this.ReleaseMouseCapture();
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
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // 处理键盘快捷键
            if (e.Key == Key.Right || e.Key == Key.Space)
            {
                NextPageButton_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Left || e.Key == Key.Back)
            {
                PrevPageButton_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (_isNavigationPanelVisible)
                {
                    PageNavigationButton_Click(sender, e);
                    e.Handled = true;
                }
            }
        }

        private void UpdateWindowState()
        {
            _isFullscreen = WindowState == WindowState.Maximized;
            ResizeMode = _isFullscreen ? ResizeMode.NoResize : ResizeMode.CanResize;
        }

        private void ToggleFullscreen(bool enable)
        {
            WindowState = enable ? WindowState.Maximized : WindowState.Normal;
            _isFullscreen = enable;
        }

        private void MusicPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (MusicPlayer.Source != null)
            {
                MusicPlayer.Position = TimeSpan.Zero;
                MusicPlayer.Play();
            }
        }

        private void ApplyTheme(bool isDark)
        {
            _isDarkTheme = isDark;

            // 更新资源
            var resources = this.Resources;

            if (isDark)
            {
                resources["WindowBackgroundBrush"] = resources["DarkBackgroundBrush"];
                resources["TextBackgroundBrush"] = resources["DarkTextBackgroundBrush"];
                resources["ForegroundBrush"] = resources["DarkForegroundBrush"];
                resources["AccentBrush"] = resources["DarkAccentBrush"];
                ThemeIcon.Text = "☀"; // 太阳图标表示可以切换到亮色
            }
            else
            {
                resources["WindowBackgroundBrush"] = resources["LightBackgroundBrush"];
                resources["TextBackgroundBrush"] = resources["LightTextBackgroundBrush"];
                resources["ForegroundBrush"] = resources["LightForegroundBrush"];
                resources["AccentBrush"] = resources["LightAccentBrush"];
                ThemeIcon.Text = "☾"; // 月亮图标表示可以切换到暗色
            }
        }

        private void LoadPageThumbnails()
        {
            try
            {
                PageThumbnailList.Items.Clear();

                if (string.IsNullOrEmpty(ProjectData.ProjectPath) || string.IsNullOrEmpty(GroupData.GroupName))
                {
                    return;
                }

                string groupPath = PathGetter.GetGroupPath(GroupData.GroupName);
                if (!Directory.Exists(groupPath))
                {
                    return;
                }

                // 获取所有JSON文件
                string[] jsonFiles = Directory.GetFiles(groupPath, "*.json");
                foreach (string jsonFile in jsonFiles)
                {
                    try
                    {
                        var pageInfo = PageJsonInfo.LoadFromFile(jsonFile);
                        var pageName = System.IO.Path.GetFileNameWithoutExtension(jsonFile);

                        // 创建缩略图
                        BitmapImage thumbnail = null;
                        if (!string.IsNullOrEmpty(pageInfo.ImagePath))
                        {
                            string imagePath = pageInfo.GetAbsoluteImagePath();
                            if (File.Exists(imagePath))
                            {
                                thumbnail = new BitmapImage();
                                thumbnail.BeginInit();
                                thumbnail.UriSource = new Uri(imagePath);
                                thumbnail.DecodePixelWidth = 150; // 缩略图宽度
                                thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                                thumbnail.EndInit();
                                thumbnail.Freeze();
                            }
                        }

                        // 添加到列表
                        PageThumbnailList.Items.Add(new PageThumbnailItem
                        {
                            PageName = pageName,
                            ThumbnailImage = thumbnail,
                            PagePath = jsonFile
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"加载页面缩略图失败: {jsonFile}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("加载页面缩略图列表失败", ex);
            }
        }

        private void OnPagePathChanged(object sender, EventArgs e)
        {
            // 更新当前选中的页面
            if (PageThumbnailList.Items.Count > 0 && !string.IsNullOrEmpty(PageData.PagePath))
            {
                foreach (PageThumbnailItem item in PageThumbnailList.Items)
                {
                    if (item.PagePath == PageData.PagePath)
                    {
                        PageThumbnailList.SelectedItem = item;
                        break;
                    }
                }
            }

            // 加载页面内容
            LoadPageContent();
        }

        private void LoadPageContent()
        {
            try
            {
                // 淡入动画
                var fadeInStoryboard = (Storyboard)FindResource("FadeInStoryboard");
                fadeInStoryboard.Begin(DialogueText);

                // 加载背景图片
                if (!string.IsNullOrEmpty(PageData.PagePath))
                {
                    var pageInfo = PageJsonInfo.LoadFromFile(PageData.PagePath);

                    if (!string.IsNullOrEmpty(pageInfo.ImagePath))
                    {
                        string imagePath = pageInfo.GetAbsoluteImagePath();
                        if (File.Exists(imagePath))
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(imagePath);
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            BackgroundImage.Source = image;
                        }
                    }

                    // 加载文本
                    if (!string.IsNullOrEmpty(pageInfo.TextPath))
                    {
                        string textPath = pageInfo.GetAbsoluteTextPath();
                        if (File.Exists(textPath))
                        {
                            DialogueText.Document = new FlowDocument();
                            using (FileStream fileStream = new FileStream(textPath, FileMode.Open, FileAccess.Read))
                            {
                                var textRange = new TextRange(DialogueText.Document.ContentStart, DialogueText.Document.ContentEnd);
                                textRange.Load(fileStream, DataFormats.Rtf);
                            }

                            // 高亮显示跳转点
                            JumpManager.HighlightJumpPoints(DialogueText, pageInfo);
                        }
                    }

                    // 加载音乐
                    if (!string.IsNullOrEmpty(pageInfo.MusicPath))
                    {
                        string musicPath = pageInfo.GetAbsoluteMusicPath();
                        if (File.Exists(musicPath))
                        {
                            MusicPlayer.Source = new Uri(musicPath);
                            MusicPlayer.Play();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("加载页面内容失败", ex);
            }
        }

        #region 导航和主题

        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme(!_isDarkTheme);
        }

        private void PageNavigationButton_Click(object sender, RoutedEventArgs e)
        {
            _isNavigationPanelVisible = !_isNavigationPanelVisible;

            if (_isNavigationPanelVisible)
            {
                PageNavigationPanel.Visibility = Visibility.Visible;

                // 淡入动画
                var fadeInStoryboard = new Storyboard();
                var animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
                Storyboard.SetTarget(animation, PageNavigationPanel);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                fadeInStoryboard.Children.Add(animation);
                fadeInStoryboard.Begin();
            }
            else
            {
                // 淡出动画
                var fadeOutStoryboard = new Storyboard();
                var animation = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
                Storyboard.SetTarget(animation, PageNavigationPanel);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                fadeOutStoryboard.Children.Add(animation);
                fadeOutStoryboard.Completed += (s, args) => PageNavigationPanel.Visibility = Visibility.Collapsed;
                fadeOutStoryboard.Begin();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(PageData.PagePath))
                {
                    return;
                }

                var pageInfo = PageJsonInfo.LoadFromFile(PageData.PagePath);

                if (!string.IsNullOrEmpty(pageInfo.NextPagePath))
                {
                    string nextPagePath = System.IO.Path.Combine(ProjectData.ProjectPath, GroupData.GroupName, $"{pageInfo.NextPagePath}.json");
                    if (File.Exists(nextPagePath))
                    {
                        PageData.PagePath = nextPagePath;
                        NavigationManager.NavigateTo(nextPagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("导航到下一页失败", ex);
            }
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(PageData.PagePath))
                {
                    return;
                }

                var pageInfo = PageJsonInfo.LoadFromFile(PageData.PagePath);

                if (!string.IsNullOrEmpty(pageInfo.PreviousPagePath))
                {
                    string prevPagePath = System.IO.Path.Combine(ProjectData.ProjectPath, GroupData.GroupName, $"{pageInfo.PreviousPagePath}.json");
                    if (File.Exists(prevPagePath))
                    {
                        PageData.PagePath = prevPagePath;
                        NavigationManager.NavigateTo(prevPagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("导航到上一页失败", ex);
            }
        }

        #endregion


        private void Window_StateChanged(object sender, EventArgs e)
        {
            UpdateWindowState();
        }

    }

    // 页面缩略图项
    public class PageThumbnailItem
    {
        public string PageName { get; set; }
        public BitmapImage ThumbnailImage { get; set; }
        public string PagePath { get; set; }
    }
}
