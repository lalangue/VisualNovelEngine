using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;

namespace VisualNovelEngine.Model.Page.PageEdit
{
    public static class RichTextFileHandler
    {
        public static string CreatePairedRtfFile(string jsonFilePath)
        {
            var timeStamp = Path.GetFileNameWithoutExtension(jsonFilePath).Split('_')[1];
            var rtfFileName = $"text_{timeStamp}.rtf";
            var directory = Path.GetDirectoryName(jsonFilePath);
            var rtfPath = Path.Combine(directory, rtfFileName);
            Directory.CreateDirectory(directory);
            File.WriteAllText(rtfPath, string.Empty);

            return rtfPath;
        }

        public static void LoadRtfToEditWindow(string rtfPath, RichTextBox targetRichTextBox)
        {
            if (!File.Exists(rtfPath))
            {
                MessageBox.Show("指定的RTF文件不存在");
                return;
            }

            try
            {
                using FileStream fs = new FileStream(rtfPath, FileMode.Open);
                TextRange documentRange = new TextRange(
                    targetRichTextBox.Document.ContentStart,
                    targetRichTextBox.Document.ContentEnd
                );

                documentRange.Load(fs, DataFormats.Rtf);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件失败: {ex.Message}");
            }
        }

        public static void SaveRtfFromEditWindow(string savePath, RichTextBox sourceRichTextBox)
        {
            try
            {
                var directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                TextRange documentRange = new TextRange(
                    sourceRichTextBox.Document.ContentStart,
                    sourceRichTextBox.Document.ContentEnd
                );

                using (FileStream fs = new FileStream(savePath, FileMode.Create))
                {
                    documentRange.Save(fs, DataFormats.Rtf);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存文件失败: {ex.Message}");
            }
        }
    }
}
