namespace AngelPhoneTrack.Data
{
    public class Department
    {
        public Department()
        {
            Assignments = new HashSet<LotAssignment>();
            Employees = new HashSet<Employee>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool IsAssignable { get; set; } = false; // default to be non-assignable departments

        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<LotAssignment> Assignments { get; set; }
    }
}