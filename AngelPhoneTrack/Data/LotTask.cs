using System.Text.Json.Serialization;

namespace AngelPhoneTrack.Data
{
    public class LotTask
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Category { get; set; } = default!;
        public bool Completed { get; set; } = false;
        public int? TemplateId { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        [JsonIgnore]
        public Lot Lot { get; set; } = default!;
    }
}
