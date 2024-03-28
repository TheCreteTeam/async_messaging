namespace WebApplication1.Services.Definitions;

public interface IMessageService
{
    Task SendMessageToAzureStorageQueue(string message);
    Task<string?> RetrieveNextMessageAsync();
}