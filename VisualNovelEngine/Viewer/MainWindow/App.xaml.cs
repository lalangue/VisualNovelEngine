using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using VisualNovelEngine.Model.Logging;

namespace VisualNovelEngine.Viewer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
    }
}

namespace VisualNovelEngine.Model.Text
{
    /// <summary>
    /// LaTeX 渲染管理器，负责处理文本中的 LaTeX 公式
    /// </summary>
    public class LatexRenderer
    {
        // LaTeX 标记正则表达式：匹配 $...$ 和 $$...$$ 格式的公式
        private static readonly Regex LatexRegex = new Regex(@"\$\$(.*?)\$\$|\$(.*?)\$", RegexOptions.Compiled);

        // 是否启用 LaTeX 支持
        private static bool _isLatexEnabled = true;

        // LaTeX 渲染引擎
        private static ILatexEngine _latexEngine;

        /// <summary>
        /// 初始化 LaTeX 渲染器
        /// </summary>
        /// <remarks>
        /// 在应用程序启动时调用此方法以初始化 LaTeX 渲染引擎
        /// </remarks>
        public static void Initialize()
        {
            try
            {
                // 初始化 LaTeX 渲染引擎
                // 注意：实际使用时需要引入第三方库如 WPF-Math
                _latexEngine = new WpfMathEngine();
                Logger.Log("LaTeX 渲染器已初始化");
            }
            catch (Exception ex)
            {
                Logger.LogError("LaTeX 渲染器初始化失败", ex);
                _isLatexEnabled = false;
            }
        }

        /// <summary>
        /// 启用或禁用 LaTeX 支持
        /// </summary>
        /// <param name="enable">是否启用 LaTeX 支持</param>
        public static void EnableLatex(bool enable)
        {
            _isLatexEnabled = enable;
            Logger.Log($"LaTeX 支持已{(_isLatexEnabled ? "启用" : "禁用")}");
        }

        /// <summary>
        /// 处理富文本框中的 LaTeX 公式
        /// </summary>
        /// <param name="richTextBox">要处理的富文本框</param>
        /// <remarks>
        /// 此方法会扫描富文本框中的文本，识别 LaTeX 公式并将其渲染为图像
        /// </remarks>
        public static void ProcessLatexInRichTextBox(RichTextBox richTextBox)
        {
            // 验证参数和状态
            if (!_isLatexEnabled || richTextBox == null || _latexEngine == null)
                return;

            try
            {
                // 获取文本
                TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                string text = textRange.Text;

                // 如果文本为空，直接返回
                if (string.IsNullOrEmpty(text))
                    return;

                // 查找所有 LaTeX 公式
                var matches = LatexRegex.Matches(text);
                if (matches.Count == 0)
                    return;

                // 创建新文档
                var document = new FlowDocument();

                // 当前处理位置
                int currentPosition = 0;

                // 处理每个匹配项
                foreach (Match match in matches)
                {
                    // 添加匹配前的文本
                    if (match.Index > currentPosition)
                    {
                        var paragraph = new Paragraph();
                        paragraph.Inlines.Add(new Run(text.Substring(currentPosition, match.Index - currentPosition)));
                        document.Blocks.Add(paragraph);
                    }

                    // 提取 LaTeX 公式
                    string formula = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    bool isDisplayMode = match.Groups[1].Success; // $$ 表示显示模式，$ 表示行内模式

                    try
                    {
                        // 渲染 LaTeX 公式
                        var formulaImage = _latexEngine.RenderFormula(formula, isDisplayMode);

                        // 创建包含公式图像的段落
                        var paragraph = new Paragraph();

                        // 显示模式的公式居中显示并添加边距
                        if (isDisplayMode)
                        {
                            paragraph.TextAlignment = TextAlignment.Center;
                            paragraph.Margin = new Thickness(0, 10, 0, 10);
                        }

                        // 添加图像
                        var image = new Image
                        {
                            Source = formulaImage,
                            Stretch = Stretch.None
                        };

                        paragraph.Inlines.Add(new InlineUIContainer(image));
                        document.Blocks.Add(paragraph);
                    }
                    catch (Exception ex)
                    {
                        // 渲染失败，显示原始文本并标记为红色斜体
                        var paragraph = new Paragraph();
                        var run = new Run(match.Value)
                        {
                            Foreground = Brushes.Red,
                            FontStyle = FontStyles.Italic
                        };
                        paragraph.Inlines.Add(run);
                        document.Blocks.Add(paragraph);

                        Logger.LogError($"LaTeX 公式渲染失败: {formula}", ex);
                    }

                    // 更新当前位置
                    currentPosition = match.Index + match.Length;
                }

                // 添加剩余文本
                if (currentPosition < text.Length)
                {
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run(text.Substring(currentPosition)));
                    document.Blocks.Add(paragraph);
                }

                // 更新富文本框
                richTextBox.Document = document;
            }
            catch (Exception ex)
            {
                Logger.LogError("处理 LaTeX 公式时发生错误", ex);
            }
        }

