namespace AngelPhoneTrack.Data
{
    public class LotAssignment
    {
        public Guid Id { get; set; }
        public int Count {  get; set; } = 0;
        public Lot Lot { get; set; } = default!;
        public Department Department { get; set; } = default!;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
