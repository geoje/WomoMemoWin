using System;

namespace WomoMemo.Models
{
    public class Memo
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Color { get; set; }
        public string BgColor
        {
            get {
                return
                    Color == "white" ? "#FFF" :
                    Color == "gray" ? "#EDF2F7" :
                    Color == "red" ? "#FED7D7" :
                    Color == "orange" ? "#FEEBC8" :
                    Color == "yellow" ? "#FEFCBF" :
                    Color == "green" ? "#C6F6D5" :
                    Color == "teal" ? "#B2F5EA" :
                    Color == "blue" ? "#BEE3F8" :
                    Color == "cyan" ? "#C4F1F9" :
                    Color == "purple" ? "#E9D8FD" :
                    Color == "pink" ? "#FED7E2" : "#FFF";
            }
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
