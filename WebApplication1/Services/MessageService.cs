using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using WebApplication1.Services.Definitions;

namespace WebApplication1.Services;

public class MessageService: IMessageService
{

    private readonly ILogger<MessageService> _logger;
    
    public MessageService(ILogger<MessageService> logger)
    {
        _logger = logger;
    }
    
    public async Task SendMessageToAzureStorageQueue(string message)
    {
        var queueClient = QueueClient();

        await InsertMessageAsync(queueClient, message);
    }

    private static QueueClient QueueClient()
    {
        string? connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        string? queueName = Environment.GetEnvironmentVariable("AZURE_STORAGE_QUEUE_NAME");
        
        string? queueUri = Environment.GetEnvironmentVariable("AZURE_STORAGE_URI");
        
        // Create a client to interact with the queue
        
        // 1. Connection string
        // QueueClient queueClient = new QueueClient(connectionString, queueName);
        
        // 2. URI - passwordless
        var options = new DefaultAzureCredentialOptions
        {    
            Diagnostics =
            {
                LoggedHeaderNames = { "x-ms-request-id" },
                LoggedQueryParameters = { "api-version" },
                IsLoggingContentEnabled = true
            },
            ExcludeManagedIdentityCredential = true,
            ExcludeVisualStudioCodeCredential = true
        };
        if (queueUri != null)
        {
            QueueClient queueClient = new QueueClient(new Uri(queueUri), new DefaultAzureCredential(options));
            return queueClient;
        }
        else
        {
            QueueClient queueClient = new QueueClient(connectionString, queueName);
            return queueClient;
        }
    }

    private async Task InsertMessageAsync(QueueClient theQueue, string newMessage)
    {
        if (null != await theQueue.CreateIfNotExistsAsync())
        {
            _logger.LogInformation("The queue was created.");
        }
        // var queueMessage = new CloudQueueMessage(newMessage);
        var receipt = await theQueue.SendMessageAsync(newMessage, default, TimeSpan.FromSeconds(-1), default);
        _logger.LogInformation("The message was added to the queue, id: {0}", receipt.Value.MessageId);
    }
    
    // Below code should be used by a client task
    public async Task<string?> RetrieveNextMessageAsync()
    {
        var queueClient = QueueClient();
        
        return await RetrieveNextMessageAsync(queueClient);
    }
    
    private async Task<string?> RetrieveNextMessageAsync(QueueClient theQueue)
    {
        if (!await theQueue.ExistsAsync()) return null;
        QueueProperties properties = await theQueue.GetPropertiesAsync();

        if (properties.ApproximateMessagesCount <= 0) return null;
        
        QueueMessage[] retrievedMessage = await theQueue.ReceiveMessagesAsync(1);
        string? theMessage = retrievedMessage[0].Body.ToString();
        await theQueue.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
        return theMessage;

    }
}