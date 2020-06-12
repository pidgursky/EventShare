﻿using Microsoft.AspNetCore.Identity;

namespace EventShare.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
