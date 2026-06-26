using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Reddit;
using Reddit.Controllers;
using BlogAtor.Server.Models;
using BlogAtor.Server.Config;
using BlogAtor.Server.Data;

namespace BlogAtor.Server.Services
{
    public class RedditService : IRedditService
    {
        private readonly RedditClient _reddit;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RedditService> _logger;
        private readonly RedditConfig _config;

        public RedditService(
            IOptions<RedditConfig> config,
            ApplicationDbContext context,
            ILogger<RedditService> logger)
        {
            _config = config.Value;
            _context = context;
            _logger = logger;

            _reddit = new RedditClient(
                appId: _config.ClientId,
                appSecret: _config.ClientSecret,
                userAgent: _config.UserAgent
            );
        }

        public async Task<List<RedditPost>> GetHotPostsFromSubredditAsync(string subreddit, int limit = 25)
        {
            var posts = new List<RedditPost>();

            try
            {
                _logger.LogInformation("Начат сбор постов из r/{Subreddit}", subreddit);

                var sub = _reddit.Subreddit(subreddit);
                var hotPosts = sub.Posts.Hot.Take(limit);

                foreach (var post in hotPosts)
                {
                    posts.Add(new RedditPost
                    {
                        PostId = post.Id,
                        Subreddit = subreddit,
                        Author = post.Author ?? "unknown",
                        Title = post.Title ?? "No title",
                        Content = post.Listing.SelfText ?? string.Empty,  // ✅ Через Listing
                        Url = $"https://reddit.com{post.Permalink}",
                        Score = post.Score,
                        CommentCount = post.Listing.NumComments,          // ✅ Через Listing
                        CreatedAt = post.Created,
                        CollectedAt = DateTime.UtcNow,
                        Source = "Reddit"
                    });
                }

                _logger.LogInformation("Получено {Count} постов из r/{Subreddit}", posts.Count, subreddit);
                await Task.Delay(_config.RateLimitDelayMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении постов из r/{Subreddit}", subreddit);
                throw;
            }

            return posts;
        }

        public async Task<List<RedditPost>> GetPostsByAuthorAsync(string author, int limit = 25)
        {
            var posts = new List<RedditPost>();

            try
            {
                _logger.LogInformation("Поиск постов автора {Author}", author);

                var searchResults = _reddit.Search($"author:\"{author}\"", limit: limit);

                foreach (var post in searchResults)
                {
                    posts.Add(new RedditPost
                    {
                        PostId = post.Id,
                        Subreddit = post.Subreddit ?? "unknown",
                        Author = post.Author ?? author,
                        Title = post.Title ?? "No title",
                        Content = post.Listing.SelfText ?? string.Empty,  // ✅ Через Listing
                        Url = $"https://reddit.com{post.Permalink}",
                        Score = post.Score,
                        CommentCount = post.Listing.NumComments,          // ✅ Через Listing
                        CreatedAt = post.Created,
                        CollectedAt = DateTime.UtcNow,
                        Source = "Reddit"
                    });
                }

                await Task.Delay(_config.RateLimitDelayMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске постов автора {Author}", author);
                throw;
            }

            return posts;
        }

        public async Task SavePostsAsync(List<RedditPost> posts)
        {
            foreach (var post in posts)
            {
                var existing = await _context.RedditPosts
                    .FirstOrDefaultAsync(p => p.PostId == post.PostId);

                if (existing == null)
                {
                    await _context.RedditPosts.AddAsync(post);
                }
                else
                {
                    existing.Score = post.Score;
                    existing.CommentCount = post.CommentCount;
                    existing.CollectedAt = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(post.Content))
                        existing.Content = post.Content;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Сохранено {Count} постов в БД", posts.Count);
        }

        public async Task<IQueryable<RedditPost>> GetAllPostsAsync()
        {
            return await Task.FromResult(_context.RedditPosts.AsQueryable());
        }

        public async Task<RedditPost?> GetPostByIdAsync(int id)
        {
            return await _context.RedditPosts.FindAsync(id);
        }
    }
}