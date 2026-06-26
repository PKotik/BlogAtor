using BlogAtor.Server.Models;
using BlogAtor.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http.Headers;

namespace BlogAtor.Server.Services
{
    public class MockRedditService : IRedditService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MockRedditService> _logger;

        public MockRedditService(
            HttpClient httpClient,
            ApplicationDbContext context,
            ILogger<MockRedditService> logger)
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

                // Используем старую версию Reddit (она менее требовательна)
                var url = $"https://www.reddit.com/r/{subreddit}/hot.json?limit={limit}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Полностью копируем заголовки из браузера
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                request.Headers.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                request.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
                request.Headers.Add("Sec-Ch-Ua", "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"");
                request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
                request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
                request.Headers.Add("Sec-Fetch-Dest", "document");
                request.Headers.Add("Sec-Fetch-Mode", "navigate");
                request.Headers.Add("Sec-Fetch-Site", "none");
                request.Headers.Add("Sec-Fetch-User", "?1");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Cache-Control", "max-age=0");

                request.Headers.Add("Cookie", "reddit_session=eyJhbGciOiJSUzI1NiIsImtpZCI6IlNIQTI1NjpsVFdYNlFVUEloWktaRG1rR0pVd1gvdWNFK01BSjBYRE12RU1kNzVxTXQ4IiwidHlwIjoiSldUIn0.eyJzdWIiOiJ0Ml8xcXk1YThiM3p2IiwiZXhwIjoxNzk4MDk2NjU0LjIzMDU1LCJpYXQiOjE3ODI0NTgyNTQuMjMwNTUsImp0aSI6Ijd2dnVCUUowTFpvLVpFWXF6XzNfbVoydkdzOUo4ZyIsImF0IjoxLCJjaWQiOiJjb29raWUiLCJsY2EiOjE3NDkyMzkzNzM1NzYsInNjcCI6ImVKeUtqZ1VFQUFEX193RVZBTGsiLCJmbG8iOjMsImFtciI6WyJzc28iXX0.wNzX2JsaTLNf_6cWQ3mjoMDROEEDX3_kaIP9uRRpkgHH674TsYJ479sZ8v3Yk-urGNbMG8kblYirOS4wq7JNXnbQWh8wzSq5gOMMwObwdCvxegv41LR0w6c2msDpobRy4P2Yfea6qeZsKgrwzA-aKIEooIL1gOzq_xD0OSNEUOoRyfzzzNcrN50OmGgDOQfWjgDteNxEvBWP65j2-Kj_laahmiDirwx1Tib4Y2OcY7tbP5Qly3X3z9OSzRZ5rYWzdeN6j5LfgvQj8IrgXTZZjsnRL-6rh2KiuTfKSkxjIlkmX1kRJr5Gb28URTus7YLtGLCMCmoQIkQfMV8CdCHOJw; csv=1; token_v2=eyJhbGciOiJSUzI1NiIsImtpZCI6IlNIQTI1NjpzS3dsMnlsV0VtMjVmcXhwTU40cWY4MXE2OWFFdWFyMnpLMUdhVGxjdWNZIiwidHlwIjoiSldUIn0.eyJzdWIiOiJ1c2VyIiwiZXhwIjoxNzgyNTQ0NjU0Ljg4NDQzNCwiaWF0IjoxNzgyNDU4MjU0Ljg4NDQzNCwianRpIjoiT2hjaDctNFJfc3ZVNDRaSVhYcFBZa2RTYVFqOEdBIiwiY2lkIjoiMFItV0FNaHVvby1NeVEiLCJsaWQiOiJ0Ml8xcXk1YThiM3p2IiwiYWlkIjoidDJfMXF5NWE4YjN6diIsImF0IjoxLCJsY2EiOjE3NDkyMzkzNzM1NzYsInNjcCI6ImVKeGtrZEdPdERBSWhkLUZhNV9nZjVVX20wMXRjWWFzTFFhb2szbjdEVm9jazcwN2NENHBIUDlES29xRkRDWlhncW5BQkZnVHJUREJSdVQ5bkxtM2cyaU5lOHRZc1puQ0JGbXdGRHJrbUxHc2lRUW1lSklheXhzbW9JTE55Rnl1dEdOTkxUMFFKcWhjTXJlRkhwYzJvYmtiaTU2ZEdGVzVyRHlvc1ZmbDB0akdGTFlueGpjYnF3MnB1QzZuTWtuTFF2a3NYdlRqTjlXMzl2bXpfU2EwSjhPS3F1bUIzaGxKQ0c0c2ZwaW0zZDlUazU2dEN4YTE5M3FRMnVkNjNLNTkxaXcwTzdlZjZfbHJJeG1YWTJoLUp2dDMxeS1oQTQ4OEx6UHFBRWFzNFVjWmRtUWRfbFVIVUxtZ0pHTUo0dE1JNU1ybDIzOEp0bXZUdjhidEV6OThNLUttTl96V0ROUnpDZUxRcF9IMUd3QUFfXzhRMWVUUiIsInJjaWQiOiJOTThpT3k0SjVYMjN0WWk4ekR6LW9MQm83blZiX1l2SF9OUWJZMm9La3dnIiwiZmxvIjoyfQ.HG-USn0j1lvawRAqMUERfumunTt5Pz_VGdYi_91XvfpiLXs2iMiMUj5xKRYzFku7YmPyjNkzZstJcytEWhljbV9FNh67XkwVrs65L4gXrdFM0nFp18ije49I4jXFqDzWxM4X3KcUmt8fJE8vIBRJx8sUt1V9UxIRvkpB0VksAkLSAbrmX2Qv3vJiX1-2T96xvjUk_BzHwi-W5ruP1yOOws09WusbxG0kGe3-i7aAp9u4PlZE0u0C6kemeWKQN4HWkMABEfdotuHdsMLDVSsLPTrMCA1v5g39jnWsGobc2IspRHMczTXzYtyMfbgCYocPtQSTAAPi0fgJEZWwapIPdQ");


                _logger.LogInformation("Запрос к URL: {Url}", url);

                var response = await _httpClient.SendAsync(request);
                string content;
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        using var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
                        using var reader = new StreamReader(gzip);
                        content = await reader.ReadToEndAsync();
                    }
                    else
                    {
                        using var reader = new StreamReader(stream);
                        content = await reader.ReadToEndAsync();
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Ошибка: {StatusCode}, Content: {Content}",
                        response.StatusCode, content?.Substring(0, Math.Min(500, content?.Length ?? 0)));

