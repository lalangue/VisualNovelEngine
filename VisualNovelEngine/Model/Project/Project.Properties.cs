using System.Collections.Generic;

namespace VisualNovelEngine.Model
{
    public partial class Project
    {
        // 单例模式实现，使用静态字段和静态构造函数
        private static readonly Project instance = new Project();
        public static Project Instance
        {
            get
            {
                return instance;
            }
        }

        // 项目名称
        private string projectName;
        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; }
        }

        // 项目对应的 JSON 文件路径
        private string projectJsonPath;
        public string ProjectJsonPath
        {
            get { return projectJsonPath; }
            set { projectJsonPath = value; }
        }

        // 项目包含的组路径列表
        private List<string> groupPaths;
        public List<string> GroupPaths
        {
            get { return groupPaths; }
            set { groupPaths = value; }
        }

        // 项目根目录路径
        private string projectRootPath;
        public string ProjectRootPath
        {
            get { return projectRootPath; }
            set { projectRootPath = value; }
        }

        private Project()
        {
            // 初始化组路径列表
            groupPaths = new List<string>();
        }
    }
}