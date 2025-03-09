namespace VisualNovelEngine.Viewer
{
    public partial class Viewer
    {
        // 单例模式实现，使用静态字段和静态构造函数
        private static readonly Viewer instance = new Viewer();
        public static Viewer Instance
        {
            get
            {
                return instance;
            }
        }

        // 可以添加 Viewer 相关的属性
        private string currentPageName;
        public string CurrentPageName
        {
            get { return currentPageName; }
            set { currentPageName = value; }
        }

        private Viewer()
        {
            // 私有构造函数，防止外部实例化
        }
    }
}