namespace AngelPhoneTrack.Data
{
    public class Note
    {
        public Guid Id { get; set; }
        public Lot Lot { get; set; } = default!;
        public string Data { get; set; } = default!;
        public string CreatedBy { get; set; } = default!;
        public Department Department { get; set; } = default!;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}