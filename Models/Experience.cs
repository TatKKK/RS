namespace RS.Models
{
    public class Experience
    {
        public int? ExperienceId { get; set; }
        public int? DoctorId {  get; set; }
        public string? CompanyName { get; set;}
        public string? Role {  get; set;}
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; } 
        public string? Description { get; set;}
    }
}
