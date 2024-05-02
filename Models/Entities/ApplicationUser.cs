using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Identity;

namespace MyCourse.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}