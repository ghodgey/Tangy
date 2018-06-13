using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Tangy.Services;
using Tangy.Utility;

namespace Tangy.Services
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }

        public static Task SendOrderStatusAsync(this IEmailSender emailSender, string email, string orderNumber, string status)
        {
            string subject = "";
            string message = "";

            if(status == SD.StatusCancelled)
            {
                subject = "Order Cancelled";
                message = "Order number " + orderNumber + " has been cancelled. Please contact us if you have any questions.";
            }
            if (status == SD.StatusSubmitted)
            {
                subject = "Order Created Successfully";
                message = "Order number " + orderNumber + " has been created successfully.";
            }
            if (status == SD.StatusReady)
            {
                subject = "Order Ready for Pickup";
                message = "Order number " + orderNumber + " is ready for pickup.";
            }


            return emailSender.SendEmailAsync(email, subject, message);


        }


    }
}
