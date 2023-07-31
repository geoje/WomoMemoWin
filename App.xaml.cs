﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WomoMemo.Models;
using WomoMemo.Views;

namespace WomoMemo
{
    public partial class App : System.Windows.Application
    {
        public static MainWindow? MainWin;
        public static Dictionary<int, MemoWindow> MemoWins = new Dictionary<int, MemoWindow>();

        public static HttpClientHandler Handler = new HttpClientHandler();
        public static HttpClient Client = new HttpClient(Handler) { BaseAddress = new Uri(Config.MemoUrl) };
        public static ObservableCollection<Memo> Memos = new ObservableCollection<Memo>();

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

#if DEBUG
#else
            // Init registry
            string appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "WomoMemoWin";
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key?.SetValue(appName, Process.GetCurrentProcess().MainModule?.FileName ?? "");
#endif

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
            Task.Run(UpdateDataFromServer);

            // Open all memo windows
            Config.OpenedMemos.ForEach(jObj => {
                int memoId = jObj["id"]?.ToObject<int>() ?? -1;
                if (memoId == -1) return;

                Memo memo = Memo.Empty;
                memo.Id = memoId;
                MemoWindow memoWin = new MemoWindow(memo);
                memoWin.WindowStartupLocation = WindowStartupLocation.Manual;

                // Intersect window with screen
                Rectangle windowBounds = new Rectangle(
                    (int)(jObj["x"]?.ToObject<double>() ?? memoWin.window.Left),
                    (int)(jObj["y"]?.ToObject<double>() ?? memoWin.window.Top),
                    (int)(jObj["w"]?.ToObject<double>() ?? memoWin.window.Width),
                    (int)(jObj["h"]?.ToObject<double>() ?? memoWin.window.Height));
                bool intersect = false;
                foreach (Screen screen in Screen.AllScreens)
                    if (screen.Bounds.IntersectsWith(windowBounds))
                    {
                        intersect = true;
                        break;
                    }
                if (intersect)
                {
                    memoWin.window.Left = windowBounds.Left;
                    memoWin.window.Top = windowBounds.Top;
                    memoWin.window.Width = windowBounds.Width;
                    memoWin.window.Height = windowBounds.Height;
                }
                
                memoWin.Show();
            });

            // If there is no opened memo, Open main window
            if (Config.OpenedMemos.Count == 0) new MainWindow().Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
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
                            // Open Main window if there is no window for keeping process running
                            if (MainWin == null && MemoWins.Count == 1) new MainWindow().Show();

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
