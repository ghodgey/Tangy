﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tangy.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public string StatusMessage { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Lockout Reason")]
        public string LockoutReason { get; set; }

        [Display(Name = "Lockout End date")]
        public DateTimeOffset? LockoutEnd { get; set; }

        [Display(Name = "Access Fail Count")]
        public int AccessFailedCount { get; set; }


    }
}
