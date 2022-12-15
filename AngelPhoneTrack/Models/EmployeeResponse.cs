using AngelPhoneTrack.Data;

namespace AngelPhoneTrack.Models
{
    public class EmployeeResponse
    {
        public EmployeeResponse(Employee e)
        {
            Id = e.Id;
            Name = e.Name;
            Pin = e.Pin;
            Admin = e.Admin;
            Timestamp = e.Timestamp;
            Supervisor = e.Supervisor;
            Department = new DepartmentResponse(e.Department);
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Pin { get; set; }
        public bool Admin { get; set; }
        public bool Supervisor { get; set; }

        public DepartmentResponse Department { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
