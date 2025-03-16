using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VisualNovelEngine.Model.Group
{
    public static class GroupData
    {
        public static string GroupName { get; set; }
        public static string TextFolderPath { get; set; }
        public static string ImageFolderPath { get; set; }
        public static string MusicFolderPath { get; set; }

        // 获取文本文件夹的相对路径
        public static string GetRelativeTextPath()
        {
            return Path.Combine(GroupName, "text");
        }

        // 获取图片文件夹的相对路径
        public static string GetRelativeImagePath()
        {
            return Path.Combine(GroupName, "image");
        }

        // 获取音乐文件夹的相对路径
        public static string GetRelativeMusicPath()
        {
            return Path.Combine(GroupName, "music");
        }
    }
}