                    _logger.LogWarning("Пробуем через www.reddit.com...");
                    return await TryWwwRedditAsync(subreddit, limit);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var apiResponse = JsonSerializer.Deserialize<RedditApiResponse>(content, options);

                if (apiResponse?.Data?.Children == null || !apiResponse.Data.Children.Any())
                {
                    _logger.LogWarning("Нет данных в ответе");
                    return await GetMockPostsAsync(subreddit, limit);
                }

                foreach (var child in apiResponse.Data.Children)
                {
                    var postData = child.Data;
                    posts.Add(new RedditPost
                    {
                        PostId = postData.Id,
                        Subreddit = postData.Subreddit,
                        Author = postData.Author ?? "unknown",
                        Title = postData.Title ?? "No title",
                        Content = postData.SelfText ?? string.Empty,
                        Url = $"https://reddit.com{postData.Permalink}",
                        Score = postData.Score,
                        CommentCount = postData.NumComments,
                        CreatedAt = DateTimeOffset.FromUnixTimeSeconds((long)postData.CreatedUtc).UtcDateTime,
                        CollectedAt = DateTime.UtcNow,
                        Source = "Reddit"
                    });
                }

                _logger.LogInformation("✅ Получено {Count} постов из r/{Subreddit}", posts.Count, subreddit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении постов из r/{Subreddit}", subreddit);
                return await GetMockPostsAsync(subreddit, limit);
            }

