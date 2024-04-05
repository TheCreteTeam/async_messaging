using AsyncMessagingCommon.Commands;
using MassTransit;
using Paramore.Brighter;
using Paramore.Brighter.Policies.Attributes;

namespace WebApplication1.Handlers
{
    public class GetMessageHandler : RequestHandlerAsync<GetMessageCommand>
    {
        private readonly ILogger<GetMessageHandler> _logger;

        public GetMessageHandler(ILogger<GetMessageHandler> logger)
        {
            _logger = logger;
        }

        // Brighter
        [UsePolicyAsync(step: 1, policy: CommandProcessor.RETRYPOLICYASYNC)]
        public override async Task<GetMessageCommand> HandleAsync(GetMessageCommand command, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Brighter - Message FINALLY received!");
            
            return  await base.HandleAsync(command, cancellationToken);
        }
    }
}
