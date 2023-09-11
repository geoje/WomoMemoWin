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

        public static string GetDataPath(string folder)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory(Path.Combine(folderPath, folder));
            return Path.Combine(folderPath, folder, CONFIG_FILENAME);
        }
        public static void Load()
        {
            if (!File.Exists(GetDataPath(CONFIG_FILENAME))) return;

            JObject config = JObject.Parse(File.ReadAllText(GetDataPath(CONFIG_FILENAME)));
            OpenedMemos = config["OpenedMemos"]?.ToObject<List<JObject>>() ?? OpenedMemos;
        }
        public static void Save()
        {
            string path = GetDataPath(CONFIG_FILENAME);
            if (!Directory.Exists(path)) Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");

            File.WriteAllTextAsync(GetDataPath(CONFIG_FILENAME), JsonConvert.SerializeObject(new JObject
            {
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