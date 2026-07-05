namespace Crime_analysis.Models
{
    public class Suspect
    {
        public int SuspectId { get; set; }
        public int CrimeId { get; set; }
        public string? FullName { get; set; }
        public string? Age { get; set; }
        public string? Gender     { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
    }
}