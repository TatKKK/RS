namespace RS.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string IdNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Discriminator {  get; set; }
       public string ImageUrl {  get; set; }    
    }

    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string IdNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Discriminator { get; set; }
      
        public IFormFile Image {  get; set; }

    }

   
}

