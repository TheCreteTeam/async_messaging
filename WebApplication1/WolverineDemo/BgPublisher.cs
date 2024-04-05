using AsyncMessagingCommon.Contracts;
using AsyncMessagingCommon.Entities;
using Coravel.Invocable;
using Wolverine;

namespace WebApplication1.WolverineDemo;

public class BgPublisher: IInvocable
{
    private readonly ILogger<BgPublisher> _logger;
    private readonly IMessageBus _messageBus;
    
    public BgPublisher(ILogger<BgPublisher> logger, IMessageBus messageBus)
    {
        _logger = logger;
        _messageBus = messageBus;
    }

    // BackgroundService
    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     while (!stoppingToken.IsCancellationRequested)
    //     {
    //         _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
    //         string randomString = Guid.NewGuid().ToString();
    //         await _messageBus.SendAsync(new CreateMessage("Hello from BgPublisher, guid: " + randomString));
    //         await Task.Delay(2000, stoppingToken);
    //     }
    // }

    public async Task Invoke()
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        string randomString = Guid.NewGuid().ToString();
        AppMessage message = new()
        {
            Text = "Hello from BgPublisher, guid: " + randomString,
            TimeStamp = DateTime.Now
        };
        await _messageBus.SendAsync(new CreateMessage(message));
    }
}