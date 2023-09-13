using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
        public string? Delete
        {
            get { return _delete?.ToString("O"); }
            set {
                try { _delete = value == null ? null : DateTime.Parse(value); }
                catch { _delete = null; }
            }
        }

        [JsonIgnore]
        public string Key { get; set; }
        [JsonIgnore]
        public HashSet<int>? _checked { get; set; }
        [JsonIgnore]
        public DateTime? _delete { get; set; }
        [JsonIgnore]
        public string BackgroundColor
        {
            get { return ColorMap.Background(Color); }
        }
        [JsonIgnore]
        public string BorderColor
        {
            get { return ColorMap.Border(Color); }
        }

        public static Memo Empty => new();

        public Memo()
        {
            Title = "";
            Content = "";
            Color = "clear";
            Archive = false;
            Checked = null;
            Delete = null;

            Key = "";
        }

        public bool Equals(Memo? other)
        {
            if (other == null) return false;

            return Title == other.Title &&
                Content == other.Content &&
                Color == other.Color &&
                Archive == other.Archive &&
                ((_checked == null && other._checked == null) ||
                (_checked!.SetEquals(other._checked!))) &&
                _delete == other._delete;
        }
    }
}
