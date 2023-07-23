using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace WomoMemo.Models
{
    public class Config
    {
#if DEBUG
        public static string MemoUrl = "http://localhost:3000";
        public static string AuthUrl = "http://localhost:3001";
        public static string SessionTokenName = "next-auth.session-token";
#else
        public static string MemoUrl = "https://memo.womosoft.com";
        public static string AuthUrl = "https://www.womosoft.com";
        public static string SessionTokenName = "__Secure-next-auth.session-token";
#endif

        public static string SessionTokenValue = string.Empty;

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
            SessionTokenValue = config["sessionToken"]?.ToString() ?? string.Empty;
        }
        public static void Save()
        {
            string path = GetDataPath("config.json");
            if (!Directory.Exists(path)) Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");

            File.WriteAllTextAsync(GetDataPath("config.json"), JsonConvert.SerializeObject(new JObject
            {
                { "sessionToken", SessionTokenValue },
            }));
        }
    }
}