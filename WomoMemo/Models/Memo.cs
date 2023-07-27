using System;
using System.Collections.Generic;

namespace WomoMemo.Models
{
    public class Memo
    {
        public static Dictionary<string, string> COLORS = new Dictionary<string, string>()
        {
            { "white" , "#FFF" },
            { "gray" , "#EDF2F7" },
            { "red" , "#FED7D7" },
            { "orange" , "#FEEBC8" },
            { "yellow" , "#FEFCBF" },
            { "green" , "#C6F6D5" },
            { "teal" , "#B2F5EA" },
            { "blue" , "#BEE3F8" },
            { "cyan" , "#C4F1F9" },
            { "purple" , "#E9D8FD" },
            { "pink" , "#FED7E2" }
        };

        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Color { get; set; }
        public string BgColor
        {
            get { return COLORS.ContainsKey(Color) ? COLORS[Color] : COLORS["white"]; }
        }
        public string BorderColor
        {
            get { return Color == "white" ? "#CBD5E0" : BgColor; }
        }
        public bool Checkbox { get; set; }
        public string UpdatedAt { get; set; }

        public static Memo Empty
        {
            get { return new Memo(-1, "", "", "", "white", false, DateTime.UtcNow.ToString("o")); }
        }

        public Memo(int id, string userId, string title, string content, string color, bool checkbox, string updatedAt)
        {
            Id = id;
            UserId = userId;
            Title = title;
            Content = content;
            Color = color;
            Checkbox = checkbox;
            UpdatedAt = updatedAt;
        }
    }
}
