using AsyncMessagingCommon.Contracts;
using MassTransit;
using WebApplication2.Data;

namespace WebApplication2.Consumers;

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
        
        // randomise error or success, success in 1 out of 4
        if (new Random().Next(0, 5) != 0)
        {
            _logger.LogError("Random error occurred.");
            throw new Exception("Random error occurred.");
        }

        _logger.LogInformation("Consume message start.");
        
        // get message from db with same guid
        var appMessage = _dbContext.AppMessages?.FirstOrDefault(m => m.Guid == context.Message.AppMessage.Guid);
        if(appMessage == null)
        {
            _logger.LogError("Message not found in db.");
            return;
        }
        appMessage.MailSent += 1;
        
        _dbContext.AppMessages?.Update(appMessage);
        await _dbContext.SaveChangesAsync();
        
        // log all messages
        var messages = _dbContext.AppMessages?.ToList();
        if (messages != null)
        {
            foreach (var message in messages)
            {
                _logger.LogInformation("Message in db: {MessageText}", message.Text);
            }
        }

    }
 
}