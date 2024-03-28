namespace RS.Models
{
   
    public class Doctor : User
    {
        public string Category { get; set; }
       
        public decimal? Score { get; set; }
        public List<Experience>? Experiences { get; set; }

        public Doctor()
        {
            Experiences = new List<Experience>();
        }

        public int ViewCount {  get; set; }
        public string CvUrl {  get; set; }  


    }

    public class DoctorDto : UserDto
    {
        public string Category { get; set; }
        public IFormFile Cv { get; set; }

    }
}
