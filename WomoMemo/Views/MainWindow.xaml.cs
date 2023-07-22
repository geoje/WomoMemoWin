using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WomoMemo.Models;
using WomoMemo.Views;
using System.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace WomoMemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Memo> memos = new ObservableCollection<Memo>();

        public MainWindow()
        {
            InitializeComponent();
            Config.Load();
            Task.Run(UpdateDataFromServer);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        async void UpdateDataFromServer()
        {
            for (; ; Thread.Sleep(1000))
            {
                if (string.IsNullOrEmpty(Config.sessionTokenValue)) continue;

                // Toggle user button
                Dispatcher.Invoke(() =>
                {
                    btnUser.Visibility = string.IsNullOrEmpty(Config.sessionTokenValue) ? Visibility.Collapsed : Visibility.Visible;
                    btnLogin.Visibility = string.IsNullOrEmpty(Config.sessionTokenValue) ? Visibility.Visible : Visibility.Collapsed;
                });

                // Get user profile


                // Get memos
                var baseAddress = new Uri(Config.memoUrl);
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    cookieContainer.Add(baseAddress, new Cookie(Config.sessionTokenName, Config.sessionTokenValue));
                    var response = await client.GetAsync("/api/memos");
                    if (response.IsSuccessStatusCode)
                    {
                        Trace.WriteLine(await response.Content.ReadAsStringAsync());
                        // JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
                        // Trace.WriteLine(result.ToString());
                    }
                }
            }
        }
    }
}
