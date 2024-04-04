using Coravel.Events.Interfaces;

public class PostListener : IListener<PostCreated>
{
    private readonly ILogger<PostListener> _logger;
    
    public PostListener(ILogger<PostListener> logger)
    {
        _logger = logger;
    }
    
    public async Task HandleAsync(PostCreated broadcasted)
    {
        _logger.LogInformation($"Event broadcasted: {broadcasted.message}");
    }
}
