using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.Dtos;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]

    public class RestoreReadDbController(
        ILogger<RestoreReadDbController> logger,
        ICommandDispatcher commandDispatcher
        ) : ControllerBase
    {
        private readonly ILogger<RestoreReadDbController> _logger = logger;
        private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;

        [HttpPost]
        public async Task<ActionResult> RestoreReadDbAsync() {
            try
            {
                await _commandDispatcher.SendAsync(new RestoreReadDbCommand{});

                return StatusCode(StatusCodes.Status200OK, new BaseResponse{
                    Message = "Read database restore request completed successfully"
                });
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
                const string SAFE_ERROR_MSG = "Error while processing request to restore read db";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MSG);
                
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse{
                    Message = SAFE_ERROR_MSG
                });
            }
        }
    }
}