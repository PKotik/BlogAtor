using System.Text.Json.Serialization;

namespace BlogAtor.Server.Models
{
    public class RedditApiResponse
    {
        [JsonPropertyName("data")]
        public RedditData Data { get; set; } = new();
    }

    public class RedditData
    {
        [JsonPropertyName("children")]
        public List<RedditChild> Children { get; set; } = new();
    }

    public class RedditChild
    {
        [JsonPropertyName("data")]
        public RedditPostData Data { get; set; } = new();
    }

    public class RedditPostData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("selftext")]
        public string SelfText { get; set; } = string.Empty;

        [JsonPropertyName("permalink")]
        public string Permalink { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("num_comments")]
        public int NumComments { get; set; }

        [JsonPropertyName("created_utc")]
        public double CreatedUtc { get; set; }

        [JsonPropertyName("subreddit")]
        public string Subreddit { get; set; } = string.Empty;
    }
}