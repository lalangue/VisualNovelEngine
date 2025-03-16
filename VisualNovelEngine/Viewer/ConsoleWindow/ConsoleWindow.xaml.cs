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
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using IOPath = System.IO.Path;  // 添加别名
using VisualNovelEngine.Model.Page;
using VisualNovelEngine.Model.Project;
using VisualNovelEngine.Model.Group;
using VisualNovelEngine.Model.Page.PageEdit;
using VisualNovelEngine.Model.Logging;
using VisualNovelEngine.Model.Navigation;
using VisualNovelEngine.Model.Cache;

namespace VisualNovelEngine.Viewer.ConsoleWindow
{
    /// <summary>
    /// ConsoleWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        private PageJsonInfo _currentPageInfo;
        private bool _isUpdatingSelection = false;
        private bool _isTextChangedByUser = true;

        public ConsoleWindow()
        {
            InitializeComponent();
            InitializeComponents();
            PageData.PageChanged += OnPagePathChanged;
            LoadPageList();
        }

        private void InitializeComponents()
        {
            // 初始化字体列表
            foreach (var fontFamily in Fonts.SystemFontFamilies.OrderBy(f => f.Source))
            {
                FontFamilyComboBox.Items.Add(fontFamily.Source);
            }
            FontFamilyComboBox.SelectedIndex = 0;

            // 初始化字体大小
            FontSizeComboBox.SelectedIndex = 4; // 默认12号字体

            // 初始化按钮状态
            UpdateButtonStates();

            // 禁用删除按钮
            DeletePageButton.IsEnabled = false;
        }

