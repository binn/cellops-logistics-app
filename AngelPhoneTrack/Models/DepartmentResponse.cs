using AngelPhoneTrack.Data;

namespace AngelPhoneTrack.Models
{
    public class DepartmentResponse
    {
        public DepartmentResponse(Department d)
        {
            Id = d.Id;
            Name = d.Name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DepartmentResponseWithDescription : DepartmentResponse
    {
        public DepartmentResponseWithDescription(Department d) : base(d)
        {
            Description = d.Description;
        }

        public string Description { get; set; }
    }
}
