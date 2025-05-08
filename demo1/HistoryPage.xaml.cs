using demo1.Services;
using demo1.Models;
using Microsoft.Maui.Graphics; // Color �����ڴ������ռ�
using System.Collections.ObjectModel;
using System.Linq; // ��Ҫ���� System.Linq ��ʹ�� Select �� Sum ����չ����

namespace demo1
{
    public partial class HistoryPage : ContentPage
    {
        private readonly WageService _wageService; // ����ʹ�� readonly
        private enum ViewMode { Today, Month, Year, All, Custom }
        private ViewMode _currentViewMode = ViewMode.Today;
        private List<WageRecord> _currentRecords = new List<WageRecord>();

        public HistoryPage(WageService wageService)
        {
            InitializeComponent();
            _wageService = wageService;

            // ��������ѡ������Ĭ��ֵΪ��ǰ�·ݵĵ�һ������һ�죬�������Ҫ����
            StartDatePicker.Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDatePicker.Date = DateTime.Now;


            // Ĭ�ϼ��ؽ��ռ�¼
            LoadTodayRecordsAsync(); // ���� await����Ϊ����һ�� async void ���������ں�ִ̨��
        }

        private async void LoadTodayRecordsAsync()
        {
            _currentViewMode = ViewMode.Today;
            UpdateButtonColors();
            await LoadRecordsAsync(() => _wageService.GetTodayRecordsAsync());
        }

        private async void LoadMonthRecordsAsync()
        {
            _currentViewMode = ViewMode.Month;
            UpdateButtonColors();
            await LoadRecordsAsync(() => _wageService.GetMonthRecordsAsync());
        }

        private async void LoadYearRecordsAsync()
        {
            _currentViewMode = ViewMode.Year;
            UpdateButtonColors();
            await LoadRecordsAsync(() => _wageService.GetCurrentYearRecordsAsync());
        }

        private async void LoadAllRecordsAsync()
        {
            _currentViewMode = ViewMode.All;
            UpdateButtonColors();
            await LoadRecordsAsync(() => _wageService.GetAllRecordsAsync());
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            _currentViewMode = ViewMode.Custom;
            UpdateButtonColors(); // ���°�ť��ɫ�Է�ӳ��ǰ����Ԥ����ͼ

            DateTime startDate = StartDatePicker.Date;
            DateTime endDate = EndDatePicker.Date.AddDays(1).AddTicks(-1); // ȷ�������������ڵ�����ʱ��

            if (startDate > endDate)
            {
                await DisplayAlert("���ڴ���", "��ʼ���ڲ������ڽ������ڡ�", "ȷ��");
                return;
            }
            await LoadRecordsAsync(() => _wageService.GetCustomDateRangeRecordsAsync(startDate, endDate));
        }

        // ͨ�õļ��ؼ�¼�������Լ����ظ�����
        private async Task LoadRecordsAsync(Func<Task<List<WageRecord>>> getRecordsFunc)
        {
            try
            {
                var records = await getRecordsFunc();
                _currentRecords = records ?? new List<WageRecord>(); // Ensure _currentRecords is not null

                // Ϊ CollectionView �������� IsEven ����ʾ�б�
                var displayRecords = _currentRecords.Select((r, i) => new
                {
                    r.Id,
                    r.Date,
                    r.ProductCount,
                    r.Rate,
                    r.DailyWage,
                    IsEven = i % 2 == 0 // ���ڽ����б���ɫ
                }).ToList();

                RecordsTable.ItemsSource = displayRecords;

                if (!displayRecords.Any())
                {
                    // ����ѡ����ʾһ����Ϣ������ "û���ҵ���¼"
                    // EmptyViewLabel.IsVisible = true; // ��������һ��������ʾ��״̬�� Label
                }
                else
                {
                    // EmptyViewLabel.IsVisible = false;
                }


                UpdateStatistics(_currentRecords);
            }
            catch (Exception ex)
            {
                await DisplayAlert("����", $"���ؼ�¼ʧ��: {ex.Message}", "ȷ��");
                RecordsTable.ItemsSource = null; // ��ձ���Է���ʾ������
                UpdateStatistics(new List<WageRecord>()); // ���ͳ��
            }
        }


