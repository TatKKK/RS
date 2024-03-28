using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RS.Auth;
using RS.Models;
using RS.Packages;
using static RS.Packages.IPKG_APPPOINTMENTS;

namespace RS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AppointmentsController : MainController
    {
        IPKG_APPPOINTMENTS package;

        private readonly IJwtManager jwtManager;

        public AppointmentsController(IPKG_APPPOINTMENTS package, IJwtManager jwtManager)
        {
            this.package = package;
            this.jwtManager = jwtManager;
        }

        [HttpPost("create")]
        [Authorize(Roles = "patient, doctor, admin")]
        public IActionResult CreateAppointment(Appointment appointment)
        {
            {
                if (AuthUser == null)
                {
                    return Unauthorized("User not authenticated.");
                }
                try
                {
                    if (string.IsNullOrEmpty(appointment.Notes))
                    {
                        return BadRequest("Visit reason should be described!");
                    }
                    //appointment.UserId = AuthUser.Id;

                    package.Create_Appointment(appointment);
                    return Ok(appointment);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
                }
            }
        }
        [HttpGet("doctor/{doctorId}")]

        public IActionResult GetAppointmentsByDoctors(int doctorId)
        {
            List<Appointment> appointments;
            try
            {
                appointments = package.Get_Appointments_By_Doctor(doctorId);
                return Ok(appointments);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }

        [HttpGet("patient/{patientId}")]
        [Authorize]

        public IActionResult GetAppointmentsByPatients(int patientId)
        {
            List<Appointment> appointments;
            try
            {
                appointments = package.Get_Appointments_By_Patient(patientId);
                return Ok(appointments);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
        }

        [HttpPut("update/{id}")]
        [Authorize]

        public IActionResult UpdateAppointmentStatus(Appointment appointment, int id)
        {
            try
            {
                package.Update_Appointment_Status(appointment, id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        [Authorize]

        public IActionResult DeleteAppointment(int id)
        {
            try
            {
                package.Delete_Appointment(id);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, "System error, try again");
            }
            return Ok();
        }


    }
}
