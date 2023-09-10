using System;
using System.Collections.Generic;

namespace WomoMemo.Models
{
    public class Memo
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Color { get; set; }
        public bool Archive { get; set; }
        public HashSet<int>? Checked { get; set; }
        public DateTime? Delete { get; set; }

        public static Memo Empty
        {
            get { return new Memo("", "", "", "clear", false, null, null); }
        }

        public Memo(string key, string title, string content, string color, bool archive, HashSet<int>? @checked, DateTime? delete)
        {
            Key = key;
            Title = title;
            Content = content;
            Color = color;
            Archive = archive;
            Checked = @checked;
            Delete = delete;
        }
    }
}
