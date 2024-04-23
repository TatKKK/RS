using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using RS.Models;
using System.Data;
using System.Drawing;
using System.Numerics;
using System.Xml.Linq;

namespace RS.Packages
{
    public interface IPKG_DOCTOR
    {
        public List<Doctor> Get_doctors();
        public Doctor Get_doctor_byEmail(string Email);
        public Doctor Get_doctor(int id);
        public void Add_doctor(Doctor doctor);
        public void Delete_doctor(int id);
        public void Edit_doctor(Doctor doctor, int id);
        public int Category_count(string category);
        public Task<IEnumerable<Doctor>> GetDoctorsByCategoryAsync(string category);
        public int UpdateViewCount(int id);
        public int GetDoctorViewCount(int doctorId);
        Task<(IEnumerable<Doctor>, int)> GetDoctorsPaginatedAsync(int pageNumber, int pageSize);

    }
    public class PKG_DOCTOR : PKG_BASE, IPKG_DOCTOR
    {
        IConfiguration config;

        public PKG_DOCTOR(IConfiguration config) : base(config)
        {
            this.config = config;
        }

        public async Task<(IEnumerable<Doctor>, int)> GetDoctorsPaginatedAsync(int pageNumber, int pageSize)
        {
            var doctors = new List<Doctor>();
            int totalCount = 0;

            using (var conn = new OracleConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new OracleCommand("c##tat.PKG_DOCS_SPEC.get_doctors_paginated", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_page_number", OracleDbType.Int32).Value = pageNumber;
                    cmd.Parameters.Add("p_page_size", OracleDbType.Int32).Value = pageSize;
                    cmd.Parameters.Add("p_total_count", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        var doctorMap = new Dictionary<int, Doctor>();
                        while (await reader.ReadAsync())
                        {
                            int doctorId = reader["id"] != DBNull.Value ? int.Parse(reader["id"].ToString()) : 0;
                            if (!doctorMap.TryGetValue(doctorId, out var doctor))
                            {
                                doctor = new Doctor
                                {
                                    Id = doctorId,
                                    Fname = reader["fname"].ToString(),
                                    Lname = reader["lname"].ToString(),
                                    IdNumber = reader["idNumber"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Discriminator = reader["discriminator"].ToString(),
                                    ImageUrl = reader["imageurl"].ToString(),
                                    Category = reader["category"].ToString(),
                                    Score = reader["score"] != DBNull.Value ? Decimal.Parse(reader["score"].ToString()) : 0,
                                    ViewCount = int.Parse(reader["viewcount"].ToString()),
                                    Experiences = new List<Experience>()
                                };
                                doctors.Add(doctor);
                                doctorMap[doctorId] = doctor;
                            }

                            if (!(reader["experience_id"] is DBNull))
                            {
                                var experience = new Experience
                                {
                                    ExperienceId = int.Parse(reader["experience_id"].ToString()),
                                    DoctorId = int.Parse(reader["doctor_id"].ToString()),
                                    CompanyName = reader["company_name"].ToString(),
                                    Role = reader["role"].ToString(),
                                    StartDate = reader["start_date"] is DBNull ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("start_date")),
                                    EndDate = reader["end_date"] is DBNull ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("end_date")),
                                    Description = reader["description"].ToString()
                                };
                                doctor.Experiences.Add(experience);
                            }
                        }
                    }
                    totalCount = Convert.ToInt32(cmd.Parameters["p_total_count"].Value.ToString());
                }
            }
            return (doctors, totalCount);
        }
        public List<Doctor> Get_doctors()
        {
            var doctorMap = new Dictionary<int, Doctor>();
            List<Doctor> doctors = new List<Doctor>();

            try
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_DOCS_SPEC.get_doctors", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MapDoctor(reader, doctorMap);
                            }
                        }
                    }
                }
                doctors = doctorMap.Values.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return doctors;
        }

        public Doctor Get_doctor_byEmail(string Email)
        {
            Doctor doctor = null;
            try
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_DOCS_SPEC.get_doctor_byEmail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.Parameters.Add("p_userId", OracleDbType.Decimal).Value = id;
                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = Email;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                doctor = new Doctor()
                                {
                                    Id = reader["id"] != DBNull.Value ? int.Parse(reader["id"].ToString()) : 0,
                                    IdNumber = reader["idNumber"] != DBNull.Value ? reader["idNumber"].ToString() : string.Empty,
                                    Fname = reader["fname"] != DBNull.Value ? reader["fname"].ToString() : string.Empty,
                                    Lname = reader["lname"] != DBNull.Value ? reader["lname"].ToString() : string.Empty,
                                    Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : string.Empty,
                                    Discriminator = reader["discriminator"] != DBNull.Value ? reader["discriminator"].ToString() : string.Empty,
                                    ImageUrl = reader["imageurl"] != DBNull.Value ? reader["imageurl"].ToString() : string.Empty
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return doctor;
        }

        public Doctor Get_doctor(int id)
        {
            Doctor doctor = null;

            var doctorMap = new Dictionary<int, Doctor>();

            try
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_DOCS_SPEC.get_doctor", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_userId", OracleDbType.Decimal).Value = id;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                doctor = MapDoctor(reader, doctorMap);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return doctor;
        }

        private Doctor MapDoctor(OracleDataReader reader, Dictionary<int, Doctor> doctorMap)

        {
            Console.WriteLine("Mapping doctor...");


            if (!doctorMap.TryGetValue(reader.GetInt32(reader.GetOrdinal("Id")), out var doctor))
            {
                doctor = new Doctor();
                {
                    doctor.Id = reader["id"] != DBNull.Value ? int.Parse(reader["id"].ToString()) : 0;
                    doctor.IdNumber = reader["idNumber"] != DBNull.Value ? reader["idNumber"].ToString() : string.Empty;
                    doctor.Fname = reader["fname"] != DBNull.Value ? reader["fname"].ToString() : string.Empty;
                    doctor.Lname = reader["lname"] != DBNull.Value ? reader["lname"].ToString() : string.Empty;
                    doctor.Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : string.Empty;
                    doctor.Category = reader["category"] != DBNull.Value ? reader["category"].ToString() : string.Empty;
                    doctor.Score = reader["score"] != DBNull.Value ? Decimal.Parse(reader["score"].ToString()) : 0;
                    doctor.ViewCount = reader["viewcount"] != DBNull.Value ? int.Parse(reader["viewcount"].ToString()) : 0;
                    doctor.Discriminator = reader["discriminator"] != DBNull.Value ? reader["discriminator"].ToString() : string.Empty;
                    doctor.ImageUrl = reader["imageurl"] != DBNull.Value ? reader["imageurl"].ToString() : string.Empty;
                    doctor.Experiences = new List<Experience>();
                };
                doctorMap.Add(doctor.Id, doctor);
                Console.WriteLine($"Mapped doctor: {doctor.Id}");
            }


            if (!(reader["experience_id"] is DBNull))
            {
                var experience = new Experience
                {
                    ExperienceId = reader["experience_id"] != DBNull.Value ? int.Parse(reader["experience_id"].ToString()) : 0,
                    DoctorId = reader["doctor_id"] != DBNull.Value ? int.Parse(reader["doctor_id"].ToString()) : 0,
                    CompanyName = reader["company_name"] != DBNull.Value ? reader["company_name"].ToString() : string.Empty,
                    Role = reader["role"] != DBNull.Value ? reader["role"].ToString() : string.Empty,
                    StartDate = (DateTime)(reader["start_date"] is DBNull ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("start_date"))),
                    EndDate = reader["end_date"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("end_date")) : (DateTime?)null,
                    Description = reader["description"].ToString()
                };
                doctor.Experiences.Add(experience);
                Console.WriteLine();
            }
            return doctor;
        }


        public void Add_doctor(Doctor doctor)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand("c##tat.PKG_DOCS_SPEC.add_doctor", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;

                    cmd.Parameters.Add("p_fname", OracleDbType.Varchar2).Value = doctor.Fname;
                    cmd.Parameters.Add("p_lname", OracleDbType.Varchar2).Value = doctor.Lname;
                    cmd.Parameters.Add("p_idnumber", OracleDbType.Varchar2).Value = doctor.IdNumber;
                    cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = doctor.Email;
                    cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = doctor.Password;
                    string discriminatorValue = string.IsNullOrEmpty(doctor.Discriminator) ? "doctor" : doctor.Discriminator;
                    cmd.Parameters.Add("p_discriminator", OracleDbType.Varchar2).Value = discriminatorValue;
                    cmd.Parameters.Add("p_category", OracleDbType.Varchar2).Value = doctor.Category;


                    cmd.Parameters.Add("p_imageurl", OracleDbType.Varchar2).Value = doctor.ImageUrl ?? string.Empty;

                    cmd.Parameters.Add("p_cvurl", OracleDbType.Varchar2).Value = doctor.CvUrl ?? string.Empty;

                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }


        public void Delete_doctor(int id)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();

                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_DOCS_SPEC.delete_doctor", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_UserId", OracleDbType.Int32).Value = id;

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


        public void Edit_doctor(Doctor doctor, int id)
        {
            Doctor currentDoctor = Get_doctor(id);
            if (currentDoctor != null)
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_DOCS_SPEC.edit_doctor", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.BindByName = true;

                        cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = id;
                        cmd.Parameters.Add("p_fname", OracleDbType.Varchar2).Value = doctor.Fname ?? currentDoctor.Fname;
                        cmd.Parameters.Add("p_lname", OracleDbType.Varchar2).Value = doctor.Lname ?? currentDoctor.Lname;
                        cmd.Parameters.Add("p_idNumber", OracleDbType.Varchar2).Value = doctor.IdNumber ?? currentDoctor.IdNumber;
                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = doctor.Email ?? currentDoctor.Email;
                        //cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = doctor.Password ?? currentDoctor.Password;
                        //cmd.Parameters.Add("p_category", OracleDbType.Varchar2).Value = doctor.Category ?? currentDoctor.Category;
                      
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }


        public int Category_count(string category)
        {
            int count = 0;
            using (OracleConnection conn = new OracleConnection(ConnStr))
            {
                conn.Open();

                using (OracleCommand cmd = new OracleCommand("c##tat.PKG_DOCS_SPEC.category_count", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;


                    OracleParameter pCategory = new OracleParameter("p_category", OracleDbType.Varchar2, ParameterDirection.Input);
                    pCategory.Value = category;
                    cmd.Parameters.Add(pCategory);


                    OracleParameter result = new OracleParameter("return_value", OracleDbType.Int32, ParameterDirection.ReturnValue);
                    cmd.Parameters.Add(result);

                    cmd.ExecuteNonQuery();


                    if (result.Value != DBNull.Value)
                    {
                        count = Convert.ToInt32(result.Value);
                    }
                }
            }
            return count;
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsByCategoryAsync(string category)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr))
            {
                await conn.OpenAsync();

                using (OracleCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "c##tat.PKG_DOCS_SPEC.getDoctorsByCategory";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_category", OracleDbType.Varchar2).Value = category;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    List<Doctor> doctors = new List<Doctor>();
                    using (OracleDataReader reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Doctor doctor = new Doctor
                            {
                                Id = reader["id"] != DBNull.Value ? int.Parse(reader["id"].ToString()) : 0,
                                Fname = reader["fname"] != DBNull.Value ? reader["fname"].ToString() : string.Empty,
                                Lname = reader["lname"] != DBNull.Value ? reader["lname"].ToString() : string.Empty,
                                ImageUrl = reader["imageurl"] != DBNull.Value ? reader["imageurl"].ToString() : string.Empty,
                                Category = reader["category"] != DBNull.Value ? reader["category"].ToString() : string.Empty,
                                Score = reader["score"] != DBNull.Value ? Decimal.Parse(reader["score"].ToString()) : 0
                            };
                            doctors.Add(doctor);
                        }
                    }

                    return doctors;
                }
            }


        }

        public int UpdateViewCount(int id)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr))
            {
                conn.Open();

                using (OracleCommand cmd = new OracleCommand("c##tat.PKG_DOCS_SPEC.update_viewcount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_doctorId", OracleDbType.Int32).Value = id;

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
            }

            return GetDoctorViewCount(id);
        }
        public int GetDoctorViewCount(int doctorId)
        {
            int viewCount = 0;

            using (OracleConnection conn = new OracleConnection(ConnStr))
            {
                conn.Open();

                using (OracleCommand cmd = new OracleCommand("SELECT viewcount FROM Doctors WHERE Id = :doctorId", conn))
                {

                    cmd.Parameters.Add(new OracleParameter("doctorId", OracleDbType.Int32)).Value = doctorId;

                    try
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            viewCount = Convert.ToInt32(result);
                        }
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
            }

            return viewCount;
        }


    }
}

