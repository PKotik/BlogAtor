using BlogAtor.Server.Models;

namespace BlogAtor.Server.Services
{
    public interface IRedditService
    {
        Task<List<RedditPost>> GetHotPostsFromSubredditAsync(string subreddit, int limit = 25);
        Task<List<RedditPost>> GetPostsByAuthorAsync(string author, int limit = 25);
        Task SavePostsAsync(List<RedditPost> posts);
        Task<IQueryable<RedditPost>> GetAllPostsAsync();
        Task<RedditPost?> GetPostByIdAsync(int id);
    }
}