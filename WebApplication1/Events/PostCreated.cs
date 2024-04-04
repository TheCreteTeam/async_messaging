using Coravel.Events.Interfaces;

public class PostCreated : IEvent
{
    public string message { get; set; }
    
    public PostCreated(string message)
    {
        this.message = message;
    }
}