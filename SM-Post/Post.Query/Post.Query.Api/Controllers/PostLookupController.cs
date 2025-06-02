using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Common.Dtos;
using Post.Query.Api.DTOs;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;

namespace Post.Query.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PostLookupController : ControllerBase
    {
        private readonly ILogger<PostLookupController> _logger;
        private readonly IQueryDispatcher<PostEntity> _dispatcher;

        public PostLookupController(
            ILogger<PostLookupController> logger, 
            IQueryDispatcher<PostEntity> dispatcher
        )
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPostsAsync() {
            try
            {
                var posts = await _dispatcher.SendAsync(new FindAllPostsQuery());
                return SuccessResponse(posts);
            }
            catch (System.Exception ex)
            {
                const string SAFE_ERROR_MSG = "Error while processing request to retreive all posts";
                return ErrorResponse(ex, SAFE_ERROR_MSG);
            }
        }

        [HttpGet("byId/{postId}")]
        public async Task<ActionResult> GetByPostIdAsync(Guid postId) {
            try
            {
                var posts = await _dispatcher.SendAsync(new FindPostByIdQuery{
                    Id = postId
                });
                return SuccessResponse(posts);
            }
            catch (System.Exception ex)
            {
                const string SAFE_ERROR_MSG = "Error while processing request to post by ID";
                return ErrorResponse(ex, SAFE_ERROR_MSG);
            }
        }

        [HttpGet("byAuthor/{author}")]
        public async Task<ActionResult> GetPostsByAuthorAsync(string author) {
            try
            {
                var posts = await _dispatcher.SendAsync(new FindPostsByAuthorQuery{
                    Author = author
                });
                return SuccessResponse(posts);
            }
            catch (System.Exception ex)
            {
                const string SAFE_ERROR_MSG = "Error while processing request to get posts by author";
                return ErrorResponse(ex, SAFE_ERROR_MSG);
            }
        }

        [HttpGet("withComments")]
        public async Task<ActionResult> GetPostsWithCommentsAsync() {
            try
            {
                var posts = await _dispatcher.SendAsync(new FindPostsWithCommentsQuery { });
                return SuccessResponse(posts);
            }
            catch (System.Exception ex)
            {
                const string SAFE_ERROR_MSG = "Error while processing request to get posts with comments";
                return ErrorResponse(ex, SAFE_ERROR_MSG);
            }
        }

        [HttpGet("withLikes/{numberOfLikes}")]
        public async Task<ActionResult> GetPostsWithLikesAsync(int numberOfLikes){
            try
            {
                var posts = await _dispatcher.SendAsync(new FindPostsWithLikesQuery { 
                    NumberOfLikes = numberOfLikes
                });
                return SuccessResponse(posts);
            }
            catch (System.Exception ex)
            {
                const string SAFE_ERROR_MSG = "Error while processing request to get posts with likes";
                return ErrorResponse(ex, SAFE_ERROR_MSG);
            }
        }

        private ActionResult SuccessResponse(List<PostEntity> posts)
        {
            if (posts == null || !posts.Any())
                return NoContent();

            var count = posts.Count;

            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = $"Successfully returned {count} post{(count > 1? "s" : string.Empty)}"
            });
        }
    
        private ActionResult ErrorResponse(Exception ex, string SAFE_ERROR_MSG)
        {
            _logger.LogError(ex, SAFE_ERROR_MSG);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new BaseResponse
                {
                    Message = SAFE_ERROR_MSG
                }
            );
        }
    }
}