using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Definitions;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    private readonly IMessageService _messageService;
    
    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }
    
    [HttpPost("PostMessage")]
    public async ValueTask<ActionResult<string>> PostMessage(string? message)
    {
        if (message == null)
        {
            return BadRequest("Message is null.");
        }

        await _messageService.SendMessageToAzureStorageQueue(message);
        return Ok();
    }

    [HttpGet("GetMessage")]
    public async ValueTask<ActionResult<string>> GetMessage()
    {
         var msg = await _messageService.RetrieveNextMessageAsync();
         return Ok(msg);
    }
}