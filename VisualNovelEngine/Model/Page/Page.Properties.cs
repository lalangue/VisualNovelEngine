using System;

namespace VisualNovelEngine.Model
{
    public partial class Page
    {
        // 单例模式实现，使用静态字段和静态构造函数
        private static readonly Page instance = new Page();
        public static Page Instance
        {
            get
            {
                return instance;
            }
        }

        // 页面名称
        private string pageName;
        public string PageName
        {
            get { return pageName; }
            set { pageName = value; }
        }

        // 页面路径，核心属性
        private string pagePath;
        public string PagePath
        {
            get { return pagePath; }
            set { pagePath = value; }
        }

        // 文本文件相对路径
        private string textFilePath;
        public string TextFilePath
        {
            get { return textFilePath; }
            set { textFilePath = value; }
        }

        // 图片文件相对路径
        private string imageFilePath;
        public string ImageFilePath
        {
            get { return imageFilePath; }
            set { imageFilePath = value; }
        }

        // 音乐文件相对路径
        private string musicFilePath;
        public string MusicFilePath
        {
            get { return musicFilePath; }
            set { musicFilePath = value; }
        }

        // 前一个页面的路径
        private string previousPagePath;
        public string PreviousPagePath
        {
            get { return previousPagePath; }
            set { previousPagePath = value; }
        }

        // 后一个页面的路径
        private string nextPagePath;
        public string NextPagePath
        {
            get { return nextPagePath; }
            set { nextPagePath = value; }
        }

        // 页面跳转信息，可存储多个跳转路径
        private string[] pageJumpPaths;
        public string[] PageJumpPaths
        {
            get { return pageJumpPaths; }
            set { pageJumpPaths = value; }
        }

        private Page()
        {
            // 私有构造函数，确保单例
        }
    }
}