namespace RS.Models
{
    public class ActivationCodess
    {


        public class ActivationCodeRequest
        {
            public string UserEmail { get; set; }
        }

        public class VerifyCodeRequest
        {
            public string UserEmail { get; set; }
            public string ActivationCode_ { get; set; }
        }

        public class ActivationCode
        {
            public int CodeId { get; set; }
            public string UserEmail { get; set; }
            public string ActivationCode_ { get; set; }
            public DateTime GeneratedTime { get; set; }
            public DateTime ExpirationTime { get; set; }
            public bool IsValid { get; set; }

        }
    }
}
