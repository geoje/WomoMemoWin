using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;

namespace WomoMemo.Models
{
    public class Memo : IEquatable<Memo>
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
        [JsonProperty("archive")]
        public bool Archive { get; set; }
        [JsonProperty("checked")]
        public string? Checked
        {
            get { return _checked == null ? null : string.Join(",", _checked); }
            set
            {
                try { _checked = value == null ? null :
                        new HashSet<int>(Array.ConvertAll(value.Split(','), int.Parse)); }
                catch { _checked = new HashSet<int>(); }
            }
        }
        [JsonProperty("delete")]
        public DateTime? Delete { get; set; }

        [JsonIgnore]
        public string Key { get; set; }
        [JsonIgnore]
        public HashSet<int>? _checked { get; set; }
        [JsonIgnore]
        public string BackgroundColor { get { return ColorMap.Background(Color); } }
        [JsonIgnore]
        public string BorderColor { get { return ColorMap.Border(Color); } }
        [JsonIgnore]
        public Visibility VisibilityTitle
        {
            get { return string.IsNullOrEmpty(Title) ?
                    Visibility.Collapsed : Visibility.Visible; }
        }
        [JsonIgnore]
        public Visibility VisibilitySeparator
        {
            get
            {
                return string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Content) ?
                    Visibility.Collapsed : Visibility.Visible;
            }
        }
        [JsonIgnore]
        public Visibility VisibilityContent
        {
            get
            {
                return string.IsNullOrEmpty(Content) && !string.IsNullOrEmpty(Title) ?
                    Visibility.Collapsed : Visibility.Visible;
            }
        }

        public static Memo Empty => new("");

        public Memo(string key)
        {
            Title = "";
            Content = "";
            Color = "clear";
            Archive = false;
            Checked = null;
            Delete = null;

            Key = key;
        }

        public bool Equals(Memo? other)
        {
            if (other == null) return false;

            return Title == other.Title &&
                Content == other.Content &&
                Color == other.Color &&
                Archive == other.Archive &&
                Checked == other.Checked &&
                Delete == other.Delete;
        }
    }
}
