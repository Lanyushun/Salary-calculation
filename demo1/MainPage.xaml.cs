using demo1.Services;
using demo1.Models;

namespace demo1
{
    public partial class MainPage : ContentPage
    {
        private WageService _wageService;
        private int _currentProductCount;
        private double _currentRate;
        private double _currentWage;

        public MainPage()
        {
            InitializeComponent();

            // 初始化数据服务
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "wages.db3");
            _wageService = new WageService(dbPath);

            // 加载统计数据
            LoadStatisticsAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadStatisticsAsync();
        }

        private async 
        Task
LoadStatisticsAsync()
        {
            try
            {
                var todayTotal = await _wageService.GetTodayTotalWageAsync();
                var monthTotal = await _wageService.GetMonthTotalWageAsync();
                var allTimeTotal = await _wageService.GetTotalWageAsync();

                TodayTotalLabel.Text = $"{todayTotal:F2}元";
                MonthTotalLabel.Text = $"{monthTotal:F2}元";
                AllTimeTotalLabel.Text = $"{allTimeTotal:F2}元";
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载统计数据失败: {ex.Message}", "确定");
            }
        }

        private async void OnCalculateClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProductCountEntry.Text) ||
                    !int.TryParse(ProductCountEntry.Text, out _currentProductCount))
                {
                    await DisplayAlert("提示", "请输入有效的产品数量", "确定");
                    return;
                }

                _currentRate = _wageService.CalculateRate(_currentProductCount);
                _currentWage = _wageService.CalculateDailyWage(_currentProductCount);

                RateLabel.Text = $"{_currentRate:F2}元/件";
                DailyWageLabel.Text = $"{_currentWage:F2}元";
                StatusLabel.Text = "未保存";
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"计算失败: {ex.Message}", "确定");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (_currentProductCount <= 0 || _currentWage <= 0)
                {
                    await DisplayAlert("提示", "请先计算工资", "确定");
                    return;
                }

                await _wageService.SaveRecordAsync(_currentProductCount);
                StatusLabel.Text = "已保存";
                await LoadStatisticsAsync();

                bool continueInput = await DisplayAlert("保存成功", "记录已保存，是否清除输入继续?", "是", "否");
                if (continueInput)
                {
                    ProductCountEntry.Text = string.Empty;
                    RateLabel.Text = "0.00元/件";
                    DailyWageLabel.Text = "0.00元";
                    StatusLabel.Text = "未保存";
                    _currentProductCount = 0;
                    _currentRate = 0;
                    _currentWage = 0;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"保存失败: {ex.Message}", "确定");
            }
        }

        private async void OnViewHistoryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistoryPage(_wageService));
        }
        // 在MainPage.xaml.cs中添加
        private async void OnAboutClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AboutPage());
        }
    }
}