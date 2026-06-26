namespace BlogAtor.Server.DTOs
{
    public class CollectRequest
    {
        public string Subreddit { get; set; } = string.Empty;
        public int? Limit { get; set; } = 25;
    }
}