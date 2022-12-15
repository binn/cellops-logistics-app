namespace AngelPhoneTrack.Data
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Pin { get; set; } = default!;
        public bool Admin { get; set; } = false;
        public string Token { get; set; } = default!;
        public bool Supervisor { get; set; } = false;

        public Department Department { get; set; } = default!;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}