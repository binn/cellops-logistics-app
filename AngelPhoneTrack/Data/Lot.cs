using Microsoft.EntityFrameworkCore;

namespace AngelPhoneTrack.Data
{
    [Index(nameof(LotNo))]
    public class Lot
    {
        public Lot(string lotNo = null!)
        {
            Assignments = new HashSet<LotAssignment>();
            Timestamp = DateTimeOffset.UtcNow;
            Audits = new HashSet<Audit>();
            Notes = new HashSet<Note>();
            LotNo = lotNo;
        }

        public Guid Id { get; set; }
        public int Count { get; set; } = 0;

        public DateTimeOffset Timestamp { get; set; }
        public string LotNo { get; set; } = default!; // another key to search by

        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Audit> Audits { get; set; }
        public virtual ICollection<LotAssignment> Assignments { get; set; }

        public Note CreateNote(Employee employee, Department department, string data)
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

        public Audit CreateAudit(string type, string data)
        {
            var audit = new Audit()
            {
                Type = type,
                Data = data
            };

            this.Audits.Add(audit);
            return audit;
        }
    }
}