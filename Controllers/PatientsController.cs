using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RS.Models;
using RS.Packages;
using System.Text.Json;

namespace RS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : MainController
    {

        IPKG_PATIENT package;
       
        public PatientsController(IPKG_PATIENT package)
        {
            this.package = package;
        
        }
        

        [HttpPost]
        public async Task<IActionResult> AddPatient([FromForm] PatientDto patientDto, [FromForm] IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var patient = new Patient
                {
                    Fname = patientDto.Fname,
                    Lname = patientDto.Lname,
                    IdNumber = patientDto.IdNumber,
                    Email = patientDto.Email,
                    Password = patientDto.Password
                };

                if (image != null)
                {
                    patient.ImageUrl = await SaveImageAsync(image);
                }
             
                package.Add_patient(patient); 
                return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }

       

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadPath, fileName);


            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            await Console.Out.WriteLineAsync($"/images/{fileName}"); ;
            return $"/images/{fileName}";
        }
        [HttpGet]
        public IActionResult GetPatients()
        {
            List<Patient> patients = new List<Patient>();
            try
            {
                patients = package.Get_patients();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }
        [HttpGet("patient/email/{Email}")]
        [Authorize(Roles = "patient, admin")]

        public IActionResult GetPatientByEmail(string Email)
        {
            if(AuthUser == null)
            {
                return Unauthorized("User not Authorized");
            }
            try
            {
                Patient patient= package.Get_patient_byEmail(Email);
                if (patient == null)
                {
                    return NotFound($"Patient with EMail {Email} not found.");
                }
                return Ok(patient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }



        [HttpGet("patient/{id}")]
        [Authorize(Roles = "patient, admin")]

        public IActionResult GetPatient(int id)
        {
            if (AuthUser == null)
            {
                return Unauthorized("User not Authorized");
            }
            try
            {
                var patient = package.Get_patient(id);
                if (patient == null)
                {
                    return NotFound(); 
                }
                return Ok(patient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }


        [HttpDelete]
        public IActionResult DeletePatient(int id)
        {
            try
            {
                package.Delete_patient(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again: " + ex.Message);
            }
            return Ok();
        }

        [HttpPut]
        public IActionResult EditPatient(Patient patient, int id)
        {
            try
            {
                package.Edit_patient(patient, id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again: " + ex.Message);
            }
            return Ok();
        }
    }
}
