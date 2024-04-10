using Oracle.ManagedDataAccess.Client;
using RS.Models;
using System.Data;
using System.Xml.Linq;

namespace RS.Packages
{
    public interface IPKG_USER
    {
        public User Get_user_byEmail(string email);
        public List<User> Get_users();
        public User Get_user(int id);   
        public void Add_user(User user);
        public void Delete_user(int id);
        public void Update_password(string email, string password);
        public void Edit_user(User user, int id);

        public User Authenticate(Login loginData);
    }
    public class PKG_USER : PKG_BASE, IPKG_USER
    {
        IConfiguration config;

        public PKG_USER(IConfiguration config) : base(config)
        {
            this.config = config;
        }


        public User Authenticate(Login loginData)
        {

            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = ConnStr;

            conn.Open();

            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "c##tat.PKG_USERS_SPEC.authenticate";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = loginData.Email;
            cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = loginData.Password;
            cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            OracleDataReader reader = cmd.ExecuteReader();

            User user = null;

            if (reader.Read())
            {
                user = new User();

                user.Id = reader["Id"] != DBNull.Value ? int.Parse(reader["Id"].ToString()) : 0;
                user.Fname = reader["Fname"] != DBNull.Value ? reader["Fname"].ToString() : string.Empty;
                user.Lname = reader["Lname"] != DBNull.Value ? reader["Lname"].ToString() : string.Empty;
                user.Discriminator = reader["Discriminator"] != DBNull.Value ? reader["Discriminator"].ToString() : string.Empty;
                user.ImageUrl = reader["ImageUrl"] != DBNull.Value ? reader["ImageUrl"].ToString() : string.Empty;

            }

            conn.Close();
            return user;
        }

        public User Get_user_byEmail(string email)
        {
            User user = null;

            try
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_USERS_SPEC.get_user_byEmail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = MapUser(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return user;
        }
        public  List<User> Get_users()
        {
            List<User>users = new List<User>();

            try
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_USERS_SPEC.get_users", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(MapUser(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return users;
        }
       
        public User Get_user(int id)
        {
           User user = null;

            try
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_USERS_SPEC.get_user", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_userId", OracleDbType.Decimal).Value = id;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = MapUser(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return user;
        }
        private User MapUser(OracleDataReader reader)
        {
            return new User
            {
                Id = reader["Id"] != DBNull.Value ? int.Parse(reader["Id"].ToString()) : 0,
                Fname = reader["Fname"] != DBNull.Value ? reader["Fname"].ToString() : string.Empty,
                Lname = reader["Lname"] != DBNull.Value ? reader["Lname"].ToString() : string.Empty,
                IdNumber = reader["IdNumber"] != DBNull.Value ? reader["IdNumber"].ToString() : string.Empty,
                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty,
                Discriminator = reader["Discriminator"] != DBNull.Value ? reader["Discriminator"].ToString() : string.Empty,
                ImageUrl = reader["imageurl"] != DBNull.Value ? reader["imageurl"].ToString() : string.Empty
        };
        }

        public void Add_user(User user)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand("c##tat.PKG_USERS_SPEC.add_user", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_fname", OracleDbType.Varchar2).Value = user.Fname;
                    cmd.Parameters.Add("p_lname", OracleDbType.Varchar2).Value = user.Lname;
                    cmd.Parameters.Add("p_idnumber", OracleDbType.Varchar2).Value = user.IdNumber;
                    cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = user.Email;
                    cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = user.Password;
                    cmd.Parameters.Add("p_discriminator", OracleDbType.Varchar2).Value = user.Discriminator;

                    cmd.Parameters.Add("p_imageurl", OracleDbType.Varchar2).Value = user.ImageUrl ?? string.Empty;

                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }


        public void Delete_user(int id)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();

                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_USERS_SPEC.delete_user", conn))
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


        public void Edit_user(User user, int id)
        {
            User currentUser = Get_user(id);
            if (currentUser != null)
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_USERS_SPEC.edit_user", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.BindByName = true;
                        cmd.Parameters.Add("p_userId", OracleDbType.Int32).Value = id;
                        cmd.Parameters.Add("p_fname", OracleDbType.Varchar2).Value = user.Fname ?? currentUser.Fname;
                        cmd.Parameters.Add("p_lname", OracleDbType.Varchar2).Value = user.Lname ?? currentUser.Lname;
                        cmd.Parameters.Add("p_idNumber", OracleDbType.Varchar2).Value = user.IdNumber ?? currentUser.IdNumber;
                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = user.Email ?? currentUser.Email;
                        cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = user.Password ?? currentUser.Password;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }

        public void Update_password(string email, string password)
        {
            User currentUser = Get_user_byEmail(email);
            if (currentUser != null)
            {
                using (OracleConnection conn = new OracleConnection(ConnStr))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("c##tat.PKG_USERS_SPEC.update_password", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;                       
                        cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email ?? currentUser.Email;
                        cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = password ?? currentUser.Password;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }
    }
    }
