using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WomoMemo.Models;
using WomoMemo.Views;

namespace WomoMemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow? MainWin;
        public static Dictionary<int, MemoWindow> MemoWins = new Dictionary<int, MemoWindow>();

        public static HttpClientHandler Handler = new HttpClientHandler();
        public static HttpClient Client = new HttpClient(Handler) { BaseAddress = new Uri(Config.MemoUrl) };
        public static ObservableCollection<Memo> Memos = new ObservableCollection<Memo>();
        public static string ErrorMessage = "";

        public Task? MemoTask;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Execute single process
            string procName = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcesses().Where(p => p.ProcessName == procName).Count() > 1)
            {
                Current.Shutdown();
                return;
            }

            // Run parent's code
            base.OnStartup(e);

            // Get profile if token available
            Config.Load();
            if (!string.IsNullOrEmpty(Config.SessionTokenValue))
            {
                Handler.CookieContainer = new CookieContainer();
                Handler.CookieContainer.Add(new Uri(Config.MemoUrl), new Cookie(Config.SessionTokenName, Config.SessionTokenValue));
                Task.Run(async () =>
                {
                    await GetUserProfile();
                    await DownloadUserProfileImage();
                });
            }

            // Start to sync data
            Task MemoTask = Task.Run(UpdateDataFromServer);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            MemoTask?.Dispose();
            base.OnExit(e);
        }

        public static void UpdateErrorMessage(string message, bool remove = false)
        {
            ErrorMessage = remove ?
                (message == ErrorMessage || string.IsNullOrEmpty(ErrorMessage) ? "" : ErrorMessage) :
                (message);
        }
        public static async Task GetUserProfile()
        {
            // Get user profile
            try
            {
                // Request
                var response = await Client.GetAsync("/api/auth/session");
                response.EnsureSuccessStatusCode();

                // Parse json to User instance
                JToken result = JObject.Parse(await response.Content.ReadAsStringAsync())["user"] ?? JObject.Parse("{}");
                User.Id = result["id"]?.ToString() ?? "";
                UpdateErrorMessage("Error on getting profile", true);

                if (!string.IsNullOrEmpty(User.Id))
                {
                    User.Name = result["name"]?.ToString() ?? "";
                    User.Email = result["email"]?.ToString() ?? "";
                    User.ImageUrl = result["image"]?.ToString() ?? "";
                    User.Provider = result["provider"]?.ToString() ?? "";
                    MainWin?.UpdateControls();
                }
                UpdateErrorMessage("Invalid token. Please login again", !string.IsNullOrEmpty(User.Id));
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                UpdateErrorMessage("Error on getting profile");
            }
        }
        public static async Task DownloadUserProfileImage()
        {
            try
            {
                using (var client = new HttpClient() { BaseAddress = new Uri(User.ImageUrl) })
                {
                    var response = await client.GetAsync(User.ImageUrl);
                    response.EnsureSuccessStatusCode();
                    UpdateErrorMessage("Error on downloading user profile", true);

                    if (MainWin != null)
                        MainWin.Dispatcher.Invoke(() =>
                        {
                            byte[] imageBytes = response.Content.ReadAsByteArrayAsync().Result;
                            User.Image = new BitmapImage();
                            User.Image.BeginInit();
                            User.Image.StreamSource = new System.IO.MemoryStream(imageBytes);
                            User.Image.CacheOption = BitmapCacheOption.OnLoad;
                            User.Image.EndInit();
                        });

                    MainWin?.UpdateControls();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                UpdateErrorMessage("Error on downloading user profile");
            }
        }
        async void UpdateDataFromServer()
        {
            for (; ; Thread.Sleep(1000))
            {
                // Validate token
                if (string.IsNullOrEmpty(Config.SessionTokenValue)) continue;

                // Get memos
                try
                {
                    // Request
                    var response = await Client.GetAsync("/api/memos");
                    response.EnsureSuccessStatusCode();

                    // Parse json to memo array
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

                    // Clear error
                    UpdateErrorMessage("Error on getting memos", true);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                    UpdateErrorMessage("Error on getting memos");
                }

                // Update Controls
                MainWin?.UpdateControls();
            }
        }
    }
}
