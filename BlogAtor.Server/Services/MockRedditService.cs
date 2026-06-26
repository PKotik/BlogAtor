using BlogAtor.Server.Models;
using BlogAtor.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogAtor.Server.Services
{
    public class MockRedditService : IRedditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MockRedditService> _logger;

        public MockRedditService(ApplicationDbContext context, ILogger<MockRedditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<RedditPost>> GetHotPostsFromSubredditAsync(string subreddit, int limit = 25)
        {
            _logger.LogInformation("Используем МОК-данные для r/{Subreddit}", subreddit);

            var posts = new List<RedditPost>();

            for (int i = 0; i < Math.Min(limit, 10); i++)
            {
                posts.Add(new RedditPost
                {
                    PostId = $"mock_{i}_{DateTime.UtcNow.Ticks}",
                    Subreddit = subreddit,
                    Author = $"user_{i + 1}",
                    Title = $"Тестовый пост #{i + 1} из r/{subreddit}",
                    Content = $"Это тестовое содержимое поста #{i + 1}. Reddit API временно недоступен, поэтому используются мок-данные.",
                    Url = $"https://reddit.com/r/{subreddit}/mock_{i}",
                    Score = 100 - i * 10,
                    CommentCount = 50 - i * 5,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5),
                    CollectedAt = DateTime.UtcNow,
                    Source = "Mock (Reddit временно недоступен)"
                });
            }

            _logger.LogInformation("Сгенерировано {Count} мок-постов", posts.Count);
            return await Task.FromResult(posts);
        }

        public async Task<List<RedditPost>> GetPostsByAuthorAsync(string author, int limit = 25)
        {
            var posts = new List<RedditPost>();

            for (int i = 0; i < Math.Min(limit, 5); i++)
            {
                posts.Add(new RedditPost
                {
                    PostId = $"mock_{i}_{DateTime.UtcNow.Ticks}",
                    Subreddit = "dotnet",
                    Author = author,
                    Title = $"Тестовый пост автора {author} #{i + 1}",
                    Content = "Тестовое содержимое.",
                    Url = $"https://reddit.com/u/{author}/mock_{i}",
                    Score = 50 - i * 5,
                    CommentCount = 20 - i * 2,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i * 10),
                    CollectedAt = DateTime.UtcNow,
                    Source = "Mock"
                });
            }

            return await Task.FromResult(posts);
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
                    existing.Title = post.Title;
                    existing.Content = post.Content;
                    existing.Score = post.Score;
                    existing.CommentCount = post.CommentCount;
                    existing.CollectedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("💾 Сохранено {Count} мок-постов в БД", posts.Count);
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