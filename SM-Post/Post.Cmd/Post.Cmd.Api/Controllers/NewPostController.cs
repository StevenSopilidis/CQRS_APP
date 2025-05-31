using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.Dtos;
using Post.Common.Events;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class NewPostController(
        ILogger<NewPostController> logger,
        ICommandDispatcher commandDispatcher
        ) : ControllerBase
    {
        private readonly ILogger<NewPostController> _logger = logger;
        private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;

        [HttpPost]
        public async Task<ActionResult> NewPostAsync(NewPostCommand command) 
        {
            var id  = Guid.NewGuid();
            try
            {
                command.Id = id;

                await _commandDispatcher.SendAsync(command);
                
                return StatusCode(
                    StatusCodes.Status201Created, 
                    new NewPostResponse{
                        Message = "New post creation request completed successfully"      
                    }
                );
            } 
            catch (InvalidOperationException ex) 
            {
                _logger.Log(LogLevel.Warning, ex, "Client made bad request");
                return BadRequest(new BaseResponse{
                    Message = ex.Message
                });
            }
            catch (Exception ex) 
            {
                const string SAFE_ERROR_MSG = "Error while processing request to create new post";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MSG);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new NewPostResponse
                    {
                        Id = id,
                        Message = SAFE_ERROR_MSG
                    } 
                );
            }
        }
    }
}