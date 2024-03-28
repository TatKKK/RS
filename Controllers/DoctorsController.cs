using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RS.Models;
using RS.Packages;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : MainController
    {

        IPKG_DOCTOR package;
      

        public DoctorsController(IPKG_DOCTOR package)
        {
            this.package = package;
          
        }


        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddDoctor([FromForm] DoctorDto doctorDto)

        {
            //Console.WriteLine($"Received doctor data: {JsonSerializer.Serialize(doctorDto)}");

            var errors = new List<string>();
            if (string.IsNullOrEmpty(doctorDto.Fname)) errors.Add("Fname is required.");
            if (string.IsNullOrEmpty(doctorDto.Lname)) errors.Add("Lname is required.");
            if (string.IsNullOrEmpty(doctorDto.Email)) errors.Add("Email is required.");
            if (string.IsNullOrEmpty(doctorDto.Password)) errors.Add("Password is required.");

            if (AuthUser == null)
            {
                return Unauthorized("User not Authorized");
            }
            try
            {
                var doctor = new Doctor
                {
                    Fname = doctorDto.Fname,
                    Lname = doctorDto.Lname,
                    IdNumber = doctorDto.IdNumber,
                    Email = doctorDto.Email,
                    Password = doctorDto.Password, 
                    Category = doctorDto.Category
                };
                if (doctorDto.Image != null)
                {
                    string imageUrl = await SaveFileAsync(doctorDto.Image, "images");
                    doctor.ImageUrl = imageUrl;
                }
                if (doctorDto.Cv != null)
                {
                    string cvUrl = await SaveFileAsync(doctorDto.Cv, "cvs");
                    doctor.CvUrl = cvUrl;
                }
                package.Add_doctor(doctor);
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again: " + ex.Message);
            }
        }



        private async Task<string> SaveFileAsync(IFormFile file, string subDirectory)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{subDirectory}");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/{subDirectory}/{fileName}";
        }

        [HttpGet("docsPaginate")]
        public async Task<IActionResult> GetDoctorsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 6)
        {
            try
            {
                var (doctors, totalCount) = await package.GetDoctorsPaginatedAsync(pageNumber, pageSize);

                if (doctors == null || !doctors.Any())
                {
                    return NotFound("No doctors found.");
                }

                // Create a response object that includes the data and pagination details
                var response = new
                {
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    Doctors = doctors
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again: " + ex.Message);
            }
        }



        [HttpGet("docs")]
      
        public IActionResult GetDoctors()
        {
            List<Doctor> doctors = new List<Doctor>();
            try
            {
                doctors = package.Get_doctors();
                if (doctors.Count == 0)
                {
                    return NotFound("No doctors found.");
                }
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }
        [HttpGet("doctor/{id}")]


        public IActionResult GetDoctor(int id)
        {
            try
            {
                Doctor doctor = package.Get_doctor(id);
                if (doctor == null)
                {
                    return NotFound($"Doctor with Id {id} not found.");
                }
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }
        [HttpGet("doctor/email/{Email}")]
        [Authorize(Roles ="admin, patient, doctor")]
       

        public IActionResult GetDoctorByEmail(string Email)
        {
            try
            {
                Doctor doctor = package.Get_doctor_byEmail(Email);
                if (doctor == null)
                {
                    return NotFound($"Doctor with EMail {Email} not found.");
                }
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }

        [HttpDelete("deletedoctor/{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteDoctor(int id)
        {
            if (AuthUser == null)
            {
                return NotFound("User not authenticated.");
            }
            try
            {
                package.Delete_doctor(id);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again: " + ex.Message);
            }
            return Ok();
        }

        [HttpPut]
        [Authorize]
        public IActionResult Edit_doctor(Doctor doctor, int id)
        {
            try
            {
                package.Edit_doctor(doctor, id);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again: " + ex.Message);
            }
            return Ok();
        }

        //ალბათ არ მჭირდება
        //[HttpGet("docsby")]
        //public IActionResult CategoryCount(string category)
        //{
        //    int count = 0;
        //    try
        //    {
        //        package.Category_count(category);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again: " + ex.Message);
        //    }
        //    return Ok(count);
        //}

        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctorsByCategory(string category)
        {
            try
            {
                var doctors = await package.GetDoctorsByCategoryAsync(category);
                if (doctors == null) return NotFound();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again: " + ex.Message);
            }
        }

        [HttpGet("viewCount/{id}")] 
        public ActionResult UpdateViewCount(int id)
        {
            try
            {
               
                package.UpdateViewCount(id);
              
                int updatedViewCount = package.GetDoctorViewCount(id);
                
                return Ok(updatedViewCount);
            }
            catch (Exception ex)
            {              
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the view count.");
            }
        }

    }
}
