using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Auth.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WomoMemo.Models;
using WomoMemo.Views;

namespace WomoMemo
{
    public partial class App : Application
    {
        readonly static string FIREBASE_API_KEY = "AIzaSyBegDS_abY3Jl9FsldvKR2sP_YpSkzobjc";
        readonly static string FIREBASE_AUTH_DOMAIN = "womoso.firebaseapp.com";
        readonly static string FIREBASE_PRIVACY_POLICY_URL = "";
        readonly static string FIREBASE_TERMS_IF_SERVICE_URL = "";

        public static MainWindow? MainWin;
        public static Dictionary<string, MemoWindow> MemoWins = new Dictionary<string, MemoWindow>();
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

            // Init Firebase Authentication
            FirebaseUI.Initialize(new FirebaseUIConfig
            {
                ApiKey = FIREBASE_API_KEY,
                AuthDomain = FIREBASE_AUTH_DOMAIN,
                Providers = new FirebaseAuthProvider[] { new GoogleProvider() },
                PrivacyPolicyUrl = FIREBASE_PRIVACY_POLICY_URL,
                TermsOfServiceUrl = FIREBASE_TERMS_IF_SERVICE_URL,
                UserRepository = new FileUserRepository("WomoMemo")
            });

            //// Open all memo windows
            //Config.OpenedMemos.ForEach(jObj => {
            //    int memoId = jObj["id"]?.ToObject<int>() ?? -1;
            //    if (memoId == -1) return;

            //    Memo memo = Memo.Empty;
            //    memo.Key = memoId;
            //    MemoWindow memoWin = new MemoWindow(memo);
            //    memoWin.WindowStartupLocation = WindowStartupLocation.Manual;

            //    // Intersect window with screen
            //    Rectangle windowBounds = new Rectangle(
            //        (int)(jObj["x"]?.ToObject<double>() ?? memoWin.window.Left),
            //        (int)(jObj["y"]?.ToObject<double>() ?? memoWin.window.Top),
            //        (int)(jObj["w"]?.ToObject<double>() ?? memoWin.window.Width),
            //        (int)(jObj["h"]?.ToObject<double>() ?? memoWin.window.Height));
            //    bool intersect = false;
            //    foreach (Screen screen in Screen.AllScreens)
            //        if (screen.Bounds.IntersectsWith(windowBounds))
            //        {
            //            intersect = true;
            //            break;
            //        }
            //    if (intersect)
            //    {
            //        memoWin.window.Left = windowBounds.Left;
            //        memoWin.window.Top = windowBounds.Top;
            //        memoWin.window.Width = windowBounds.Width;
            //        memoWin.window.Height = windowBounds.Height;
            //    }

            //    memoWin.Show();
            //});

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

        public static async Task CreateNewMemo()
        {
            //if (Memos.Count > 0 && Memos[0].Id == Memo.Empty.Id) return;

            //MemoWindow memoWindow = new MemoWindow(Memo.Empty);
            //Memos.Insert(0, Memo.Empty);
            //memoWindow.Show();

            //try
            //{
            //    JObject jObj = new JObject
            //    {
            //        { "title", Memo.Empty.Title },
            //        { "content", Memo.Empty.Content },
            //        { "color", Memo.Empty.Color },
            //        { "checkBox", Memo.Empty.Checkbox },
            //    };
            //    StringContent content = new StringContent(jObj.ToString(Newtonsoft.Json.Formatting.None));

            //    var response = await Client.PostAsync("/api/memos", content);
            //    response.EnsureSuccessStatusCode();
            //    JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

            //    memoWindow.Memo.Id = result["id"]?.ToObject<int>() ?? Memo.Empty.Id;
            //    if (memoWindow.Memo.Id == Memo.Empty.Id) throw new Exception();

            //    MemoWins.Add(memoWindow.Memo.Id, memoWindow);
            //}
            //catch (Exception ex)
            //{
            //    if (Memos.Count > 0 && Memos[0].Id == Memo.Empty.Id) Memos.RemoveAt(0);
            //    memoWindow.Close();
            //    Trace.TraceError(ex.ToString());
            //    MainWin?.ShowAlert("Error on posting memo");
            //}
        }
    }
}
