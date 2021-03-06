﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Tangy.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

       
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Lockout Reason")]
        public string LockoutReason { get; set; }

    }
}
