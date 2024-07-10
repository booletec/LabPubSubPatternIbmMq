namespace IBMMQ.Core.Infra.Abstractions
{
    public class Event
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public DateTime CreateAt { get; private set; } = DateTime.Now;

        public string Payload { get; set; } = string.Empty;
        public bool IsCoa { get; set; }
        public bool IsCod { get; set; }
        public string? MqId { get; set; }
        public string? CorrelationId { get; set; }

    }
}
