namespace AngelPhoneTrack.Data
{
    public class LotAssignment
    {
        public Guid Id { get; set; }
        public int Count {  get; set; } = 0;
        public Lot Lot { get; set; } = default!;
        public bool Received { get; set; } = true;
        public Department Department { get; set; } = default!;
        public int? IncomingDepartmentId { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
