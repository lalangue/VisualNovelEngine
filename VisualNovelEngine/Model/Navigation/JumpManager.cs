using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using VisualNovelEngine.Model.Page;
using VisualNovelEngine.Model.Project;
using VisualNovelEngine.Model.Group;
using VisualNovelEngine.Model.Logging;

namespace VisualNovelEngine.Model.Navigation
{
    /// <summary>
    /// 跳转点管理器
    /// 负责处理文本中的跳转点标记、高亮显示和交互
    /// </summary>
    public static class JumpManager
    {
        #region 常量
        /// <summary>
        /// 跳转点正则表达式模式
        /// 匹配[[文本]]格式的跳转标记
        /// </summary>
        private static readonly Regex JumpPointRegex = new Regex(@"\[\[(.*?)\]\]", RegexOptions.Compiled);

        /// <summary>
        /// 有效跳转点的颜色（蓝色）
        /// </summary>
        private static readonly Color ValidJumpPointColor = Color.FromRgb(66, 133, 244);

        /// <summary>
        /// 无效跳转点的颜色（灰色）
        /// </summary>
        private static readonly Color InvalidJumpPointColor = Color.FromRgb(128, 128, 128);

        /// <summary>
        /// 鼠标悬停时的颜色（深蓝色）
        /// </summary>
        private static readonly Color HoverJumpPointColor = Color.FromRgb(25, 103, 210);
        #endregion

        #region 公共方法
        /// <summary>
        /// 查找文本中的所有跳转点
        /// </summary>
        /// <param name="text">要搜索的文本</param>
        /// <returns>跳转点文本列表</returns>
        public static List<string> FindJumpPoints(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return new List<string>();
                }

