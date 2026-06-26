namespace BlogAtor.Server.Config
{
	public class RedditConfig
	{
		public string ClientId { get; set; } = string.Empty;
		public string ClientSecret { get; set; } = string.Empty;
		public string UserAgent { get; set; } = string.Empty;
		public int RateLimitDelayMs { get; set; } = 2000;
	}
}