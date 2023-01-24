using AngelPhoneTrack.Data;

namespace AngelPhoneTrack.Models
{
    public class AssignmentsUpdate
    {
        public AssignmentsUpdate(Department dept, int count)
        {
            Id = dept.Id;
            Name = dept.Name;
            Count = count;
        }
        
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