        private void LoadPageList()
        {
            try
            {
                PageListBox.Items.Clear();

                if (string.IsNullOrEmpty(ProjectData.ProjectPath) || string.IsNullOrEmpty(GroupData.GroupName))
                {
                    return;
                }

                string groupPath = IOPath.Combine(ProjectData.ProjectPath, GroupData.GroupName);
                if (!Directory.Exists(groupPath))
                {
                    return;
                }

                // 获取所有JSON文件
                string[] jsonFiles = Directory.GetFiles(groupPath, "*.json");
                foreach (var jsonFile in jsonFiles)
                {
                    PageListBox.Items.Add(IOPath.GetFileNameWithoutExtension(jsonFile));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("加载页面列表失败", ex);
                System.Windows.MessageBox.Show($"加载页面列表失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPagePathChanged(object sender, EventArgs e)
        {
            try
            {
                PageJsonInfo pageJsonInfo = new PageJsonInfo();
                if (!string.IsNullOrEmpty(pageJsonInfo.TextPath))
                {
                    using (FileStream fs = new FileStream(pageJsonInfo.TextPath, FileMode.Open))
                    {
                        TextRange range = new TextRange(RichTextEditor.Document.ContentStart,
                                                      RichTextEditor.Document.ContentEnd);
                        range.Load(fs, System.Windows.DataFormats.Rtf);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("页面路径变更处理失败", ex);
            }
        }

        

        private void LoadCurrentPage()
        {
            try
            {
                if (string.IsNullOrEmpty(PageData.PagePath))
                {
                    RichTextEditor.Document = new FlowDocument();
                    _currentPageInfo = null;
                    return;
                }

                _currentPageInfo = PageJsonInfo.LoadFromFile(PageData.PagePath);

                if (!string.IsNullOrEmpty(_currentPageInfo.TextPath))
                {
                    string textPath = _currentPageInfo.GetAbsoluteTextPath();
                    if (File.Exists(textPath))
                    {
                        _isTextChangedByUser = false;
                        RichTextEditor.Document = new FlowDocument();
                        using (FileStream fileStream = new FileStream(textPath, FileMode.Open, FileAccess.Read))
                        {
                            var textRange = new TextRange(RichTextEditor.Document.ContentStart, RichTextEditor.Document.ContentEnd);
                            textRange.Load(fileStream, System.Windows.DataFormats.Rtf);
                        }

                        // 高亮显示跳转点
                        JumpManager.HighlightJumpPoints(RichTextEditor, _currentPageInfo);

                        _isTextChangedByUser = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("加载页面失败", ex);
                System.Windows.MessageBox.Show($"加载页面失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSelectedPageInList()
        {
            if (string.IsNullOrEmpty(PageData.PagePath))
            {
                PageListBox.SelectedIndex = -1;
                return;
            }

            string pageName = IOPath.GetFileNameWithoutExtension(PageData.PagePath);
            for (int i = 0; i < PageListBox.Items.Count; i++)
            {
                if (PageListBox.Items[i].ToString() == pageName)
                {
                    PageListBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void SaveCurrentPage()
        {
            try
            {
                if (_currentPageInfo == null || string.IsNullOrEmpty(PageData.PagePath))
                {
                    return;
                }

                // 保存富文本内容
                if (!string.IsNullOrEmpty(_currentPageInfo.TextPath))
                {
                    string textPath = _currentPageInfo.GetAbsoluteTextPath();
                    Directory.CreateDirectory(IOPath.GetDirectoryName(textPath));

                    using (FileStream fileStream = new FileStream(textPath, FileMode.Create, FileAccess.Write))
                    {
                        var textRange = new TextRange(RichTextEditor.Document.ContentStart, RichTextEditor.Document.ContentEnd);
                        textRange.Save(fileStream, System.Windows.DataFormats.Rtf);
                    }
                }

                // 保存JSON文件
                _currentPageInfo.SaveToFile(PageData.PagePath);
                Logger.Log($"保存页面：{PageData.PagePath}");
            }
            catch (Exception ex)
            {
                Logger.LogError("保存页面失败", ex);
                System.Windows.MessageBox.Show($"保存页面失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButtonStates()
        {
            _isUpdatingSelection = true;

            // 获取当前选择的文本格式
            TextSelection selection = RichTextEditor.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                object fontWeight = selection.GetPropertyValue(TextElement.FontWeightProperty);
                BoldButton.IsChecked = (fontWeight != DependencyProperty.UnsetValue) && (fontWeight.Equals(FontWeights.Bold));

                object fontStyle = selection.GetPropertyValue(TextElement.FontStyleProperty);
                ItalicButton.IsChecked = (fontStyle != DependencyProperty.UnsetValue) && (fontStyle.Equals(FontStyles.Italic));

                object textDecorations = selection.GetPropertyValue(Inline.TextDecorationsProperty);
                UnderlineButton.IsChecked = (textDecorations != DependencyProperty.UnsetValue) &&
                                           (textDecorations.Equals(TextDecorations.Underline));

                object foreground = selection.GetPropertyValue(TextElement.ForegroundProperty);
                if (foreground != DependencyProperty.UnsetValue && foreground is SolidColorBrush brush)
                {
                    TextColorRectangle.Fill = brush;
                }

                object background = selection.GetPropertyValue(TextElement.BackgroundProperty);
                if (background != DependencyProperty.UnsetValue && background is SolidColorBrush bgBrush)
                {
                    HighlightColorRectangle.Fill = bgBrush;
                }

                object fontFamily = selection.GetPropertyValue(TextElement.FontFamilyProperty);
                if (fontFamily != DependencyProperty.UnsetValue)
                {
                    FontFamilyComboBox.SelectedItem = ((FontFamily)fontFamily).Source;
                }

                object fontSize = selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (fontSize != DependencyProperty.UnsetValue)
                {
                    string fontSizeStr = ((double)fontSize).ToString();
                    for (int i = 0; i < FontSizeComboBox.Items.Count; i++)
                    {
                        if (((ComboBoxItem)FontSizeComboBox.Items[i]).Content.ToString() == fontSizeStr)
                        {
                            FontSizeComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }

            _isUpdatingSelection = false;
        }

        #region 富文本编辑事件处理

        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelection || FontFamilyComboBox.SelectedItem == null) return;

            string fontFamilyName = FontFamilyComboBox.SelectedItem.ToString();
            RichTextEditor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(fontFamilyName));
            RichTextEditor.Focus();
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelection || FontSizeComboBox.SelectedItem == null) return;

            string fontSize = ((ComboBoxItem)FontSizeComboBox.SelectedItem).Content.ToString();
            RichTextEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
            RichTextEditor.Focus();
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            RichTextEditor.Selection.ApplyPropertyValue(
                TextElement.FontWeightProperty,
                BoldButton.IsChecked == true ? FontWeights.Bold : FontWeights.Normal);
            RichTextEditor.Focus();
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            RichTextEditor.Selection.ApplyPropertyValue(
                TextElement.FontStyleProperty,
                ItalicButton.IsChecked == true ? FontStyles.Italic : FontStyles.Normal);
            RichTextEditor.Focus();
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            RichTextEditor.Selection.ApplyPropertyValue(
                Inline.TextDecorationsProperty,
                UnderlineButton.IsChecked == true ? TextDecorations.Underline : null);
            RichTextEditor.Focus();
        }

        private void TextColorButton_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            if (TextColorRectangle.Fill is SolidColorBrush brush)
            {
                colorDialog.Color = System.Drawing.Color.FromArgb(
                    brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
            }

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color color = Color.FromArgb(
                    colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                SolidColorBrush newBrush = new SolidColorBrush(color);
                TextColorRectangle.Fill = newBrush;
                RichTextEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, newBrush);
                RichTextEditor.Focus();
            }
        }

        private void HighlightColorButton_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            if (HighlightColorRectangle.Fill is SolidColorBrush brush)
            {
                colorDialog.Color = System.Drawing.Color.FromArgb(
                    brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
            }

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color color = Color.FromArgb(
                    colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                SolidColorBrush newBrush = new SolidColorBrush(color);
                HighlightColorRectangle.Fill = newBrush;
                RichTextEditor.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, newBrush);
                RichTextEditor.Focus();
            }
        }

        private void InsertJumpButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开页面选择对话框
            var dialog = new System.Windows.Controls.ListBox();
            dialog.ItemsSource = PageListBox.Items;

            var window = new Window
            {
                Title = "选择跳转目标",
                Content = dialog,
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            if (window.ShowDialog() == true && dialog.SelectedItem != null)
            {
                string targetPage = dialog.SelectedItem.ToString();
                RichTextEditor.Selection.Text = $"[[{targetPage}]]";
            }

            RichTextEditor.Focus();
        }

        private void RichTextEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateButtonStates();
        }

        private void RichTextEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isTextChangedByUser)
            {
                // 保存页面
                SaveCurrentPage();

                // 更新跳转点列表
                UpdateJumpPoints();
            }
        }

        // 更新跳转点列表
        private void UpdateJumpPoints()
        {
            if (_currentPageInfo == null) return;

            // 获取文本内容
            var textRange = new TextRange(RichTextEditor.Document.ContentStart, RichTextEditor.Document.ContentEnd);
            string text = textRange.Text;

            // 查找所有跳转点
            var jumpPoints = JumpManager.FindJumpPoints(text);

            // 移除不再存在的跳转点
            _currentPageInfo.JumpPoints.RemoveAll(jp => !jumpPoints.Contains(jp.Text));

            // 高亮显示跳转点
            JumpManager.HighlightJumpPoints(RichTextEditor, _currentPageInfo);
        }

        #endregion

        #region 页面列表事件处理

        private void PageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageListBox.SelectedItem == null)
            {
                DeletePageButton.IsEnabled = false;
                return;
            }

            DeletePageButton.IsEnabled = true;
            string pageName = PageListBox.SelectedItem.ToString();
            string pagePath = IOPath.Combine(ProjectData.ProjectPath, GroupData.GroupName, $"{pageName}.json");

            if (File.Exists(pagePath) && pagePath != PageData.PagePath)
            {
                SaveCurrentPage();
                PageData.PagePath = pagePath;
                NavigationManager.NavigateTo(pagePath);
            }
        }

        private void NewPageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取新页面名称
                string pageName = $"page_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

                // 创建新的PageJsonInfo
                var newPage = new PageJsonInfo
                {
                    TextPath = $"{pageName}.rtf",
                    ImagePath = "",
                    MusicPath = "",
                    PreviousPagePath = _currentPageInfo?.PreviousPagePath ?? "",
                    NextPagePath = "",
                    JumpToPagePaths = new string[0]
                };

                // 保存JSON文件
                string pagePath = IOPath.Combine(ProjectData.ProjectPath, GroupData.GroupName, $"{pageName}.json");
                newPage.SaveToFile(pagePath);

                // 创建空的RTF文件
                string textPath = newPage.GetAbsoluteTextPath();
                Directory.CreateDirectory(IOPath.GetDirectoryName(textPath));
                using (FileStream fileStream = new(textPath, FileMode.Create, FileAccess.Write))
                {
                    var document = new FlowDocument();
                    var textRange = new TextRange(document.ContentStart, document.ContentEnd);
                    textRange.Save(fileStream, System.Windows.DataFormats.Rtf);
                }

                // 更新页面列表
                LoadPageList();

                // 导航到新页面
                PageData.PagePath = pagePath;
                NavigationManager.NavigateTo(pagePath);

                Logger.Log($"创建新页面：{pagePath}");
            }
            catch (Exception ex)
            {
                Logger.LogError("创建新页面失败", ex);
                System.Windows.MessageBox.Show($"创建新页面失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeletePageButton_Click(object sender, RoutedEventArgs e)
        {
            if (PageListBox.SelectedItem == null) return;

            string pageName = PageListBox.SelectedItem.ToString();
            string pagePath = IOPath.Combine(ProjectData.ProjectPath, GroupData.GroupName, $"{pageName}.json");

            if (System.Windows.MessageBox.Show($"确定要删除页面 {pageName} 吗？", "确认删除",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    // 加载页面信息
                    var pageInfo = PageJsonInfo.LoadFromFile(pagePath);

                    // 删除相关文件
                    if (!string.IsNullOrEmpty(pageInfo.TextPath))
                    {
                        string textPath = pageInfo.GetAbsoluteTextPath();
                        if (File.Exists(textPath))
                        {
                            File.Delete(textPath);
                        }
                    }

                    // 删除JSON文件
                    File.Delete(pagePath);

                    // 更新页面列表
                    LoadPageList();

                    // 如果删除的是当前页面，则清空编辑器
                    if (pagePath == PageData.PagePath)
                    {
                        PageData.PagePath = "";
                        RichTextEditor.Document = new FlowDocument();
                        _currentPageInfo = null;
                    }

                    Logger.Log($"删除页面：{pagePath}");
                }
                catch (Exception ex)
                {
                    Logger.LogError("删除页面失败", ex);
                    System.Windows.MessageBox.Show($"删除页面失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            SaveCurrentPage();
            PageData.PageChanged -= OnPagePathChanged;
            base.OnClosed(e);
        }
    }
}
