using Oracle.ManagedDataAccess.Client;
using RS.Models;
using System.Data;
using System.Numerics;

namespace RS.Packages
{

    public interface IPKG_APPPOINTMENTS
    {
        public void Create_Appointment(Appointment appointment);
        public void Update_Appointment_Status(Appointment appointment, int id);
        public void Delete_Appointment(int id);
        public List<Appointment> Get_Appointments_By_User(int userId);
        public List<Appointment> Get_Appointments_By_Doctor(int doctorId);
        public List<Appointment> Get_Appointments_By_Patient(int patientId);
        public List<Appointment> Get_Appointments();
        public class PKG_APPOINTMENT : PKG_BASE, IPKG_APPPOINTMENTS
        {
            IConfiguration config;

            public PKG_APPOINTMENT(IConfiguration config) : base(config)
            {
                this.config = config;
            }

            public void Create_Appointment(Appointment appointment)
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_APPOINTMENTS.create_appointment", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.BindByName = true;

                        cmd.Parameters.Add("p_doctorId", OracleDbType.Varchar2).Value = appointment.DoctorId;
                        cmd.Parameters.Add("p_patientId", OracleDbType.Varchar2).Value = appointment.PatientId;
                        cmd.Parameters.Add("p_startTime", OracleDbType.TimeStamp).Value = appointment.StartTime;

                        cmd.Parameters.Add("p_notes", OracleDbType.Varchar2).Value = appointment.Notes;
                        //cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = appointment.UserId;
                        cmd.Parameters.Add("p_isBooked", OracleDbType.Int32).Value = appointment.IsBooked ? 1 : 0;

                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

            }
            public void Update_Appointment_Status(Appointment appointment, int id)
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_APPOINTMENTS.update_appointment_status", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.BindByName = true;

                        cmd.Parameters.Add("p_appointmentId", OracleDbType.Int32).Value = appointment.Id;
                        cmd.Parameters.Add("p_isBooked", OracleDbType.Int32).Value = appointment.IsBooked ? 1 : 0;

                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

            }
            public void Delete_Appointment(int id)
            {
                try
                {
                    using (OracleConnection conn = new OracleConnection(ConnStr))
                    {
                        conn.Open();

                        using (OracleCommand cmd = new OracleCommand("c##tat.PKG_APPOINTMENTS.delete_appointment", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("p_appointmentId", OracleDbType.Int32).Value = id;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");

                    throw;
                }
            }

            public List<Appointment> Get_Appointments_By_User(int userId)
            {
                List<Appointment> appointments = new List<Appointment>();

                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    OracleCommand cmd = new OracleCommand("c##tat.PKG_APPOINTMENTS.GetAppointmentsByUser", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);

                    conn.Open();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Appointment appointment = new Appointment
                            {
                                Id = reader["appointment_id"] != DBNull.Value ? int.Parse(reader["appointment_id"].ToString()) : 0,
                                DoctorId = reader["doctor_id"] != DBNull.Value ? int.Parse(reader["doctor_id"].ToString()) : 0,
                                PatientId = reader["patient_id"] != DBNull.Value ? Convert.ToInt32(reader["patient_id"].ToString()) : 0,
                                StartTime = Convert.ToDateTime(reader["start_time"]),
                                Notes = reader["notes"].ToString(),
                                IsBooked = reader["isBooked"] != DBNull.Value ? Convert.ToInt32(reader["isBooked"].ToString()) == 1 : false,
                            };

                            appointments.Add(appointment);
                        }
                    }
                }

                return appointments;
            }

            public List<Appointment> Get_Appointments_By_Doctor(int doctorId)
            {
                List<Appointment> appointments = new List<Appointment>();

                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    OracleCommand cmd = new OracleCommand("c##tat.PKG_APPOINTMENTS.GetAppointmentsByDoctor", conn);
                    cmd.CommandType = CommandType.StoredProcedure;             

                    cmd.Parameters.Add("p_doctorId", OracleDbType.Int32).Value = doctorId;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);

                    conn.Open();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Appointment appointment = new Appointment
                            {
                                Id = reader["appointment_id"] != DBNull.Value ? int.Parse(reader["appointment_id"].ToString()) : 0,
                                DoctorId = int.Parse(reader["doctor_id"].ToString()),
                                PatientId = Convert.ToInt32(reader["PATIENT_ID"]),
                                StartTime = Convert.ToDateTime(reader["START_TIME"]),
                                Notes = reader["NOTES"].ToString(),
                                IsBooked = reader["isBooked"] != DBNull.Value ? Convert.ToInt32(reader["isBooked"].ToString()) == 1 : false,
                            };


                            appointments.Add(appointment);
                        }
                    }
                }

                return appointments;
            }

            public List<Appointment> Get_Appointments_By_Patient(int patientId)
            {
                List<Appointment> appointments = new List<Appointment>();

                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    OracleCommand cmd = new OracleCommand("c##tat.PKG_APPOINTMENTS.GetAppointmentsByPatient", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_patientId", OracleDbType.Int32).Value = patientId;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);

                    conn.Open();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Appointment appointment = new Appointment
                            {
                                Id = reader["appointment_id"] != DBNull.Value ? int.Parse(reader["appointment_id"].ToString()) : 0,
                                DoctorId = int.Parse(reader["doctor_id"].ToString()),
                                PatientId = Convert.ToInt32(reader["PATIENT_ID"]),
                                StartTime = Convert.ToDateTime(reader["START_TIME"]),
                                Notes = reader["NOTES"].ToString(),
                                IsBooked = reader["isBooked"] != DBNull.Value ? Convert.ToInt32(reader["isBooked"].ToString()) == 1 : false,
                            };


                            appointments.Add(appointment);
                        }
                    }
                }

                return appointments;
            }

            public List<Appointment> Get_Appointments()
            {
                List<Appointment> appointments = new List<Appointment>();

                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();

                    using (OracleCommand cmd = new OracleCommand("PKG_APPOINTMENTS.GetAppointments", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;


                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Appointment appointment = new Appointment
                                {
                                    Id = reader["appointment_id"] != DBNull.Value ? int.Parse(reader["appointment_id"].ToString()) : 0,
                                    DoctorId = reader["doctor_id"] != DBNull.Value ? int.Parse(reader["doctor_id"].ToString()) : 0,
                                    PatientId = reader["patient_id"] != DBNull.Value ? int.Parse(reader["patient_id"].ToString()) : 0,
                                    StartTime = Convert.ToDateTime(reader["START_TIME"]),
                                    Notes = reader["notes"] != DBNull.Value ? reader["notes"].ToString() : string.Empty,
                                    IsBooked = reader["isBooked"] != DBNull.Value ? Convert.ToInt32(reader["isBooked"].ToString()) != 0 : false,
                                    //UserId = reader["userid"] != DBNull.Value ? int.Parse(reader["userid"].ToString()) : 0,
                                };

                                appointments.Add(appointment);
                            }
                        }
                    }
                }

                return appointments;
            }


        }
    }
}
