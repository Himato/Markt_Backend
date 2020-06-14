using System;
using Microsoft.AspNetCore.Identity;

namespace Markt.Models
{
    public sealed class UserRole : IdentityRole<string>
    {
        public const string Seller = "Seller";
        public const string Admin = "Admin";

        public UserRole() { }

        public UserRole(string roleName) : base(roleName)
        {
            Id = Guid.NewGuid().ToString();
            NormalizedName = roleName.ToUpper();
        }

        public UserRole(string id, string roleName) : base(roleName)
        {
            Id = id;
            NormalizedName = roleName.ToUpper();
        }
    }
}
