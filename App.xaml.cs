using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Auth.UI;
using Firebase.Database;
using Firebase.Database.Query;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WomoMemo.Models;
using WomoMemo.Views;
using System;
using Firebase.Auth;
using Firebase.Database.Streaming;
using System.Drawing;
using System.Windows.Forms;

namespace WomoMemo
{
    public partial class App : System.Windows.Application
    {
        readonly static string FIREBASE_API_KEY = "AIzaSyBegDS_abY3Jl9FsldvKR2sP_YpSkzobjc";
        readonly static string FIREBASE_AUTH_DOMAIN = "womoso.firebaseapp.com";
        readonly static string FIREBASE_PRIVACY_POLICY_URL = "https://www.womosoft.com/privacy_policy";
        readonly static string FIREBASE_TERMS_OF_SERVICE_URL = "https://www.womosoft.com/terms_of_service";
        public readonly static string FIREBASE_DB_URL = "https://womoso-default-rtdb.firebaseio.com";
        public readonly static string APP_NAME = "WomoMemo";

        public static MainWindow? MainWin;
        public static Dictionary<string, MemoWindow> MemoWins = new Dictionary<string, MemoWindow>();
        public static Dictionary<string, Memo> Memos = new Dictionary<string, Memo>();

        static FirebaseClient? firebase;
        static IDisposable? Observable;

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

            // Init Firebase Authentication
            FirebaseUI.Initialize(new FirebaseUIConfig
            {
                ApiKey = FIREBASE_API_KEY,
                AuthDomain = FIREBASE_AUTH_DOMAIN,
                Providers = new FirebaseAuthProvider[] { new GoogleProvider() },
                PrivacyPolicyUrl = FIREBASE_PRIVACY_POLICY_URL,
                TermsOfServiceUrl = FIREBASE_TERMS_OF_SERVICE_URL,
                UserRepository = new FileUserRepository(APP_NAME)
            });
            FirebaseUI.Instance.Client.AuthStateChanged += AuthStateChanged;

            // Open all memo windows
            Config.Load();
            Config.OpenedMemos.ForEach(jObj =>
            {
                string? memoKey = jObj["key"]?.ToObject<string>();
                if (string.IsNullOrEmpty(memoKey)) return;

                Memo memo = Memo.Empty;
                memo.Key = memoKey;
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

            // Open main window if there is no opened memo
            if (Config.OpenedMemos.Count == 0) (MainWin = new MainWindow()).Show();

            // Startup registry
#if DEBUG
#else
            string appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "WomoMemoWin";
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key?.SetValue(appName, Process.GetCurrentProcess().MainModule?.FileName ?? "");
#endif
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void AuthStateChanged(object? sender, UserEventArgs e)
        {
            if (MainWin != null) MainWin.UpdateUser(e.User);

            if (e.User == null)
            {
                Observable?.Dispose();
                Observable = null;
                firebase?.Dispose();
                firebase = null;
            }
            else
            {
                firebase = new FirebaseClient(
                    FIREBASE_DB_URL,
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(
                            FirebaseUI.Instance.Client.User.Credential.IdToken)
                    });
                Observable = firebase
                  .Child("memos")
                  .Child(e.User.Uid)
                  .AsObservable<Memo>()
                  .Subscribe(MemoUpdated, HandleSubscribeError);
            }
        }
        private void MemoUpdated(FirebaseEvent<Memo> e)
        {
            // Check if the memo need to update
            Memo memo = e.Object;
            memo.Key = e.Key;
            if (Memos.ContainsKey(e.Key))
            {
                if (e.EventType == FirebaseEventType.Delete)
                    Memos.Remove(e.Key);
            }
            else
            {
                if (e.EventType == FirebaseEventType.InsertOrUpdate)
                    Memos.Add(e.Key, memo);
            }

            // Update main window
            if (MainWin != null)
                MainWin.UpdateMemos(Memos.Values);

            // Update memo windows
            if (MemoWins.ContainsKey(e.Key))
            {
                if (e.EventType == FirebaseEventType.InsertOrUpdate)
                    MemoWins[e.Key].UpdateMemo(memo);
                else if (e.EventType == FirebaseEventType.Delete)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MemoWins[e.Key].Close();
                        MemoWins.Remove(e.Key);
                    });
                    Config.Save();
                }
            }
        }
        private void HandleSubscribeError(Exception ex)
        {
            if (MainWin != null)
            {
                MainWin.ShowAlert("[HandleSubscribeError]\n" + ex.Message);
            }
        }

        public static async Task<string> CreateMemo()
        {
            if (firebase == null) return "";

            FirebaseObject<Memo> e = await firebase
                .Child("memos")
                .Child(FirebaseUI.Instance.Client.User.Uid)
                .PostAsync(Memo.Empty);

            MemoWindow memoWin = new MemoWindow(Memo.Empty);
            memoWin.Show();
            Memos.Add(e.Key, Memo.Empty);
            MemoWins.Add(e.Key, memoWin);

            return e.Key;
        }
        public static async Task UpdateMemo(Memo memo)
        {
            if (firebase == null) return;

            await firebase
                .Child("memos")
                .Child(FirebaseUI.Instance.Client.User.Uid)
                .Child(memo.Key)
                .PutAsync(Memo.Empty);
        }
        public static async Task DeleteMemo(string key)
        {
            if (firebase == null) return;

            await firebase
                .Child("memos")
                .Child(FirebaseUI.Instance.Client.User.Uid)
                .Child(key)
                .DeleteAsync();

            Memos.Remove(key);
            MemoWins.Remove(key);
        }
    }
}
