namespace RS.Packages
{
    public class PKG_BASE
    {
        string connStr;

        IConfiguration config;

        public PKG_BASE(IConfiguration config)
        {
            this.config = config;
            connStr = this.config.GetConnectionString("OrclConnStr");
        }

        protected string ConnStr
        {
            get { return connStr; }
        }
    }
}
