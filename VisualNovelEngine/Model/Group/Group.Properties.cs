using System.Collections.Generic;

namespace VisualNovelEngine.Model
{
    public partial class Group
    {
        // 单例模式实现，使用静态字段和静态构造函数
        private static readonly Group instance = new Group();
        public static Group Instance
        {
            get
            {
                return instance;
            }
        }

        // 组名称
        private string groupName;
        public string GroupName
        {
            get { return groupName; }
            set { groupName = value; }
        }

        // 组对应的 JSON 文件路径
        private string groupJsonPath;
        public string GroupJsonPath
        {
            get { return groupJsonPath; }
            set { groupJsonPath = value; }
        }

        // 组内包含的页面路径列表
        private List<string> pagePaths;
        public List<string> PagePaths
        {
            get { return pagePaths; }
            set { pagePaths = value; }
        }

        // 文本文件夹相对路径
        private string textFolderPath;
        public string TextFolderPath
        {
            get { return textFolderPath; }
            set { textFolderPath = value; }
        }

        // 图片文件夹相对路径
        private string imageFolderPath;
        public string ImageFolderPath
        {
            get { return imageFolderPath; }
            set { imageFolderPath = value; }
        }

        // 音乐文件夹相对路径
        private string musicFolderPath;
        public string MusicFolderPath
        {
            get { return musicFolderPath; }
            set { musicFolderPath = value; }
        }

        // 添加 GroupPath 属性
        private string groupPath;
        public string GroupPath
        {
            get { return groupPath; }
            set { groupPath = value; }
        }

        private Group()
        {
            // 初始化页面路径列表
            pagePaths = new List<string>();
        }
    }
}