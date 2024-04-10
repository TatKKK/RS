using Oracle.ManagedDataAccess.Client;

namespace RS.Packages
{
    public interface IPKG_LOGS
    {
        public void Add_log(string message, string? email=null);
    }
    public class PKG_LOGS:PKG_BASE, IPKG_LOGS
    {
        IConfiguration config;

        public PKG_LOGS(IConfiguration config) : base(config)
        {
            this.config = config;
        }

        public void Add_log(string message, string? email = null)
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = ConnStr;
            conn.Open();

            OracleCommand cmd = new OracleCommand();

            cmd.Connection = conn;
            cmd.CommandText = "c##tat.PKG_LOGS_SPEC.add_log";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("p_message", OracleDbType.Varchar2).Value = message;
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email;           
            cmd.Parameters.Add("p_discriminator", OracleDbType.Varchar2).Value = email;

            cmd.ExecuteNonQuery();

            conn.Close();
        }
    }
   
}
