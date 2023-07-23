namespace WomoMemo.Models
{
    public class Memo
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Checkbox { get; set; }
        public string UpdatedAt { get; set; }
        private string color;
        public string Color {
            get { return color; }
            set
            {
                this.color = value;
            }
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
