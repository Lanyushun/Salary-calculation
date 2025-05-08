using demo1.Services;
using demo1.Models;
using Microsoft.Maui.Graphics;

namespace demo1
{
    public partial class HistoryPage : ContentPage
    {
        private WageService _wageService;
        private enum ViewMode { Today, Month, All }
        private ViewMode _currentViewMode = ViewMode.Today;

        public HistoryPage(WageService wageService)
        {
            InitializeComponent();
            _wageService = wageService;

            // 默认加载今日记录
            LoadTodayRecordsAsync();
        }

        private async void LoadTodayRecordsAsync()
        {
            _currentViewMode = ViewMode.Today;
            CurrentViewLabel.Text = "今日记录";
            UpdateButtonColors();

            try
            {
                var records = await _wageService.GetTodayRecordsAsync();
                RecordsCollection.ItemsSource = records;

                var total = await _wageService.GetTodayTotalWageAsync();
                TotalLabel.Text = $"总计: {total:F2}元";
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载记录失败: {ex.Message}", "确定");
            }
        }

        private async void LoadMonthRecordsAsync()
        {
            _currentViewMode = ViewMode.Month;
            CurrentViewLabel.Text = "本月记录";
            UpdateButtonColors();

            try
            {
                var records = await _wageService.GetMonthRecordsAsync();
                RecordsCollection.ItemsSource = records;

                var total = await _wageService.GetMonthTotalWageAsync();
                TotalLabel.Text = $"总计: {total:F2}元";
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载记录失败: {ex.Message}", "确定");
            }
        }

        private async void LoadAllRecordsAsync()
        {
            _currentViewMode = ViewMode.All;
            CurrentViewLabel.Text = "所有记录";
            UpdateButtonColors();

            try
            {
                var records = await _wageService.GetAllRecordsAsync();
                RecordsCollection.ItemsSource = records;

                var total = await _wageService.GetTotalWageAsync();
                TotalLabel.Text = $"总计: {total:F2}元";
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载记录失败: {ex.Message}", "确定");
            }
        }

        private void UpdateButtonColors()
        {
            TodayButton.BackgroundColor = _currentViewMode == ViewMode.Today ? Color.FromArgb("#4caf50") : Color.FromArgb("#9e9e9e");
            MonthButton.BackgroundColor = _currentViewMode == ViewMode.Month ? Color.FromArgb("#2196f3") : Color.FromArgb("#9e9e9e");
            AllButton.BackgroundColor = _currentViewMode == ViewMode.All ? Color.FromArgb("#ff9800") : Color.FromArgb("#9e9e9e");
        }

        private void OnTodayClicked(object sender, EventArgs e)
        {
            LoadTodayRecordsAsync();
        }

        private void OnMonthClicked(object sender, EventArgs e)
        {
            LoadMonthRecordsAsync();
        }

        private void OnAllClicked(object sender, EventArgs e)
        {
            LoadAllRecordsAsync();
        }
    }
}