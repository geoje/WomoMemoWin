using System;
using System.Collections.Generic;

namespace WomoMemo.Models
{
    public class Memo : IEquatable<Memo>
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Color { get; set; }
        public bool Archive { get; set; }
        public HashSet<int>? Checked { get; set; }
        public DateTime? Delete { get; set; }

        public string BackgroundColor
        {
            get { return ColorMap.Background(Color); }
        }
        public string BorderColor
        {
            get { return ColorMap.Border(Color); }
        }

        public static Memo Empty => new("", "", "", "clear", false, null, null);
        public Memo(string key, string title, string content, string color, bool archive, string? @checked, string? delete)
        {
            Key = key;
            Title = title;
            Content = content;
            Color = color;
            Archive = archive;
            //Checked = @checked;
            //Delete = delete;
        }

        public bool Equals(Memo? other)
        {
            if (other == null) return false;

            return Title == other.Title &&
                Content == other.Content &&
                Color == other.Color &&
                Archive == other.Archive &&
                ((Checked == null && other.Checked == null) ||
                (Checked!.SetEquals(other.Checked!))) &&
                Delete == other.Delete;
        }
    }
}
