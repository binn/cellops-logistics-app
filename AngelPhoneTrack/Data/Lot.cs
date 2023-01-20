using Microsoft.EntityFrameworkCore;

namespace AngelPhoneTrack.Data
{
    [Index(nameof(LotNo))]
    public class Lot
    {
        public Lot(string lotNo = null!, int count = 0)
        {
            Assignments = new HashSet<LotAssignment>();
            Timestamp = DateTimeOffset.UtcNow;
            Tasks = new HashSet<LotTask>();
            Audits = new HashSet<Audit>();
            Notes = new HashSet<Note>();
            LotNo = lotNo;
            Count = count;
        }

        public Guid Id { get; set; }
        public int Count { get; set; } = 0;

        public DateTimeOffset Timestamp { get; set; }
        public string LotNo { get; set; } = default!; // another key to search by
        public string Grade { get; set; } = default!;
        public string? Model { get; set; }
        public bool Archived { get; set; }
        public DateTimeOffset? ArchivedAt { get; set; }
        public Priority Priority { get; set; }
        public DateTimeOffset Expiration { get; set; } = DateTimeOffset.UtcNow.AddHours(24);

        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Audit> Audits { get; set; }
        public virtual ICollection<LotAssignment> Assignments { get; set; }
        public virtual ICollection<LotTask> Tasks { get; set; }

        public Note CreateNote(Employee employee, Department department, string data = "")
        {
            var note = new Note()
            {
                Data = data,
                CreatedBy = employee.Name,
                Department = department,
            };

            this.Notes.Add(note);
            return note;
        }

        public Audit CreateAudit(Employee employee, Department department, string type, string data = "")
        {
            var audit = new Audit()
            {
                Department = department,
                CreatedBy = employee.Name,
                Type = type,
                Data = data
            };

            this.Audits.Add(audit);
            return audit;
        }

        public LotTask CreateTask(string name, string category, int? templateId)
        {
            var task = new LotTask()
            {
                Name = name,
                Category = category,
                TemplateId = templateId
            };

            this.Tasks.Add(task);
            return task;
        }
    }
}