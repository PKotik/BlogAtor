using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogAtor.Server.Services;
using BlogAtor.Server.DTOs;
using BlogAtor.Server.Models;

namespace BlogAtor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly IRedditService _redditService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(IRedditService redditService, ILogger<NewsController> logger)
        {
            _redditService = redditService;
            _logger = logger;
        }

        [HttpPost("collect")]
        public async Task<IActionResult> CollectFromReddit([FromBody] CollectRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Subreddit))
                    return BadRequest("Не указан сабреддит");

                _logger.LogInformation("Запрос на сбор постов из r/{Subreddit}", request.Subreddit);

                var posts = await _redditService.GetHotPostsFromSubredditAsync(
                    request.Subreddit,
                    request.Limit ?? 25);

                await _redditService.SavePostsAsync(posts);

                return Ok(new
                {
                    Message = $"Собрано {posts.Count} постов из r/{request.Subreddit}",
                    Count = posts.Count,
                    Posts = posts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сборе постов");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("posts")]
        public async Task<IActionResult> GetRedditPosts([FromQuery] string? subreddit = null)
        {
            try
            {
                var query = await _redditService.GetAllPostsAsync();

                if (!string.IsNullOrEmpty(subreddit))
                    query = query.Where(p => p.Subreddit == subreddit);

                var posts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении постов");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("posts/{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            try
            {
                var post = await _redditService.GetPostByIdAsync(id);
                if (post == null)
                    return NotFound($"Пост с ID {id} не найден");

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении поста {Id}", id);
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}