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
            // ����QQ�ŵ�������
            await Clipboard.SetTextAsync(QQLabel.Text);
            await DisplayAlert("��ʾ", "QQ���Ѹ��Ƶ�������", "ȷ��");
        }
    }
}