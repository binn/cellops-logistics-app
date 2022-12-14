namespace AngelPhoneTrack.Data
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Pin { get; set; } = default!;
        public bool IsAdmin { get; set; } = false;
        public string Token { get; set; } = default!;
        public bool IsSupervisor { get; set; } = false;

        public Department Department { get; set; } = default!;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}