        private void UpdateStatistics(List<WageRecord> records)
        {
            if (records == null || !records.Any())
            {
                TotalCountLabel.Text = "0��";
                AverageRateLabel.Text = "0.00Ԫ";
                TotalWageLabel.Text = "0.00Ԫ";
                return;
            }

            int totalCount = records.Sum(r => r.ProductCount);
            double totalWage = records.Sum(r => r.DailyWage);
            // ��ֹ�����㣬��ȷ��ֻ�����м�¼ʱ�ż���ƽ������
            double averageRate = records.Any(r => r.ProductCount > 0) ? records.Where(r => r.ProductCount > 0).Average(r => r.Rate) : 0;


            TotalCountLabel.Text = $"{totalCount}��";
            AverageRateLabel.Text = $"{averageRate:F2}Ԫ"; // ������λС��
            TotalWageLabel.Text = $"{totalWage:F2}Ԫ"; // ������λС��
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int recordId)
            {
                var record = _currentRecords.FirstOrDefault(r => r.Id == recordId);
                if (record != null)
                {
                    string result = await DisplayPromptAsync("�༭��¼",
                                                             "�������µĲ�Ʒ����:",
                                                             initialValue: record.ProductCount.ToString(),
                                                             keyboard: Keyboard.Numeric,
                                                             accept: "ȷ��",
                                                             cancel: "ȡ��");

                    if (result != null && int.TryParse(result, out int newCount) && newCount >= 0)
                    {
                        // ���¼�����ʺ͹���
                        double newRate = _wageService.CalculateRate(newCount); // ���� WageService �д˷���
                        double newWage = _wageService.CalculateDailyWage(newCount); // ���� WageService �д˷���

                        record.ProductCount = newCount;
                        record.Rate = newRate;
                        record.DailyWage = newWage;

                        try
                        {
                            await _wageService.UpdateRecordAsync(record);
                            RefreshCurrentView(); // ˢ����ͼ����ʾ����
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("����", $"���¼�¼ʧ��: {ex.Message}", "ȷ��");
                        }
                    }
                    else if (result != null) // ����û������˵�������Ч������
                    {
                        await DisplayAlert("������Ч", "��������Ч�Ĳ�Ʒ���� (�Ǹ�����)��", "ȷ��");
                    }
                }
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int recordId)
            {
                bool confirm = await DisplayAlert("ȷ��ɾ��", "��ȷ��Ҫɾ��������¼�𣿴˲����޷�������", "��", "��");
                if (confirm)
                {
                    try
                    {
                        await _wageService.DeleteRecordAsync(recordId);
                        RefreshCurrentView(); // ˢ����ͼ
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("����", $"ɾ����¼ʧ��: {ex.Message}", "ȷ��");
                    }
                }
            }
        }

        private void RefreshCurrentView()
        {
            // ���ݵ�ǰ����ͼģʽ���¼�������
            switch (_currentViewMode)
            {
                case ViewMode.Today:
                    LoadTodayRecordsAsync();
                    break;
                case ViewMode.Month:
                    LoadMonthRecordsAsync();
                    break;
                case ViewMode.Year:
                    LoadYearRecordsAsync();
                    break;
                case ViewMode.All:
                    LoadAllRecordsAsync();
                    break;
                case ViewMode.Custom:
                    // �����Զ�����ͼ�����ǿ�����Ҫ����ִ��������
                    // �������ֻ�Ǳ༭/ɾ���������´� _wageService ��ȡ��ǰ��Χ������
                    // ����򵥵��ٴε��� OnSearchClicked��������ܲ������ŵģ���Ϊ�������� _currentViewMode
                    // һ�����õķ������� LoadRecordsAsync �ܹ��������ڷ�Χ��Ϊ����
                    OnSearchClicked(null, null); // ����ִ�������߼�
                    break;
            }
        }

        private void UpdateButtonColors()
        {
            // ʹ�� Color.FromHex ��� Colors.FromArgb
            // ȷ����Щ��ɫ�ַ�������Ч��ʮ��������ɫ����
            var activeColor = Color.FromHex("#FF4081"); // ʾ��������ɫ��Ϊ������ɫ
            var inactiveColor = Color.FromHex("#9E9E9E"); // ��ɫ��Ϊ�Ǽ�����ɫ

            // ��� XAML ��ʹ���� StaticResource ������ɫ������Ҳ����ʹ�� Application.Current.Resources["ResourceKey"] as Color
            // ��ֱ��ʹ�� Color.FromHex ����ֱ��

            TodayButton.BackgroundColor = _currentViewMode == ViewMode.Today ? activeColor : inactiveColor;
            MonthButton.BackgroundColor = _currentViewMode == ViewMode.Month ? activeColor : inactiveColor;
            YearButton.BackgroundColor = _currentViewMode == ViewMode.Year ? activeColor : inactiveColor;
            AllButton.BackgroundColor = _currentViewMode == ViewMode.All ? activeColor : inactiveColor;

            // �Զ���������ťͨ�����Լ��̶�����ɫ���������Զ�����������ʱ�ı�
            // SearchButton.BackgroundColor = _currentViewMode == ViewMode.Custom ? activeColor : originalSearchButtonColor;
        }

        // �¼�������򣬶�Ӧ XAML �еİ�ť���
        private void OnTodayClicked(object sender, EventArgs e)
        {
            LoadTodayRecordsAsync();
        }

        private void OnMonthClicked(object sender, EventArgs e)
        {
            LoadMonthRecordsAsync();
        }

        private void OnYearClicked(object sender, EventArgs e)
        {
            LoadYearRecordsAsync();
        }

        private void OnAllClicked(object sender, EventArgs e)
        {
            LoadAllRecordsAsync();
        }

        // ������ҳ��ť���¼��������
        private async void OnBackToMainPageClicked(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
            // ������Ǹ�ҳ�����ֻ��һ��ҳ���ڶ�ջ�У��������Ҫ��ͬ���߼���
            // ����ر�Ӧ�û��ߵ������ض�����ҳ�棨���Ӧ�ýṹ���ӣ�
        }
    }
}
