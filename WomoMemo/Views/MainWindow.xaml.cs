using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WomoMemo.Models;
using WomoMemo.Views;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace WomoMemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Memo> Memos = new ObservableCollection<Memo>();

        public MainWindow()
        {
            InitializeComponent();
            Config.Load();
            Task.Run(UpdateDataFromServer);
            lstMemo.ItemsSource = Memos;
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
            var memoBaseAddress = new Uri(Config.MemoUrl);

            for (; ; Thread.Sleep(1000))
            {
                if (string.IsNullOrEmpty(Config.SessionTokenValue)) continue;
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(memoBaseAddress, new Cookie(Config.SessionTokenName, Config.SessionTokenValue));

                // Toggle user button
                Dispatcher.Invoke(() =>
                {
                    btnUser.Visibility = string.IsNullOrEmpty(Config.SessionTokenValue) ? Visibility.Collapsed : Visibility.Visible;
                    btnLogin.Visibility = string.IsNullOrEmpty(Config.SessionTokenValue) ? Visibility.Visible : Visibility.Collapsed;
                });

                // Get user profile
                if (!string.IsNullOrEmpty(Config.SessionTokenValue) && string.IsNullOrEmpty(User.Id))
                {
                    try
                    {
                        using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                        using (var client = new HttpClient(handler) { BaseAddress = memoBaseAddress })
                        {
                            var response = await client.GetAsync("/api/auth/session");
                            response.EnsureSuccessStatusCode();

                            // Parse session to User instance
                            JToken result = JObject.Parse(await response.Content.ReadAsStringAsync())["user"] ?? JObject.Parse("{}");
                            User.Name = result["name"]?.ToString() ?? "";
                            User.Email = result["email"]?.ToString() ?? "";
                            User.ImageUrl = result["image"]?.ToString() ?? "";
                            User.Id = result["id"]?.ToString() ?? "";
                            User.Provider = result["provider"]?.ToString() ?? "";

                            if (string.IsNullOrEmpty(User.Id))
                                Dispatcher.Invoke(() => lblAlert.Content = "Invalid token, Please login again");
                        }
                    } catch (Exception) {
                        Dispatcher.Invoke(() => lblAlert.Content = "Error on getting profile");
                    }
                }

                // Download user profile image
                if (!string.IsNullOrEmpty(User.ImageUrl) && User.Image == null)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var response = await client.GetAsync(User.ImageUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                byte[] imageBytes = response.Content.ReadAsByteArrayAsync().Result;
                                Dispatcher.Invoke(() =>
                                {
                                    User.Image = new BitmapImage();
                                    User.Image.BeginInit();
                                    User.Image.StreamSource = new System.IO.MemoryStream(imageBytes);
                                    User.Image.CacheOption = BitmapCacheOption.OnLoad;
                                    User.Image.EndInit();
                                    imgUser.ImageSource = User.Image;
                                });
                            }
                        }
                    } catch (Exception)
                    {
                        lblAlert.Content = "Error on downloading profile";
                    }
                }

                // Get memos
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler) { BaseAddress = memoBaseAddress })
                {
                    try
                    {
                        var response = await client.GetAsync("/api/memos");
                        if (response.IsSuccessStatusCode)
                        {
                            JArray result = JArray.Parse(await response.Content.ReadAsStringAsync());
                            Dispatcher.Invoke(() =>
                            {
                                Memos.Clear();
                                foreach (var item in result.Children())
                                    Memos.Add(new Memo(
                                        item["id"]?.ToObject<int>() ?? 0,
                                        item["userId"]?.ToString() ?? "",
                                        item["title"]?.ToString() ?? "",
                                        item["content"]?.ToString() ?? "",
                                        item["color"]?.ToString() ?? "",
                                        item["checkbox"]?.ToObject<bool>() ?? false,
                                        item["updatedAt"]?.ToString() ?? ""));
                            });
                        }
                    } catch(Exception)
                    {
                        lblAlert.Content = "Error on getting memos";
                    }
                }
            }
        }
    }
}
