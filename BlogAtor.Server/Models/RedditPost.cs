using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogAtor.Server.Models
{
	/// <summary>
	/// Модель поста из Reddit для хранения в БД
	/// </summary>
	public class RedditPost
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[MaxLength(50)]
		public string PostId { get; set; } = string.Empty; // ID из Reddit

		[Required]
		[MaxLength(100)]
		public string Subreddit { get; set; } = string.Empty;

		[Required]
		[MaxLength(100)]
		public string Author { get; set; } = string.Empty;

		[Required]
		[MaxLength(500)]
		public string Title { get; set; } = string.Empty;

		public string Content { get; set; } = string.Empty;

		[MaxLength(500)]
		public string Url { get; set; } = string.Empty;

		public int Score { get; set; }

		public int CommentCount { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime CollectedAt { get; set; }

		[MaxLength(50)]
		public string Source { get; set; } = "Reddit";
	}
}