            return posts;
        }

        private async Task<List<RedditPost>> TryWwwRedditAsync(string subreddit, int limit)
        {
            try
            {
                var url = $"https://www.reddit.com/r/{subreddit}/hot.json?limit={limit}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                request.Headers.Accept.ParseAdd("application/json");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("www.reddit.com тоже не работает");
                    return await GetMockPostsAsync(subreddit, limit);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<RedditApiResponse>(content, options);

                if (apiResponse?.Data?.Children == null)
                {
                    return await GetMockPostsAsync(subreddit, limit);
                }

                var posts = new List<RedditPost>();
                foreach (var child in apiResponse.Data.Children)
                {
                    var postData = child.Data;
                    posts.Add(new RedditPost
                    {
                        PostId = postData.Id,
                        Subreddit = postData.Subreddit,
                        Author = postData.Author ?? "unknown",
                        Title = postData.Title ?? "No title",
                        Content = postData.SelfText ?? string.Empty,
                        Url = $"https://reddit.com{postData.Permalink}",
                        Score = postData.Score,
                        CommentCount = postData.NumComments,
                        CreatedAt = DateTimeOffset.FromUnixTimeSeconds((long)postData.CreatedUtc).UtcDateTime,
                        CollectedAt = DateTime.UtcNow,
                        Source = "Reddit"
                    });
                }

                return posts;
            }
            catch
            {
                return await GetMockPostsAsync(subreddit, limit);
            }
        }

        public async Task<List<RedditPost>> GetPostsByAuthorAsync(string author, int limit = 25)
        {
            try
            {
                var url = $"https://old.reddit.com/search.json?q=author:{author}&limit={limit}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                request.Headers.Accept.ParseAdd("application/json");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return await GetMockPostsByAuthorAsync(author, limit);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<RedditApiResponse>(content, options);

                if (apiResponse?.Data?.Children == null)
                {
                    return await GetMockPostsByAuthorAsync(author, limit);
                }

                var posts = new List<RedditPost>();
                foreach (var child in apiResponse.Data.Children)
                {
                    var postData = child.Data;
                    posts.Add(new RedditPost
                    {
                        PostId = postData.Id,
                        Subreddit = postData.Subreddit ?? "unknown",
                        Author = postData.Author ?? author,
                        Title = postData.Title ?? "No title",
                        Content = postData.SelfText ?? string.Empty,
                        Url = $"https://reddit.com{postData.Permalink}",
                        Score = postData.Score,
                        CommentCount = postData.NumComments,
                        CreatedAt = DateTimeOffset.FromUnixTimeSeconds((long)postData.CreatedUtc).UtcDateTime,
                        CollectedAt = DateTime.UtcNow,
                        Source = "Reddit"
                    });
                }

                return posts;
            }
            catch
            {
                return await GetMockPostsByAuthorAsync(author, limit);
            }
        }

        public async Task SavePostsAsync(List<RedditPost> posts)
        {
            foreach (var post in posts)
            {
                var existing = await _context.RedditPosts
                    .FirstOrDefaultAsync(p => p.PostId == post.PostId);

                if (existing == null)
                    await _context.RedditPosts.AddAsync(post);
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
            _logger.LogInformation("💾 Сохранено {Count} постов в БД", posts.Count);
        }

        public async Task<IQueryable<RedditPost>> GetAllPostsAsync()
        {
            return await Task.FromResult(_context.RedditPosts.AsQueryable());
        }

        public async Task<RedditPost?> GetPostByIdAsync(int id)
        {
            return await _context.RedditPosts.FindAsync(id);
        }

        // Мок-методы для fallback
        private async Task<List<RedditPost>> GetMockPostsAsync(string subreddit, int limit)
        {
            var posts = new List<RedditPost>();
            for (int i = 0; i < Math.Min(limit, 10); i++)
            {
                posts.Add(new RedditPost
                {
                    PostId = $"mock_{i}_{DateTime.UtcNow.Ticks}",
                    Subreddit = subreddit,
                    Author = $"user_{i + 1}",
                    Title = $"📌 Тестовый пост #{i + 1} из r/{subreddit}",
                    Content = $"Это тестовое содержимое поста #{i + 1}.",
                    Url = $"https://reddit.com/r/{subreddit}/mock_{i}",
                    Score = 100 - i * 10,
                    CommentCount = 50 - i * 5,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5),
                    CollectedAt = DateTime.UtcNow,
                    Source = "Mock (Reddit временно недоступен)"
                });
            }
            return await Task.FromResult(posts);
        }

        private async Task<List<RedditPost>> GetMockPostsByAuthorAsync(string author, int limit)
        {
            var posts = new List<RedditPost>();
            for (int i = 0; i < Math.Min(limit, 5); i++)
            {
                posts.Add(new RedditPost
                {
                    PostId = $"mock_{i}_{DateTime.UtcNow.Ticks}",
                    Subreddit = "dotnet",
                    Author = author,
                    Title = $"📌 Тестовый пост автора {author} #{i + 1}",
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
    }
}