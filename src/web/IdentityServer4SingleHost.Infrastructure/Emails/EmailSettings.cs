using System;

namespace IdentityServer4SingleHost.Infrastructure.Emails
{
    public class EmailSettings
    {
        public string PrimaryDomain { get; set; }

        public int PrimaryPort { get; set; }

        public String UsernameEmail { get; set; }

        public String UsernamePassword { get; set; }

        public String FromEmail { get; set; }

        public String ToEmail { get; set; }

        public String CcEmail { get; set; }

        public String BccEmail { get; set; }
    }
}
