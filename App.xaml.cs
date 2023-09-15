using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Auth.UI;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WomoMemo.Models;
using WomoMemo.Views;

namespace WomoMemo
{
    public partial class App : System.Windows.Application
    {
        readonly static string FIREBASE_API_KEY = "AIzaSyBegDS_abY3Jl9FsldvKR2sP_YpSkzobjc";
        readonly static string FIREBASE_AUTH_DOMAIN = "womoso.firebaseapp.com";
        readonly static string FIREBASE_PRIVACY_POLICY_URL = "https://www.womosoft.com/privacy_policy";
        readonly static string FIREBASE_TERMS_OF_SERVICE_URL = "https://www.womosoft.com/terms_of_service";
        readonly static string FIREBASE_DB_URL = "https://womoso-default-rtdb.firebaseio.com";
        public readonly static string APP_NAME = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "WomoMemo";

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
            bool ExistsRemovableMemo = false;
            Config.Load();
            Config.OpenedMemos.ForEach(jObj =>
            {
                string? memoKey = jObj["key"]?.ToObject<string>();
                if (string.IsNullOrEmpty(memoKey))
                {
                    ExistsRemovableMemo = true;
                    return;
                }

                MemoWindow memoWin = new MemoWindow(Memos.ContainsKey(memoKey) ? Memos[memoKey] : new Memo(memoKey));
                MemoWins.Add(memoKey, memoWin);
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
            if (ExistsRemovableMemo) Config.Save();

            // Open main window if there is no opened memo
            if (Config.OpenedMemos.Count == 0) (MainWin = new MainWindow()).Show();

            // Startup registry
#if DEBUG
#else
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key?.SetValue(APP_NAME, Process.GetCurrentProcess().MainModule?.FileName ?? "");
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

                Dispatcher.Invoke(() =>
                {
                    foreach (var memoWin in MemoWins.Values)
                        memoWin.Close();
                    if (MainWin == null) MainWin = new MainWindow();
                    MainWin.Show();
                });
            }
            else
            {
                firebase = new FirebaseClient(
                    FIREBASE_DB_URL,
                    new FirebaseOptions { AuthTokenAsyncFactory =
                        () => FirebaseUI.Instance.Client.User.GetIdTokenAsync() });
                Observable = firebase
                  .Child("memos")
                  .Child(e.User.Uid)
                  .AsObservable<Memo>()
                  .Subscribe(MemoUpdated, onError: HandleSubscribeError);
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
                MainWin.UpdateMemosFromAppByView();

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
            Debug.WriteLine(ex.ToString(), "[HandleSubscribeError]");
            FirebaseUI.Instance.Client.SignOut();
            if (MainWin != null)
                MainWin.ShowAlert("[HandleSubscribeError]\n" + ex.Message);
        }

        public static void DockMemoWins()
        {
            if (Config.Dock != "Left" && Config.Dock != "Right") return;

            int SIZE = 300, GAP = 8;
            Rectangle winRect = new Rectangle(Config.Dock == "Left" ? GAP : Screen.PrimaryScreen.Bounds.Width - SIZE - GAP, GAP, SIZE, SIZE);
            foreach (MemoWindow memoWin in MemoWins.Values)
            {
                memoWin.Width = memoWin.Height = SIZE;
                memoWin.Left = winRect.X;
                memoWin.Top = winRect.Y;

                winRect.Y += SIZE + GAP;
                if (!Screen.PrimaryScreen.Bounds.Contains(winRect))
                {
                    winRect.X += Config.Dock == "Left" ? SIZE + GAP : -SIZE - GAP;
                    winRect.Y = GAP;
                }
            }
        }
        public static async Task<string> CreateMemo(MemoWindow memoWin)
        {
            if (firebase == null) return "";

            FirebaseObject<Memo> e = await firebase
                .Child("memos")
                .Child(FirebaseUI.Instance.Client.User.Uid)
                .PostAsync(memoWin.Memo);

            memoWin.Memo.Key = e.Key;
            Memos.Add(e.Key, memoWin.Memo);
            MemoWins.Add(e.Key, memoWin);
            if (MainWin != null) MainWin.UpdateMemosFromAppByView();

            return e.Key;
        }
        public static async Task UpdateMemo(Memo memo)
        {
            if (firebase == null || string.IsNullOrEmpty(memo.Key)) return;

            await firebase
                .Child("memos")
                .Child(FirebaseUI.Instance.Client.User.Uid)
                .Child(memo.Key)
                .PutAsync(memo);
        }
        public static async Task DeleteMemo(string key)
        {
            if (firebase == null || string.IsNullOrEmpty(key)) return;

            await firebase
                .Child("memos")
                .Child(FirebaseUI.Instance.Client.User.Uid)
                .Child(key)
                .DeleteAsync();
        }
    }
}
