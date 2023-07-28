using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WomoMemo.Models
{
    public class Config
    {
        public readonly static string GITHUB_URL = "https://github.com/geoje/WomoMemoWin";
#if DEBUG
        public readonly static string MemoUrl = "http://localhost:3000";
        public readonly static string AuthUrl = "http://localhost:3001";
        public readonly static string SessionTokenName = "next-auth.session-token";
#else
        public readonly static string MemoUrl = "https://memo.womosoft.com";
        public readonly static string AuthUrl = "https://www.womosoft.com";
        public readonly static string SessionTokenName = "__Secure-next-auth.session-token";
#endif

        public static string SessionTokenValue = string.Empty;
        public static List<JObject> OpenedMemos = new List<JObject>();

        public static string GetDataPath(params string[] relativePaths)
        {
            string appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "WomoMemo";
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,
                Path.Combine(relativePaths));
        }
        public static void Load()
        {
            if (!File.Exists(GetDataPath("config.json"))) return;

            JObject config = JObject.Parse(File.ReadAllText(GetDataPath("config.json")));
            SessionTokenValue = config[SessionTokenName]?.ToString() ?? string.Empty;
            OpenedMemos = config["OpenedMemos"]?.ToObject<List<JObject>>() ?? OpenedMemos;
        }
        public static void Save()
        {
            string path = GetDataPath("config.json");
            if (!Directory.Exists(path)) Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");

            File.WriteAllTextAsync(GetDataPath("config.json"), JsonConvert.SerializeObject(new JObject
            {
                { SessionTokenName, SessionTokenValue },
                { "OpenedMemos", new JArray(App.MemoWins.Values.Select(memoWin => JObject.FromObject(new {
                    id = memoWin.Memo.Id,
                    x = memoWin.window.Left,
                    y = memoWin.window.Top,
                    w = memoWin.window.Width,
                    h = memoWin.window.Height
                }))) }
            })); ;
        }
    }
}