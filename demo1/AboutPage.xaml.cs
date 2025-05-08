using Microsoft.Maui.Controls;
using System;

namespace demo1
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private async void OnCopyQQClicked(object sender, EventArgs e)
        {
            // 复制QQ号到剪贴板
            await Clipboard.SetTextAsync(QQLabel.Text);
            await DisplayAlert("提示", "QQ号已复制到剪贴板", "确定");
        }
    }
}