                return JumpPointRegex.Matches(text)
                    .Cast<Match>()
                    .Where(m => m.Groups.Count > 1)
                    .Select(m => m.Groups[1].Value)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError($"查找跳转点时出错: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// 在RichTextBox中高亮显示跳转点
        /// </summary>
        /// <param name="richTextBox">目标富文本框</param>
        /// <param name="pageInfo">页面信息</param>
        public static void HighlightJumpPoints(RichTextBox richTextBox, PageJsonInfo pageInfo)
        {
            try
            {
                if (richTextBox == null || pageInfo == null)
                {
                    throw new ArgumentNullException(richTextBox == null ? nameof(richTextBox) : nameof(pageInfo));
                }

                var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                string text = textRange.Text;

                foreach (Match match in JumpPointRegex.Matches(text))
                {
                    if (!match.Success) continue;

                    TextPointer start = FindTextPosition(richTextBox.Document.ContentStart, match.Index);
                    TextPointer end = start != null ? FindTextPosition(start, match.Length) : null;

                    if (start == null || end == null) continue;

                    var jumpRange = new TextRange(start, end);
                    string jumpPointText = ExtractJumpPointText(jumpRange.Text);
                    if (string.IsNullOrEmpty(jumpPointText)) continue;

                    var jumpPoint = pageInfo.JumpPoints.FirstOrDefault(jp => jp.Text == jumpPointText);
                    var foregroundBrush = new SolidColorBrush(jumpPoint != null ? ValidJumpPointColor : InvalidJumpPointColor);

                    ApplyJumpPointStyle(jumpRange, foregroundBrush);
                    AttachJumpPointEvents(start.Paragraph, jumpRange, pageInfo, foregroundBrush);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"高亮跳转点时出错: {ex.Message}");
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 查找文本位置
        /// </summary>
        private static TextPointer FindTextPosition(TextPointer start, int offset)
        {
            try
            {
                TextPointer current = start;
                int currentOffset = 0;

                while (current != null)
                {
                    if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                    {
                        int textLength = current.GetTextRunLength(LogicalDirection.Forward);
                        if (currentOffset + textLength >= offset)
                        {
                            return current.GetPositionAtOffset(offset - currentOffset);
                        }
                        currentOffset += textLength;
                    }
                    current = current.GetNextContextPosition(LogicalDirection.Forward);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"查找文本位置时出错: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 提取跳转点文本（去除[[]]标记）
        /// </summary>
        private static string ExtractJumpPointText(string text)
        {
            var match = JumpPointRegex.Match(text);
            return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : string.Empty;
        }

        /// <summary>
        /// 应用跳转点样式
        /// </summary>
        private static void ApplyJumpPointStyle(TextRange range, Brush foregroundBrush)
        {
            range.ApplyPropertyValue(TextElement.ForegroundProperty, foregroundBrush);
            range.ApplyPropertyValue(TextBlock.TextDecorationsProperty, TextDecorations.Underline);
            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        }

        /// <summary>
        /// 附加跳转点事件处理
        /// </summary>
        private static void AttachJumpPointEvents(Paragraph paragraph, TextRange jumpRange, PageJsonInfo pageInfo, SolidColorBrush originalColor)
        {
            if (paragraph == null) return;

            // 移除现有事件处理程序
            RemoveJumpPointEvents(paragraph);

            // 添加新的事件处理程序
            paragraph.MouseLeftButtonDown += (s, e) => OnJumpPointClicked(s, e, jumpRange, pageInfo);
            paragraph.MouseRightButtonDown += (s, e) => OnJumpPointRightClicked(s, e, jumpRange, pageInfo);
            paragraph.MouseEnter += (s, e) => OnJumpPointMouseEnter(s, e, jumpRange);
            paragraph.MouseLeave += (s, e) => OnJumpPointMouseLeave(s, e, jumpRange, originalColor);
            paragraph.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 移除跳转点事件处理
        /// </summary>
        private static void RemoveJumpPointEvents(Paragraph paragraph)
        {
            // 由于无法访问具体的事件处理程序，这里通过重新创建来清除
            paragraph.MouseLeftButtonDown -= (s, e) => { };
            paragraph.MouseRightButtonDown -= (s, e) => { };
            paragraph.MouseEnter -= (s, e) => { };
            paragraph.MouseLeave -= (s, e) => { };
        }

        /// <summary>
        /// 处理跳转点点击事件
        /// </summary>
        private static void OnJumpPointClicked(object sender, MouseButtonEventArgs e, TextRange jumpRange, PageJsonInfo pageInfo)
        {
            try
            {
                string jumpPointText = ExtractJumpPointText(jumpRange.Text);
                if (string.IsNullOrEmpty(jumpPointText)) return;

                var jumpPoint = pageInfo.JumpPoints.FirstOrDefault(jp => jp.Text == jumpPointText);
                if (jumpPoint != null)
                {
                    ExecuteJump(jumpPoint);
                }
                else
                {
                    MessageBox.Show(
                        $"未找到跳转点 '{jumpPointText}' 的目标页面。",
                        "跳转错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"处理跳转点点击时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理跳转点右键点击事件
        /// </summary>
        private static void OnJumpPointRightClicked(object sender, MouseButtonEventArgs e, TextRange jumpRange, PageJsonInfo pageInfo)
        {
            try
            {
                string jumpPointText = ExtractJumpPointText(jumpRange.Text);
                if (!string.IsNullOrEmpty(jumpPointText))
                {
                    ShowTargetSelectionDialog(jumpPointText, pageInfo);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"处理跳转点右键点击时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理跳转点鼠标进入事件
        /// </summary>
        private static void OnJumpPointMouseEnter(object sender, MouseEventArgs e, TextRange jumpRange)
        {
            try
            {
                jumpRange.ApplyPropertyValue(
                    TextElement.ForegroundProperty,
                    new SolidColorBrush(HoverJumpPointColor)
                );
            }
            catch (Exception ex)
            {
                Logger.LogError($"处理跳转点鼠标进入时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理跳转点鼠标离开事件
        /// </summary>
        private static void OnJumpPointMouseLeave(object sender, MouseEventArgs e, TextRange jumpRange, SolidColorBrush originalColor)
        {
            try
            {
                jumpRange.ApplyPropertyValue(TextElement.ForegroundProperty, originalColor);
            }
            catch (Exception ex)
            {
                Logger.LogError($"处理跳转点鼠标离开时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 执行跳转
        /// </summary>
        private static void ExecuteJump(JumpPoint jumpPoint)
        {
            try
            {
                if (jumpPoint == null)
                {
                    throw new ArgumentNullException(nameof(jumpPoint));
                }

                string targetPath = Path.Combine(
                    ProjectData.ProjectPath,
                    jumpPoint.TargetGroup,
                    $"{jumpPoint.TargetPage}.json"
                );

                if (!File.Exists(targetPath))
                {
                    throw new FileNotFoundException($"目标页面文件不存在: {targetPath}");
                }

                NavigationManager.NavigateTo(targetPath);
                Logger.Log($"执行跳转到: {jumpPoint.TargetGroup}/{jumpPoint.TargetPage}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"执行跳转时出错: {ex.Message}");
                MessageBox.Show(
                    $"跳转失败: {ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// 显示目标选择对话框
        /// </summary>
        private static void ShowTargetSelectionDialog(string jumpPointText, PageJsonInfo pageInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(ProjectData.ProjectPath))
                {
                    throw new InvalidOperationException("未打开项目");
                }

                var dialog = CreateTargetSelectionDialog(jumpPointText);
                var listBox = dialog.FindName("PageListBox") as ListBox;

                if (listBox != null)
                {
                    PopulatePageList(listBox);
                }

                bool? result = dialog.ShowDialog();
                if (result == true && listBox?.SelectedItem is JumpTarget target)
                {
                    UpdateJumpPoint(jumpPointText, target, pageInfo);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"显示目标选择对话框时出错: {ex.Message}");
                MessageBox.Show(
                    ex.Message,
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// 创建目标选择对话框
        /// </summary>
        private static Window CreateTargetSelectionDialog(string jumpPointText)
        {
            var listBox = new ListBox { Name = "PageListBox" };
            listBox.DisplayMemberPath = "DisplayName";

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var okButton = new Button
            {
                Content = "确定",
                Width = 75,
                Height = 25,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var cancelButton = new Button
            {
                Content = "取消",
                Width = 75,
                Height = 25
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            var mainPanel = new StackPanel { Margin = new Thickness(10) };
            mainPanel.Children.Add(listBox);
            mainPanel.Children.Add(buttonPanel);

            var dialog = new Window
            {
                Title = $"选择 '{jumpPointText}' 的跳转目标",
                Content = mainPanel,
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            okButton.Click += (s, e) =>
            {
                dialog.DialogResult = true;
                dialog.Close();
            };

            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            };

            return dialog;
        }

        /// <summary>
        /// 填充页面列表
        /// </summary>
        private static void PopulatePageList(ListBox listBox)
        {
            string[] groups = Directory.GetDirectories(ProjectData.ProjectPath);
            foreach (string groupPath in groups)
            {
                string groupName = Path.GetFileName(groupPath);
                string[] jsonFiles = Directory.GetFiles(groupPath, "*.json");

                foreach (string jsonFile in jsonFiles)
                {
                    string pageName = Path.GetFileNameWithoutExtension(jsonFile);
                    listBox.Items.Add(new JumpTarget
                    {
                        GroupName = groupName,
                        PageName = pageName
                    });
                }
            }
        }

        /// <summary>
        /// 更新跳转点
        /// </summary>
        private static void UpdateJumpPoint(string jumpPointText, JumpTarget target, PageJsonInfo pageInfo)
        {
            var existingJumpPoint = pageInfo.JumpPoints.FirstOrDefault(jp => jp.Text == jumpPointText);
            if (existingJumpPoint != null)
            {
                existingJumpPoint.TargetGroup = target.GroupName;
                existingJumpPoint.TargetPage = target.PageName;
            }
            else
            {
                pageInfo.JumpPoints.Add(new JumpPoint
                {
                    Text = jumpPointText,
                    TargetGroup = target.GroupName,
                    TargetPage = target.PageName
                });
            }

            Logger.Log($"更新跳转点: {jumpPointText} -> {target.GroupName}/{target.PageName}");
        }
        #endregion
    }

    /// <summary>
    /// 跳转目标信息类
    /// </summary>
    public class JumpTarget
    {
        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 页面名
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName => $"{GroupName}/{PageName}";
    }
}