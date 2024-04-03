using System.Diagnostics;
using Paramore.Brighter;

namespace AsyncMessagingCommon.Commands
{
    public class GetMessageCommand : ICommand
    {
        public string? MessageId { get; set; }
    
        public GetMessageCommand(string? messageId)
        {
            MessageId = messageId;
            Id = Guid.NewGuid();
            Span = Activity.Current ?? new Activity(nameof(GetMessageCommand));
        }

        public Guid Id { get; set; }
        public Activity Span { get; set; }
    }   
}