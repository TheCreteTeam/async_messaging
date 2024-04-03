using AsyncMessagingCommon.Commands;
using Microsoft.AspNetCore.Mvc;
using Paramore.Brighter;
using WebApplication1.Services.Definitions;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly ILogger<MessageController> _logger;
    private readonly IMessageService _messageService;
    
    public MessageController(IMessageService messageService, IAmACommandProcessor commandProcessor, ILogger<MessageController> logger)
    {
        _messageService = messageService;
        _commandProcessor = commandProcessor;
        _logger = logger;
    }
    
    [HttpPost("PostMessage")]
    public async ValueTask<ActionResult<string>> PostMessage(string? message)
    {
        if (message == null)
        {
            _logger.LogError("Message is null.");
            return BadRequest("Message is null.");
        }

        await _messageService.SendMessageToAzureStorageQueue(message);
        return Ok();
    }

    [HttpGet("GetMessage")]
    public async ValueTask<ActionResult<string>> GetMessage()
    {
         var message = await _messageService.RetrieveNextMessageAsync();
         var command = new GetMessageCommand(message);
         await _commandProcessor.SendAsync(command);
         return Ok(message);
    }
}