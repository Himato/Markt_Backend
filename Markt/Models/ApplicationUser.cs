using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Markt.Models
{
    public class ApplicationUser : IdentityUser<string>
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public ApplicationUser()
        {
            CreatedAt = DateTime.Now;
        }

        public string GetName()
        {
            return FirstName + " " + LastName;
        }
    }
}
