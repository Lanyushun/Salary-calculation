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

            // Ĭ�ϼ��ؽ��ռ�¼
            LoadTodayRecordsAsync();
        }

        private async void LoadTodayRecordsAsync()
        {
            _currentViewMode = ViewMode.Today;
            CurrentViewLabel.Text = "���ռ�¼";
            UpdateButtonColors();

            try
            {
                var records = await _wageService.GetTodayRecordsAsync();
                RecordsCollection.ItemsSource = records;

                var total = await _wageService.GetTodayTotalWageAsync();
                TotalLabel.Text = $"�ܼ�: {total:F2}Ԫ";
            }
            catch (Exception ex)
            {
                await DisplayAlert("����", $"���ؼ�¼ʧ��: {ex.Message}", "ȷ��");
            }
        }

        private async void LoadMonthRecordsAsync()
        {
            _currentViewMode = ViewMode.Month;
            CurrentViewLabel.Text = "���¼�¼";
            UpdateButtonColors();

            try
            {
                var records = await _wageService.GetMonthRecordsAsync();
                RecordsCollection.ItemsSource = records;

                var total = await _wageService.GetMonthTotalWageAsync();
                TotalLabel.Text = $"�ܼ�: {total:F2}Ԫ";
            }
            catch (Exception ex)
            {
                await DisplayAlert("����", $"���ؼ�¼ʧ��: {ex.Message}", "ȷ��");
            }
        }

        private async void LoadAllRecordsAsync()
        {
            _currentViewMode = ViewMode.All;
            CurrentViewLabel.Text = "���м�¼";
            UpdateButtonColors();

            try
            {
                var records = await _wageService.GetAllRecordsAsync();
                RecordsCollection.ItemsSource = records;

                var total = await _wageService.GetTotalWageAsync();
                TotalLabel.Text = $"�ܼ�: {total:F2}Ԫ";
            }
            catch (Exception ex)
            {
                await DisplayAlert("����", $"���ؼ�¼ʧ��: {ex.Message}", "ȷ��");
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