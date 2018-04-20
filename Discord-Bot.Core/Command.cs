namespace Discord_Bot
{
    public class Command
    {
        public string Text { get; }
        public string Author { get; }

        public Command(string text, string author)
        {
            Text = text;
            Author = author;
        }
    }
}
