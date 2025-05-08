namespace demo1
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // 注册历史页面路由
            Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
            // 在AppShell.cs的构造函数中添加
            Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
        }
    }
}