namespace VisualNovelEngine.ViewModel
{
    public partial class ViewModel
    {
        // 单例模式实现，使用静态字段和静态构造函数
        private static readonly ViewModel instance = new ViewModel();
        public static ViewModel Instance
        {
            get
            {
                return instance;
            }
        }

        // 可以添加 ViewModel 相关的属性
        private bool isDeveloperMode;
        public bool IsDeveloperMode
        {
            get { return isDeveloperMode; }
            set { isDeveloperMode = value; }
        }

        private ViewModel()
        {
            // 私有构造函数，防止外部实例化
        }
    }
}