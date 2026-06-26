using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BlogAtor.Server.Data;
using BlogAtor.Server.Models;
using BlogAtor.Server.Services;
using FluentAssertions;

namespace BlogAtor.Tests.Services
{
    public class MockRedditServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetHotPostsFromSubredditAsync_ShouldReturnPosts_WithCorrectLimit()
        {
            // Arrange
            var context = GetDbContext();
            var logger = new Mock<ILogger<MockRedditService>>().Object;
            var service = new MockRedditService(context, logger);
            var subreddit = "dotnet";
            var limit = 3;

            // Act
            var posts = await service.GetHotPostsFromSubredditAsync(subreddit, limit);

            // Assert
            posts.Should().NotBeNull();
            posts.Count.Should().Be(limit);
            posts.All(p => p.Subreddit == subreddit).Should().BeTrue();
            posts.All(p => p.Source.Contains("Mock")).Should().BeTrue();
        }

        [Fact]
        public async Task GetHotPostsFromSubredditAsync_ShouldReturnPosts_WithCorrectData()
        {
            // Arrange
            var context = GetDbContext();
            var logger = new Mock<ILogger<MockRedditService>>().Object;
            var service = new MockRedditService(context, logger);
            var subreddit = "csharp";

            // Act
            var posts = await service.GetHotPostsFromSubredditAsync(subreddit, 2);

            // Assert
            posts.Should().NotBeEmpty();
            var firstPost = posts.First();
            firstPost.Subreddit.Should().Be(subreddit);
            firstPost.Title.Should().Contain("Тестовый пост");
            firstPost.Author.Should().NotBeNullOrEmpty();
            firstPost.Score.Should().BeGreaterThan(0);
            firstPost.CommentCount.Should().BeGreaterThan(0);
            firstPost.PostId.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetPostsByAuthorAsync_ShouldReturnPosts_WithCorrectAuthor()
        {
            // Arrange
            var context = GetDbContext();
            var logger = new Mock<ILogger<MockRedditService>>().Object;
            var service = new MockRedditService(context, logger);
            var author = "test_user";

            // Act
            var posts = await service.GetPostsByAuthorAsync(author, 3);

            // Assert
            posts.Should().NotBeNull();
            posts.Count.Should().Be(3);
            posts.All(p => p.Author == author).Should().BeTrue();
        }

        [Fact]
        public async Task SavePostsAsync_ShouldSavePosts_ToDatabase()
        {
            // Arrange
            var context = GetDbContext();
            var logger = new Mock<ILogger<MockRedditService>>().Object;
            var service = new MockRedditService(context, logger);

            var posts = new List<RedditPost>
            {
                new RedditPost
                {
                    PostId = "test_1",
                    Subreddit = "dotnet",
                    Author = "user1",
                    Title = "Test Post 1",
                    Content = "Content 1",
                    Url = "https://reddit.com/test1",
                    Score = 100,
                    CommentCount = 50,
                    CreatedAt = DateTime.UtcNow,
                    CollectedAt = DateTime.UtcNow,
                    Source = "Mock"
                },
                new RedditPost
                {
                    PostId = "test_2",
                    Subreddit = "dotnet",
                    Author = "user2",
                    Title = "Test Post 2",
                    Content = "Content 2",
                    Url = "https://reddit.com/test2",
                    Score = 80,
                    CommentCount = 30,
                    CreatedAt = DateTime.UtcNow,
                    CollectedAt = DateTime.UtcNow,
                    Source = "Mock"
                }
            };

            // Act
            await service.SavePostsAsync(posts);

            // Assert
            var savedPosts = await context.RedditPosts.ToListAsync();
            savedPosts.Count.Should().Be(2);
            savedPosts.Any(p => p.PostId == "test_1").Should().BeTrue();
            savedPosts.Any(p => p.PostId == "test_2").Should().BeTrue();
        }

        [Fact]
        public async Task SavePostsAsync_ShouldUpdateExistingPosts_OnDuplicate()
        {
            // Arrange
            var context = GetDbContext();
            var logger = new Mock<ILogger<MockRedditService>>().Object;
            var service = new MockRedditService(context, logger);

            // Сначала сохраняем пост
            var initialPost = new RedditPost
            {
                PostId = "test_1",
                Subreddit = "dotnet",
                Author = "user1",
                Title = "Test Post 1",
                Content = "Content 1",
                Url = "https://reddit.com/test1",
                Score = 100,
                CommentCount = 50,
                CreatedAt = DateTime.UtcNow,
                CollectedAt = DateTime.UtcNow,
                Source = "Mock"
            };

            await service.SavePostsAsync(new List<RedditPost> { initialPost });

            // Затем обновляем его
            var updatedPost = new RedditPost
            {
                PostId = "test_1",
                Subreddit = "dotnet",
                Author = "user1",
                Title = "Test Post 1 Updated",  // ✅ Изменённый заголовок
                Content = "Content 1 Updated",  // ✅ Изменённый контент
                Url = "https://reddit.com/test1",
                Score = 150,
                CommentCount = 70,
                CreatedAt = DateTime.UtcNow,
                CollectedAt = DateTime.UtcNow,
                Source = "Mock"
            };

            // Act
            await service.SavePostsAsync(new List<RedditPost> { updatedPost });

            // Assert
            var savedPost = await context.RedditPosts.FirstOrDefaultAsync(p => p.PostId == "test_1");
            savedPost.Should().NotBeNull();
            savedPost.Title.Should().Be("Test Post 1 Updated");
            savedPost.Content.Should().Be("Content 1 Updated");  // ✅ Добавляем проверку контента
            savedPost.Score.Should().Be(150);
            savedPost.CommentCount.Should().Be(70);
        }
    }
}