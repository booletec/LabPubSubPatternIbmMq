namespace Ibmmq.Core.Conectors
{
    public abstract class Event(string? payload)
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public DateTime CreateAt { get; private set; } = DateTime.Now;

        public string? Payload { get; private set; } = payload;

    }
}
