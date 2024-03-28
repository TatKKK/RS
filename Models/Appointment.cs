namespace RS.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        
       
        public DateTime StartTime { get; set; }

        public DateTime EndTime
        {
            get { return StartTime.AddHours(1); }
        }
      public Appointment() { }
       
        public Appointment(DateTime startTime)
        {
            StartTime = startTime;
        }
        public string Notes { get; set; }
        public bool IsBooked {  get; set; }
        //public int UserId {  get; set; }

       
    }
}
