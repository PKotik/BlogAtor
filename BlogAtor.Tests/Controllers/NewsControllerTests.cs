using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlogAtor.Server.Controllers;
using BlogAtor.Server.Services;
using BlogAtor.Server.DTOs;
using BlogAtor.Server.Models;
using FluentAssertions;

namespace BlogAtor.Tests.Controllers
{
    public class NewsControllerTests
    {
        [Fact]
        public async Task CollectFromReddit_ShouldReturnBadRequest_WhenSubredditIsEmpty()
        {
            // Arrange
            var mockService = new Mock<IRedditService>();
            var logger = new Mock<ILogger<NewsController>>().Object;
            var controller = new NewsController(mockService.Object, logger);

            var request = new CollectRequest
            {
                Subreddit = "",
                Limit = 5
            };

            // Act
            var result = await controller.CollectFromReddit(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }


        [Fact]
        public async Task GetPostById_ShouldReturnOk_WhenPostExists()
        {
            // Arrange
            var mockService = new Mock<IRedditService>();
            var logger = new Mock<ILogger<NewsController>>().Object;

            var expectedPost = new RedditPost
            {
                Id = 1,
                PostId = "test_1",
                Title = "Test Post",
                Subreddit = "dotnet"
            };

            mockService
                .Setup(s => s.GetPostByIdAsync(1))
                .ReturnsAsync(expectedPost);

            var controller = new NewsController(mockService.Object, logger);

            // Act
            var result = await controller.GetPostById(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var post = okResult.Value as RedditPost;
            post.Should().NotBeNull();
            post.Id.Should().Be(1);
            post.Title.Should().Be("Test Post");
        }

        [Fact]
        public async Task GetPostById_ShouldReturnNotFound_WhenPostDoesNotExist()
        {
            // Arrange
            var mockService = new Mock<IRedditService>();
            var logger = new Mock<ILogger<NewsController>>().Object;

            mockService
                .Setup(s => s.GetPostByIdAsync(999))
                .ReturnsAsync((RedditPost?)null);

            var controller = new NewsController(mockService.Object, logger);

            // Act
            var result = await controller.GetPostById(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }
    }
}