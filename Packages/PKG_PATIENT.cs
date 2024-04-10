using Oracle.ManagedDataAccess.Client;
using RS.Models;
using System.Data;
using System.Numerics;

namespace RS.Packages
{
    public interface IPKG_PATIENT
    {
        public void Add_patient(Patient patient);
        public void Delete_patient(int id);

        public List<Patient> Get_patients();
        public Patient Get_patient(int id);
        public Patient Get_patient_byEmail(string email);

        public void Edit_patient(Patient patient, int id);

        //public User Authenticate(Login logindata);


    }
    public class PKG_PATIENT : PKG_BASE, IPKG_PATIENT
    {
        IConfiguration config;
        public PKG_PATIENT(IConfiguration config) : base(config)
        {
            this.config = config;
        }


        public void Delete_patient(int id)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = ConnStr;
            conn.Open();
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "c##tat.PKG_PATIENTS_SPEC.delete_patient";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = id;

            cmd.ExecuteNonQuery();
            conn.Close();

        }


        public void Add_patient(Patient patient)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = ConnStr;
            conn.Open();
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "c##tat.PKG_PATIENTS_SPEC.add_patient";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_fname", OracleDbType.Varchar2).Value = patient.Fname;
            cmd.Parameters.Add("p_lname", OracleDbType.Varchar2).Value = patient.Lname;
            cmd.Parameters.Add("p_idnumber", OracleDbType.Varchar2).Value = patient.IdNumber;
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = patient.Email;
            cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = patient.Password;
            string discriminatorValue = string.IsNullOrEmpty(patient.Discriminator) ? "patient" : patient.Discriminator;
            cmd.Parameters.Add("p_discriminator", OracleDbType.Varchar2).Value = discriminatorValue;

            cmd.Parameters.Add("p_imageurl", OracleDbType.Varchar2).Value = patient.ImageUrl ?? string.Empty;

            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public List<Patient> Get_patients()
        {
            List<Patient> patients = new List<Patient>();
            OracleConnection conn = new OracleConnection(ConnStr);
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            conn.Open();

            cmd.CommandText = "c##tat.PKG_PATIENTS_SPEC.get_patients";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);
            OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                patients.Add(MapPatient(reader));
            }
            conn.Close();
            return patients;
        }
        public Patient Get_patient_byEmail(string Email)
        {
            Patient patient = null;
            try
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_PATIENTS_SPEC.get_patient_byEmail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.Parameters.Add("p_userId", OracleDbType.Decimal).Value = id;
                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = Email;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                patient=new Patient()
                                {
                                    Id = reader["id"] != DBNull.Value ? int.Parse(reader["id"].ToString()) : 0,
                                    IdNumber = reader["idNumber"] != DBNull.Value ? reader["idNumber"].ToString() : string.Empty,
                                    Fname = reader["fname"] != DBNull.Value ? reader["fname"].ToString() : string.Empty,
                                    Lname = reader["lname"] != DBNull.Value ? reader["lname"].ToString() : string.Empty,
                                    Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : string.Empty,
                                    //Discriminator = reader["discriminator"] != DBNull.Value ? reader["discriminator"].ToString() : string.Empty,
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

            return patient;
        }
        public Patient Get_patient(int id)
        {
            Patient patient = null;

            OracleConnection conn = new OracleConnection(ConnStr);
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            conn.Open();

            cmd.CommandText = "c##tat.PKG_PATIENTS_SPEC.get_patient";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = id;
            cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                patient = MapPatient(reader);
            }
            conn.Close();
            return patient;
        }
        private Patient MapPatient(OracleDataReader reader)
        {
            return new Patient
            {
                Id = reader["id"] != DBNull.Value ? int.Parse(reader["id"].ToString()) : 0,
                Fname = reader["fname"] != DBNull.Value ? reader["fname"].ToString() : string.Empty,
                Lname = reader["lname"] != DBNull.Value ? reader["lname"].ToString() : string.Empty,
                IdNumber = reader["idNumber"] != DBNull.Value ? reader["idNumber"].ToString() : string.Empty,
                Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : string.Empty,
                //Discriminator = reader["discriminator"] != DBNull.Value ? reader["discriminator"].ToString() : string.Empty,
                ImageUrl = reader["imageurl"] != DBNull.Value ? reader["imageurl"].ToString() : string.Empty
            };
        }
        public void Edit_patient(Patient patient, int id)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = ConnStr;
            conn.Open();
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "c##tat.PKG_PATIENTS_SPEC.edit_patient";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = patient.Id;
            cmd.Parameters.Add("p_fname", OracleDbType.Varchar2).Value = patient.Fname;
            cmd.Parameters.Add("p_lname", OracleDbType.Varchar2).Value = patient.Lname;
            cmd.Parameters.Add("p_idnumber", OracleDbType.Varchar2).Value = patient.IdNumber;
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = patient.Email;
            cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = patient.Password;
            string discriminatorValue = string.IsNullOrEmpty(patient.Discriminator) ? "default_value" : patient.Discriminator;
            cmd.Parameters.Add("p_discriminator", OracleDbType.Varchar2).Value = discriminatorValue;


            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                throw new Exception("Patient not found or no update needed.");
            }
            conn.Close();
        }

        public async Task InsertActivationCode(int userId, string activationCode, DateTime expirationTime)
        {
            using (var conn = new OracleConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new OracleCommand("pkg_codes.insert_activation_code", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_activationCode", OracleDbType.Varchar2).Value = activationCode;
                    cmd.Parameters.Add("p_expirationTime", OracleDbType.Date).Value = expirationTime;

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


    }

}
