using Ibmmq.Core.Conectors;

namespace Ibmmq.Core.Domain.Events
{
    public class EventMessage : Event
    {
        public EventMessage(string? payload) : base(payload)
        {
        }

        public bool IsCoa { get; set; }
        public bool IsCod { get; set; }
        public string? MqId { get; set; }
        public string? CorrelationId { get; set; }
    }
}
