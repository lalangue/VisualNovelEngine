using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VisualNovelEngine.Model.Project;

namespace VisualNovelEngine.Model.Group
{
    public static class GroupEdit
    {
        public static void InitializeGroup(string groupPath)
        {
            Directory.CreateDirectory(groupPath);

            string textPath = Path.Combine(groupPath, "Text");
            Directory.CreateDirectory(textPath);
            GroupData.TextFolderPath = textPath;

            string imagePath = Path.Combine(groupPath, "Images");
            Directory.CreateDirectory(imagePath);
            GroupData.ImageFolderPath = imagePath;

            string musicPath = Path.Combine(groupPath, "Music");
            Directory.CreateDirectory(musicPath);
            GroupData.MusicFolderPath = musicPath;
        }

        public static void SetupGroup()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = ProjectData.ProjectPath,
                FileName = "NewGroup",
                DefaultExt = "",
                Filter = "All files (*.*)|*.*",
                AddExtension = false,
                OverwritePrompt = false
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string folderName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                string groupPath = Path.Combine(ProjectData.ProjectPath, folderName);
                if (Directory.Exists(groupPath))
                {
                    MessageBox.Show("Group already exists!");
                    return;
                }
                InitializeGroup(groupPath);
                GroupData.GroupName = folderName;
            }
        }
    }
}
