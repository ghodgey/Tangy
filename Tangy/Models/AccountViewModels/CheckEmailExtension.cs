using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tangy.Models.AccountViewModels
{

    public class CheckEmailExtension : ValidationAttribute
    {


        private string _extension;

        public CheckEmailExtension(string extension)
        {
            _extension = extension;
        }



        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            RegisterViewModel register = (RegisterViewModel)validationContext.ObjectInstance;



            if (!register.Email.Contains("@gmail"))
            {
                return new ValidationResult("The email has to be associated to gmail.");
            }

            return ValidationResult.Success;
        }

        



    }

}
