using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VisualNovelEngine.Model.Group;

namespace VisualNovelEngine.Model.Project
{
    public static class ProjectData
    {
        public static string ProjectPath { get; set; }

        // 获取相对路径对应的绝对路径
        public static string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(ProjectPath, relativePath);
        }

        // 获取组文件夹的绝对路径
        public static string GetGroupAbsolutePath(string groupName)
        {
            return Path.Combine(ProjectPath, groupName);
        }

        // 获取文本文件的绝对路径
        public static string GetTextAbsolutePath(string relativePath)
        {
            return Path.Combine(ProjectPath, GroupData.GetRelativeTextPath(), relativePath);
        }

        // 获取图片文件的绝对路径
        public static string GetImageAbsolutePath(string relativePath)
        {
            return Path.Combine(ProjectPath, GroupData.GetRelativeImagePath(), relativePath);
        }

        // 获取音乐文件的绝对路径
        public static string GetMusicAbsolutePath(string relativePath)
        {
            return Path.Combine(ProjectPath, GroupData.GetRelativeMusicPath(), relativePath);
        }
    }
}
