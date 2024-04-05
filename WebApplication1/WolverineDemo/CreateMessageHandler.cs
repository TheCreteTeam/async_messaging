using AsyncMessagingCommon.Contracts;

namespace WebApplication1.WolverineDemo;

public class CreateMessageHandler
{
    // logger
    private readonly ILogger<CreateMessageHandler> _logger;
    
    public CreateMessageHandler(ILogger<CreateMessageHandler> logger)
    {
        _logger = logger;
    }
    
    public void Handle(CreateMessage message)
    {
        _logger.LogInformation($"Message received: {message.AppMessage.Text}");
    }
}