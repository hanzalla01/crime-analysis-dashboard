namespace Crime_analysis.Models
{
    public class Crime
    {
        public int CrimeId { get; set; }
        public string? TypeName { get; set; }
        public string? AreaName { get; set; }
        public string? City { get; set; }
        public string? CrimeDate { get; set; }
        public string? Severity { get; set; }
        public string? Description { get; set; }
        public string? OfficerName { get; set; }
        public string? Status { get; set; }
    }
}