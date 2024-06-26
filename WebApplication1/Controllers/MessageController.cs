﻿using AsyncMessagingCommon.Commands;
using AsyncMessagingCommon.Contracts;
using AsyncMessagingCommon.Entities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Paramore.Brighter;
using WebApplication1.Data;
using WebApplication1.Services.Definitions;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly IAmACommandProcessor _commandProcessor;
    private readonly ILogger<MessageController> _logger;
    private readonly IMessageService _messageService;
    private readonly IBus _bus;
    // db context
    private readonly ApplicationDbContext _dbContext;
    
    public MessageController(IMessageService messageService, IAmACommandProcessor commandProcessor, IBus bus, ApplicationDbContext dbContext, ILogger<MessageController> logger)
    {
        _messageService = messageService;
        _commandProcessor = commandProcessor;
        _logger = logger;
        _dbContext = dbContext;
        _bus = bus;
    }
    
    [HttpPost("PostMessage")]
    public async ValueTask<ActionResult<string>> PostMessage(string? message)
    {
        if (message == null)
        {
            _logger.LogError("Message is null.");
            return BadRequest("Message is null.");
        }

        // Create a message in memory dbcontext
        var messageEntity = new AppMessage
        {
            Text = message,
            TimeStamp = DateTime.Now.ToUniversalTime(),
            MailSent = 0,
            Guid = Guid.NewGuid()
        };
        
        // save to db context
        // start transaction
        // Transaction not used by Chapsas or MassTransit demo app ???
        // await _dbContext.Database.BeginTransactionAsync();
        _dbContext.AppMessages?.Add(messageEntity);
        await _bus.Publish(new CreateMessage(messageEntity));
        await _dbContext.SaveChangesAsync();
        // await _dbContext.Database.CommitTransactionAsync();
        
        _logger.LogInformation("Message {Message} saved to db context", message);
        
        // await _messageService.SendMessageToAzureStorageQueue(message);
        return Ok();
    }

    [HttpGet("GetMessage")]
    public async ValueTask<ActionResult<string>> GetMessage()
    {
        var testConfVar = Environment.GetEnvironmentVariable("TEST_CONF_VAR");
        _logger.LogInformation($"TEST_CONF_VAR: {testConfVar}");
        
        
         var message = await _messageService.RetrieveNextMessageAsync();
         var command = new GetMessageCommand(message);
         await _commandProcessor.SendAsync(command);
         return Ok(message);
    }
}