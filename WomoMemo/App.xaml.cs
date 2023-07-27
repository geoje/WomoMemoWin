using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
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
            MemoTask = Task.Run(UpdateDataFromServer);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            MemoTask?.Dispose();
            base.OnExit(e);
        }

        public static int[] GetCurrentVersion()
        {
            Version currentVerObj = typeof(MainWindow).Assembly.GetName().Version ?? Version.Parse("1.0.0");
            return new int[3] { currentVerObj.Major, currentVerObj.Minor, currentVerObj.Build };
        }
        public static async Task<int[]> GetLatestVersion()
        {
            try
            {
                using (var client = new HttpClient() { BaseAddress = new Uri(Config.GITHUB_URL + "/releases/latest") })
                {
                    var response = await client.GetAsync(User.ImageUrl);
                    response.EnsureSuccessStatusCode();

                    string latestVerStr = Regex.Match(await response.Content.ReadAsStringAsync(), @"(?<=Release )(\d|\.)+").Value;
                    if (string.IsNullOrEmpty(latestVerStr)) latestVerStr = "1.0.0";
                    int[] latestVer = latestVerStr.Split('.').Select(s => int.Parse(s)).ToArray();
                    return latestVer;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                MainWin?.ShowAlert("Error on getting latest version");
            }
            return new int[] { 1, 0, 0 };
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

                if (!string.IsNullOrEmpty(User.Id))
                {
                    User.Name = result["name"]?.ToString() ?? "";
                    User.Email = result["email"]?.ToString() ?? "";
                    User.ImageUrl = result["image"]?.ToString() ?? "";
                    User.Provider = result["provider"]?.ToString() ?? "";
                    MainWin?.UpdateControls();
                }
                else
                    MainWin?.ShowAlert("Invalid token. Please login again");
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                MainWin?.ShowAlert("Error on getting profile");
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
                MainWin?.ShowAlert("Error on downloading user profile");
            }
        }
        public static async Task CreateNewMemo()
        {
            if (Memos.Count > 0 && Memos[0].Id == Memo.Empty.Id) return;

            MemoWindow memoWindow = new MemoWindow(Memo.Empty);
            Memos.Insert(0, Memo.Empty);
            memoWindow.Show();

            try
            {
                JObject jObj = new JObject
                {
                    { "title", Memo.Empty.Title },
                    { "content", Memo.Empty.Content },
                    { "color", Memo.Empty.Color },
                    { "checkBox", Memo.Empty.Checkbox },
                };
                StringContent content = new StringContent(jObj.ToString(Newtonsoft.Json.Formatting.None));

                var response = await Client.PostAsync("/api/memos", content);
                response.EnsureSuccessStatusCode();
                JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

                memoWindow.Memo.Id = result["id"]?.ToObject<int>() ?? Memo.Empty.Id;
                if (memoWindow.Memo.Id == Memo.Empty.Id) throw new Exception();

                MemoWins.Add(memoWindow.Memo.Id, memoWindow);
            }
            catch (Exception ex)
            {
                if (Memos.Count > 0 && Memos[0].Id == Memo.Empty.Id) Memos.RemoveAt(0);
                memoWindow.Close();
                Trace.TraceError(ex.ToString());
                MainWin?.ShowAlert("Error on posting memo");
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

                    // Update memos
                    Dispatcher.Invoke(() =>
                    {
                        ArrayList ExistsMemos = new ArrayList();

                        Memos.Clear();
                        foreach (var item in result.Children())
                        {
                            int memoId = item["id"]?.ToObject<int>() ?? Memo.Empty.Id;
                            if (memoId == Memo.Empty.Id) continue;
                            bool isMemoWinValid = MemoWins.ContainsKey(memoId) && MemoWins[memoId] != null;
                            bool isMemoWinUpdating = isMemoWinValid && MemoWins[memoId].PutMemoTimer != null;

                            // Add memo from server or local
                            var memo =
                            isMemoWinValid && isMemoWinUpdating ?
                            MemoWins[memoId].Memo :
                            new Memo(memoId,
                            item["userId"]?.ToString() ?? Memo.Empty.UserId,
                            item["title"]?.ToString() ?? Memo.Empty.Title,
                            item["content"]?.ToString() ?? Memo.Empty.Content,
                            item["color"]?.ToString() ?? Memo.Empty.Color,
                            item["checkbox"]?.ToObject<bool>() ?? Memo.Empty.Checkbox,
                            item["updatedAt"]?.ToString() ?? Memo.Empty.UpdatedAt);
                            Memos.Add(memo);
                            ExistsMemos.Add(memoId);
                            if (isMemoWinValid && !isMemoWinUpdating)
                                MemoWins[memoId].UpdateMemo(memo);
                        }

                        // Close and delete memo
                        foreach (int key in MemoWins.Keys.Where(key => !ExistsMemos.Contains(key)))
                        {
                            MemoWins[key].Close();
                            MemoWins.Remove(key);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                    MainWin?.ShowAlert("Error on getting memos");
                }

                // Update Controls
                MainWin?.UpdateControls();
            }
        }
    }
}
