using Ibmmq.Core.Conectors;

namespace Ibmmq.Core.Domain.Events
{
    public class EventMessage : Event
    {
      
        public bool IsCoa { get; set; }
        public bool IsCod { get; set; }
        public string? MqId { get; set; }
        public string? CorrelationId { get; set; }
    }

    public class ReceivedMessage : EventMessage
    {
    }

    public class ReportedMessage : EventMessage
    {
    }
}
