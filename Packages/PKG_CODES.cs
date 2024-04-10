using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using RS.Models;
using System.Data;
using System.Xml.Linq;
using static RS.Models.ActivationCodess;

namespace RS.Packages
{
    public interface IPKG_CODES
    {
        public Task<string> CreateActivationCode(ActivationCodeRequest request);
        Task<IEnumerable<ActivationCode>> GetAllActivationCodes();
        Task<ActivationCode> GetActivationCode(string userEmail);
        Task<bool> VerifyActivationCode(VerifyCodeRequest request);


    }
    public class PKG_CODES : PKG_BASE, IPKG_CODES
    {
        IConfiguration config;
        public PKG_CODES(IConfiguration config) : base(config)
        {
            this.config = config;
        }
        public async Task<IEnumerable<ActivationCode>> GetAllActivationCodes()
        {
            var activationCodes = new List<ActivationCode>();

            using (var conn = new OracleConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new OracleCommand("pkg_codes.get_all_codes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.Default))
                    {
                        while (await reader.ReadAsync())
                        {                       
                            activationCodes.Add(new ActivationCode
                            {
                                CodeId = reader["codeid"] != DBNull.Value ? int.Parse(reader["codeid"].ToString()) : 0,
                                UserEmail = reader["userEmail"] != DBNull.Value ? reader["userEmail"].ToString() : string.Empty,
                                ActivationCode_ = reader["activationCode"] != DBNull.Value ? (reader["activationCode"].ToString()) : string.Empty,
                                GeneratedTime = Convert.ToDateTime(reader["generatedtime"]),
                                ExpirationTime = Convert.ToDateTime(reader["expirationtime"]),
                                IsValid = Convert.ToBoolean(reader["isValid"])
                            });
                        }
                    }
                }
            }
            return activationCodes;
        }

        public async Task<string> CreateActivationCode(ActivationCodeRequest request)
        {
            string activationCode;
            bool activationCodeExists;

            do
            {
                activationCode = Guid.NewGuid().ToString();
                var allActivationCodes = await GetAllActivationCodes();
                activationCodeExists = allActivationCodes.Any(ac => ac.ActivationCode_ == activationCode);
            } while (activationCodeExists);

            var newActivationCode = new ActivationCode
            {
                UserEmail = request.UserEmail,
                ActivationCode_ = activationCode,
                GeneratedTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddMinutes(30), 
                IsValid = false
            };

            await Add_code(newActivationCode);

            return activationCode;
        }

        public async Task Add_code(ActivationCode activationCode)
        {
            using (var conn = new OracleConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new OracleCommand("pkg_codes.add_code", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_userEmail", OracleDbType.Varchar2).Value = activationCode.UserEmail;
                    cmd.Parameters.Add("p_activationCode", OracleDbType.Varchar2).Value = activationCode.ActivationCode_;
                    cmd.Parameters.Add("p_generatedTime", OracleDbType.Date).Value = activationCode.GeneratedTime;
                    //cmd.Parameters.Add("p_expirationTime", OracleDbType.Date).Value = activationCode.ExpirationTime;

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


        public async Task<ActivationCode> GetActivationCode(string userEmail)
        {
            using (var conn = new OracleConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new OracleCommand("pkg_codes.get_code", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_userEmail", OracleDbType.Varchar2).Value = userEmail;
                    cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ActivationCode
                            {
                                
                                CodeId = Convert.ToInt32(reader["CodeId"]),
                                UserEmail = reader["UserEmail"].ToString(),
                                ActivationCode_ = reader["ActivationCode"].ToString(),
                                GeneratedTime = Convert.ToDateTime(reader["GeneratedTime"]),
                                ExpirationTime = Convert.ToDateTime(reader["ExpirationTime"]),
                                IsValid = Convert.ToBoolean(reader["IsValid"])
                            };
                        }
                    }
                }
            }

            return null; 
        }


        //public async Task RemoveExpiredCodes()
        //{
        //    using (var conn = new OracleConnection(ConnStr))
        //    {
        //        await conn.OpenAsync();
        //        using (var cmd = new OracleCommand("pkg_codes.removeExpiredCodes", conn))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            await cmd.ExecuteNonQueryAsync();
        //        }
        //    }
        //}

        public async Task<bool> VerifyActivationCode(VerifyCodeRequest  request)//მარტო ორ არგუმენტზე აფრენს
        {
            using (var conn = new OracleConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new OracleCommand("pkg_codes.verify_code", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_useremail", OracleDbType.Varchar2).Value = request.UserEmail;
                    cmd.Parameters.Add("p_activationcode", OracleDbType.Varchar2).Value = request.ActivationCode_;
                    var p_isvalid = new OracleParameter("p_isValid", OracleDbType.Int32)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(p_isvalid);

                    await cmd.ExecuteNonQueryAsync();
                   
                    int isValidInt = ((Oracle.ManagedDataAccess.Types.OracleDecimal)p_isvalid.Value).ToInt32();

                    bool isValid = isValidInt == 1;

                    return isValid;
                }
            }
        }
    }
}
