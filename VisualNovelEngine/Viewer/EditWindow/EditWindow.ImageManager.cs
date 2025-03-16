using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using VisualNovelEngine.Model.Group;
using VisualNovelEngine.Model.Project;

namespace VisualNovelEngine.Viewer.EditWindow
{
    public partial class EditWindow
    {
        private Image _backgroundImage = new Image();

        public void SetBackgroundImage(string imagePath)
        {
            if (imagePath == null || string.IsNullOrEmpty(imagePath))
            {
                // 如果传入的路径是NULL或空字符串，则清除图片源
                _backgroundImage.Source = null;
            }
            else if (_backgroundImage.Source == null || !_backgroundImage.Source.ToString().Equals(imagePath))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                _backgroundImage.Source = bitmap;
            }
        }

        public string OpenImageDialogWithInitialPath()
        {
            string imageFolerPath = Path.Combine(ProjectData.ProjectPath, GroupData.ImageFolderPath);
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择图片文件",
                Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                InitialDirectory = imageFolerPath,
                CheckFileExists = true,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }
    }
}
