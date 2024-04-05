using MassTransit;

namespace WebApplication1.Consumers;


public class MessageConsumerDefinition :
    ConsumerDefinition<MessageConsumer> 
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<MessageConsumer> consumerConfigurator)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    }
}