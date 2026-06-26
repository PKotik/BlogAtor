using System.Text.Json.Serialization;

namespace BlogAtor.Server.Models
{
    public class PushshiftResponse
    {
        [JsonPropertyName("data")]
        public List<PushshiftPost> Data { get; set; } = new();
    }

    public class PushshiftPost
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("selftext")]
        public string SelfText { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int? Score { get; set; }

        [JsonPropertyName("num_comments")]
        public int? NumComments { get; set; }

        [JsonPropertyName("created_utc")]
        public long? CreatedUtc { get; set; }

        [JsonPropertyName("subreddit")]
        public string Subreddit { get; set; } = string.Empty;
    }
}