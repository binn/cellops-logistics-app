using System.Text.Json.Serialization;

namespace AngelPhoneTrack.Data
{
    public class LotTask
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Category { get; set; } = default!;
        public bool Completed { get; set; } = false;

        [JsonIgnore]
        public Lot Lot { get; set; } = default!;
    }
}
