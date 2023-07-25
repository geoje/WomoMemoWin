namespace WomoMemo.Models
{
    public class Memo
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        private string color = "";
        public string Color
        {
            set { color = value; }
            get {
                return
                    color == "white" ? "#FFF" :
                    color == "gray" ? "#EDF2F7" :
                    color == "red" ? "#FED7D7" :
                    color == "orange" ? "#FEEBC8" :
                    color == "yellow" ? "#FEFCBF" :
                    color == "green" ? "#C6F6D5" :
                    color == "teal" ? "#B2F5EA" :
                    color == "blue" ? "#BEE3F8" :
                    color == "cyan" ? "#C4F1F9" :
                    color == "purple" ? "#E9D8FD" :
                    color == "pink" ? "#FED7E2" : "#FFF";
            }
        }
        public string BorderColor
        {
            get { return color == "white" ? "#CBD5E0" : Color; }
        }
        public bool Checkbox { get; set; }
        public string UpdatedAt { get; set; }

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
