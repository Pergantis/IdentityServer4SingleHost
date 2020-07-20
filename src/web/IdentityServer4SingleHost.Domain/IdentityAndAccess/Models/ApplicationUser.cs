using System;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer4SingleHost.Domain.IdentityAndAccess.Models
{
    public class ApplicationUser: IdentityUser
    {
        public bool IsAdmin { get; set; }
       
        public string DataEventRecordsRole { get; set; }
        
        public string SecuredFilesRole { get; set; }

        public DateTime AccountExpires { get; set; }
    }
}
