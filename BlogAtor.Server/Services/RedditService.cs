using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BlogAtor.Server.Models;
using BlogAtor.Server.Config;
using BlogAtor.Server.Data;

namespace BlogAtor.Server.Services
{
    public class RedditService : IRedditService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RedditService> _logger;

        public RedditService(
            HttpClient httpClient,
            ApplicationDbContext context,
            ILogger<RedditService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
        }

        public async Task<List<RedditPost>> GetHotPostsFromSubredditAsync(string subreddit, int limit = 25)
        {
            var posts = new List<RedditPost>();

            try
            {
                _logger.LogInformation("Начат сбор постов из r/{Subreddit}", subreddit);

                // ✅ Используем RePull (работает без блокировок)
                var url = $"https://api.repull.io/reddit/search/submission/?subreddit={subreddit}&limit={limit}";

                _logger.LogInformation("Запрос к URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                _logger.LogInformation("Статус ответа: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка API: {StatusCode}, Content: {Content}",
                        response.StatusCode, errorContent);
                    throw new Exception($"API вернул {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Для RePull структура ответа такая же как у Pushshift
                var pushshiftResponse = JsonSerializer.Deserialize<PushshiftResponse>(json, options);

                if (pushshiftResponse?.Data == null || !pushshiftResponse.Data.Any())
                {
                    _logger.LogWarning("Нет данных в ответе");
                    return posts;
                }

                foreach (var postData in pushshiftResponse.Data)
                {
                    posts.Add(new RedditPost
                    {
                        PostId = postData.Id,
                        Subreddit = postData.Subreddit ?? subreddit,
                        Author = postData.Author ?? "unknown",
                        Title = postData.Title ?? "No title",
                        Content = postData.SelfText ?? string.Empty,
                        Url = postData.Url ?? $"https://reddit.com/r/{subreddit}/comments/{postData.Id}",
                        Score = postData.Score ?? 0,
                        CommentCount = postData.NumComments ?? 0,
                        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(postData.CreatedUtc ?? 0).UtcDateTime,
                        CollectedAt = DateTime.UtcNow,
                        Source = "Reddit(Pushshift)"
                    });
                }

                _logger.LogInformation("Получено {Count} постов из r/{Subreddit}", posts.Count, subreddit);
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

                var url = $"https://api.repull.io/reddit/search/submission/?author={author}&limit={limit}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка API: {StatusCode}", response.StatusCode);
                    throw new Exception($"API вернул {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var pushshiftResponse = JsonSerializer.Deserialize<PushshiftResponse>(json, options);

                if (pushshiftResponse?.Data == null)
                {
                    _logger.LogWarning("Нет данных в ответе");
                    return posts;
                }

                foreach (var postData in pushshiftResponse.Data)
                {
                    posts.Add(new RedditPost
                    {
                        PostId = postData.Id,
                        Subreddit = postData.Subreddit ?? "unknown",
                        Author = postData.Author ?? author,
                        Title = postData.Title ?? "No title",
                        Content = postData.SelfText ?? string.Empty,
                        Url = postData.Url ?? $"https://reddit.com/r/{postData.Subreddit}/comments/{postData.Id}",
                        Score = postData.Score ?? 0,
                        CommentCount = postData.NumComments ?? 0,
                        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(postData.CreatedUtc ?? 0).UtcDateTime,
                        CollectedAt = DateTime.UtcNow,
                        Source = "Reddit(Pushshift)"
                    });
                }

                _logger.LogInformation("Найдено {Count} постов автора {Author}", posts.Count, author);
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