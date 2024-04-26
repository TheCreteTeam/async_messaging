using MassTransit;
using WebApplication2.Data;

namespace WebApplication2.Consumers;


public class MessageConsumerDefinition :
    ConsumerDefinition<MessageConsumer> 
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<MessageConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000, 1500, 5000, 6000,7000,10000));

        // endpointConfigurator.UseInMemoryOutbox(context);
        endpointConfigurator.UseEntityFrameworkOutbox<ApplicationDbContext>(context);
    }
}