        /// <summary>
        /// 导出 LaTeX 公式为图像
        /// </summary>
        /// <param name="formula">LaTeX 公式</param>
        /// <param name="isDisplayMode">是否为显示模式</param>
        /// <param name="fontSize">字体大小</param>
        /// <returns>渲染后的公式图像</returns>
        public static BitmapSource ExportFormulaAsImage(string formula, bool isDisplayMode, double fontSize = 16)
        {
            if (!_isLatexEnabled || _latexEngine == null || string.IsNullOrEmpty(formula))
                return null;

            try
            {
                return _latexEngine.RenderFormula(formula, isDisplayMode, fontSize);
            }
            catch (Exception ex)
            {
                Logger.LogError($"导出 LaTeX 公式为图像失败: {formula}", ex);
                return null;
            }
        }
    }

    /// <summary>
    /// LaTeX 渲染引擎接口
    /// </summary>
    public interface ILatexEngine
    {
        /// <summary>
        /// 渲染 LaTeX 公式为图像
        /// </summary>
        /// <param name="formula">LaTeX 公式</param>
        /// <param name="isDisplayMode">是否为显示模式</param>
        /// <param name="fontSize">字体大小</param>
        /// <returns>渲染后的公式图像</returns>
        BitmapSource RenderFormula(string formula, bool isDisplayMode, double fontSize = 16);
    }

    /// <summary>
    /// 基于 WPF-Math 的 LaTeX 渲染引擎实现
    /// </summary>
    /// <remarks>
    /// 注意：此类需要引入第三方库 WPF-Math 才能正常工作
    /// 请在项目中添加 WPF-Math NuGet 包
    /// </remarks>
    public class WpfMathEngine : ILatexEngine
    {
        /// <summary>
        /// 渲染 LaTeX 公式为图像
        /// </summary>
        /// <param name="formula">LaTeX 公式</param>
        /// <param name="isDisplayMode">是否为显示模式</param>
        /// <param name="fontSize">字体大小</param>
        /// <returns>渲染后的公式图像</returns>
        public BitmapSource RenderFormula(string formula, bool isDisplayMode, double fontSize = 16)
        {
            // 注意：此处为示例代码，实际实现需要引入 WPF-Math 库
            // 实际使用时请替换为真实的实现
            /*
            var parser = new WpfMath.TexFormulaParser();
            var texFormula = parser.Parse(formula);
            
            var renderer = texFormula.GetRenderer(
                isDisplayMode ? WpfMath.TexStyle.Display : WpfMath.TexStyle.Text, 
                fontSize);
                
            return renderer.RenderToBitmap(0, 0);
            */

            // 临时返回空图像，实际使用时请删除此行
            return new BitmapImage();
        }
    }
}
