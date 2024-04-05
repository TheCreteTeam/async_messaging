using AsyncMessagingCommon.Contracts;
using MassTransit;
using WebApplication1.Data;

namespace WebApplication1.Consumers;

public class MessageConsumer : IConsumer<CreateMessage>
{
    private readonly ILogger<MessageConsumer> _logger;
    private readonly ApplicationDbContext _dbContext;
    
    public MessageConsumer(ILogger<MessageConsumer> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    // MassTransit
    public async Task Consume(ConsumeContext<CreateMessage> context)
    { 
        _logger.LogInformation($"MassTransit message received: {context.Message.AppMessage.Text}");
        
        // log all messages
        var messages = _dbContext.AppMessages.ToList();
        foreach (var message in messages)
        {
            _logger.LogInformation($"Message in db: {message.Text}");
        }
    }
 
}