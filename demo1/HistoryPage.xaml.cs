using demo1.Services;
using demo1.Models;
using Microsoft.Maui.Graphics; // Color 类型在此命名空间
using System.Collections.ObjectModel;
using System.Linq; // 需要引入 System.Linq 来使用 Select 和 Sum 等扩展方法

namespace demo1
{
    public partial class HistoryPage : ContentPage
    {
        private readonly WageService _wageService; // 建议使用 readonly
        private enum ViewMode { Today, Month, Year, All, Custom }
        private ViewMode _currentViewMode = ViewMode.Today;
        private List<WageRecord> _currentRecords = new List<WageRecord>();

        public HistoryPage(WageService wageService)
        {
            InitializeComponent();
            _wageService = wageService;

            // 设置日期选择器的默认值为当前月份的第一天和最后一天，或根据需要调整
            StartDatePicker.Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDatePicker.Date = DateTime.Now;


            // 默认加载今日记录
            LoadTodayRecordsAsync(); // 无需 await，因为它是一个 async void 方法，会在后台执行
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
            UpdateButtonColors(); // 更新按钮颜色以反映当前不是预设视图

            DateTime startDate = StartDatePicker.Date;
            DateTime endDate = EndDatePicker.Date.AddDays(1).AddTicks(-1); // 确保包含结束日期的所有时间

            if (startDate > endDate)
            {
                await DisplayAlert("日期错误", "开始日期不能晚于结束日期。", "确定");
                return;
            }
            await LoadRecordsAsync(() => _wageService.GetCustomDateRangeRecordsAsync(startDate, endDate));
        }

        // 通用的加载记录方法，以减少重复代码
        private async Task LoadRecordsAsync(Func<Task<List<WageRecord>>> getRecordsFunc)
        {
            try
            {
                var records = await getRecordsFunc();
                _currentRecords = records ?? new List<WageRecord>(); // Ensure _currentRecords is not null

                // 为 CollectionView 创建包含 IsEven 的显示列表
                var displayRecords = _currentRecords.Select((r, i) => new
                {
                    r.Id,
                    r.Date,
                    r.ProductCount,
                    r.Rate,
                    r.DailyWage,
                    IsEven = i % 2 == 0 // 用于交替行背景色
                }).ToList();

                RecordsTable.ItemsSource = displayRecords;

                if (!displayRecords.Any())
                {
                    // 可以选择显示一个消息，例如 "没有找到记录"
                    // EmptyViewLabel.IsVisible = true; // 假设你有一个用于显示空状态的 Label
                }
                else
                {
                    // EmptyViewLabel.IsVisible = false;
                }


                UpdateStatistics(_currentRecords);
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"加载记录失败: {ex.Message}", "确定");
                RecordsTable.ItemsSource = null; // 清空表格以防显示旧数据
                UpdateStatistics(new List<WageRecord>()); // 清空统计
            }
        }


        private void UpdateStatistics(List<WageRecord> records)
        {
            if (records == null || !records.Any())
            {
                TotalCountLabel.Text = "0件";
                AverageRateLabel.Text = "0.00元";
                TotalWageLabel.Text = "0.00元";
                return;
            }

            int totalCount = records.Sum(r => r.ProductCount);
            double totalWage = records.Sum(r => r.DailyWage);
            // 防止除以零，并确保只有在有记录时才计算平均费率
            double averageRate = records.Any(r => r.ProductCount > 0) ? records.Where(r => r.ProductCount > 0).Average(r => r.Rate) : 0;


            TotalCountLabel.Text = $"{totalCount}件";
            AverageRateLabel.Text = $"{averageRate:F2}元"; // 保留两位小数
            TotalWageLabel.Text = $"{totalWage:F2}元"; // 保留两位小数
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int recordId)
            {
                var record = _currentRecords.FirstOrDefault(r => r.Id == recordId);
                if (record != null)
                {
                    string result = await DisplayPromptAsync("编辑记录",
                                                             "请输入新的产品数量:",
                                                             initialValue: record.ProductCount.ToString(),
                                                             keyboard: Keyboard.Numeric,
                                                             accept: "确定",
                                                             cancel: "取消");

                    if (result != null && int.TryParse(result, out int newCount) && newCount >= 0)
                    {
                        // 重新计算费率和工资
                        double newRate = _wageService.CalculateRate(newCount); // 假设 WageService 有此方法
                        double newWage = _wageService.CalculateDailyWage(newCount); // 假设 WageService 有此方法

                        record.ProductCount = newCount;
                        record.Rate = newRate;
                        record.DailyWage = newWage;

                        try
                        {
                            await _wageService.UpdateRecordAsync(record);
                            RefreshCurrentView(); // 刷新视图以显示更改
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("错误", $"更新记录失败: {ex.Message}", "确定");
                        }
                    }
                    else if (result != null) // 如果用户输入了但不是有效的数字
                    {
                        await DisplayAlert("输入无效", "请输入有效的产品数量 (非负整数)。", "确定");
                    }
                }
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int recordId)
            {
                bool confirm = await DisplayAlert("确认删除", "您确定要删除这条记录吗？此操作无法撤销。", "是", "否");
                if (confirm)
                {
                    try
                    {
                        await _wageService.DeleteRecordAsync(recordId);
                        RefreshCurrentView(); // 刷新视图
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("错误", $"删除记录失败: {ex.Message}", "确定");
                    }
                }
            }
        }

        private void RefreshCurrentView()
        {
            // 根据当前的视图模式重新加载数据
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
                    // 对于自定义视图，我们可能需要重新执行搜索，
                    // 或者如果只是编辑/删除，则重新从 _wageService 获取当前范围的数据
                    // 这里简单地再次调用 OnSearchClicked，但这可能不是最优的，因为它会重置 _currentViewMode
                    // 一个更好的方法是让 LoadRecordsAsync 能够接受日期范围作为参数
                    OnSearchClicked(null, null); // 重新执行搜索逻辑
                    break;
            }
        }

        private void UpdateButtonColors()
        {
            // 使用 Color.FromHex 替代 Colors.FromArgb
            // 确保这些颜色字符串是有效的十六进制颜色代码
            var activeColor = Color.FromHex("#FF4081"); // 示例：亮粉色作为激活颜色
            var inactiveColor = Color.FromHex("#9E9E9E"); // 灰色作为非激活颜色

            // 如果 XAML 中使用了 StaticResource 引用颜色，这里也可以使用 Application.Current.Resources["ResourceKey"] as Color
            // 但直接使用 Color.FromHex 更简单直接

            TodayButton.BackgroundColor = _currentViewMode == ViewMode.Today ? activeColor : inactiveColor;
            MonthButton.BackgroundColor = _currentViewMode == ViewMode.Month ? activeColor : inactiveColor;
            YearButton.BackgroundColor = _currentViewMode == ViewMode.Year ? activeColor : inactiveColor;
            AllButton.BackgroundColor = _currentViewMode == ViewMode.All ? activeColor : inactiveColor;

            // 自定义搜索按钮通常有自己固定的颜色，或者在自定义搜索激活时改变
            // SearchButton.BackgroundColor = _currentViewMode == ViewMode.Custom ? activeColor : originalSearchButtonColor;
        }

        // 事件处理程序，对应 XAML 中的按钮点击
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

        // 返回主页按钮的事件处理程序
        private async void OnBackToMainPageClicked(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
            // 如果这是根页面或者只有一个页面在堆栈中，则可能需要不同的逻辑，
            // 例如关闭应用或者导航到特定的主页面（如果应用结构复杂）
        }
    }
}
