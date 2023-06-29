namespace WomoMemo.Models
{
    public class Memo
    {
        public int Id { get; set; }
        public bool CanCheck { get; set; }
        public string Content { get; set; }

        public Memo(int id, bool canCheck, string content)
        {
            Id = id;
            CanCheck = canCheck;
            Content = content;
        }
    }
}
