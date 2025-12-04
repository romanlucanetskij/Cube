namespace DripCube.Entities
{
    public class Report
    {
        public int Id { get; set; }
        public Guid ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;

        public string TargetType { get; set; } = string.Empty;
        public string TargetIdentifier { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}