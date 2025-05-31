using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Common.Dtos;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]

    public class EditMessageController(
        ILogger<EditMessageController> logger,
        ICommandDispatcher commandDispatcher
        ) : ControllerBase
    {
        private readonly ILogger<EditMessageController> _logger = logger;
        private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;

        [HttpPut("{id}")]
        public async Task<ActionResult> EditMessageAsync(Guid id, EditMessageCommand command) {
            try {
                command.Id = id;
                await _commandDispatcher.SendAsync(command);

                return Ok(new BaseResponse{
                    Message = "Edit message request completed successfully"
                });
            } 
            catch (InvalidOperationException ex) 
            {
                _logger.Log(LogLevel.Warning, ex, "Client made bad request");
                return BadRequest(new BaseResponse{
                    Message = ex.Message
                });
            }
            catch (AggregateNotFoundException ex) 
            {
                _logger.Log(LogLevel.Warning, ex, "Could not retrieve aggregate");
                return BadRequest(new BaseResponse{
                    Message = ex.Message
                });
            }
            catch (Exception ex) 
            {
                const string SAFE_ERROR_MSG = "Error while processing request to edit post message";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MSG);

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
}