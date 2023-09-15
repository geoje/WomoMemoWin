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
        readonly static string CONFIG_FILENAME = "config.json";

        public static List<JObject> OpenedMemos = new List<JObject>();
        public static string Dock = ""; // Left | Right

        public static string GetDataPath()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory(Path.Combine(folderPath, App.APP_NAME));
            return Path.Combine(folderPath, App.APP_NAME, CONFIG_FILENAME);
        }
        public static void Load()
        {
            if (!File.Exists(GetDataPath())) return;

            JObject config = JObject.Parse(File.ReadAllText(GetDataPath()));
            Dock = config["Dock"]?.ToObject<string>() ?? Dock;
            OpenedMemos = config["OpenedMemos"]?.ToObject<List<JObject>>() ?? OpenedMemos;
        }
        public static void Save()
        {
            string path = GetDataPath();
            if (!Directory.Exists(path)) Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");

            File.WriteAllTextAsync(GetDataPath(), JsonConvert.SerializeObject(new JObject
            {
                { "Dock", Dock },
                { "OpenedMemos", new JArray(App.MemoWins.Values.Select(memoWin => JObject.FromObject(new {
                    key = memoWin.Memo.Key,
                    x = memoWin.window.Left,
                    y = memoWin.window.Top,
                    w = memoWin.window.Width,
                    h = memoWin.window.Height
                }))) }
            }));
        }
    }
}