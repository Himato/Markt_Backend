using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Markt.Models;

namespace Markt.Dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        [Required, MinLength(4, ErrorMessage = "Username can't be less than 4 letters"), MaxLength(32, ErrorMessage = "Username can't be more than 32 letters")]
        [RegularExpression(@"^[\w]+$", ErrorMessage = "Name can only contain English letters, underscores, and numbers")]
        public string Username { get; set; }

        [Required, MinLength(4, ErrorMessage = "First Name can't be less than 4 letters"), MaxLength(32, ErrorMessage = "First Name can't be more than 32 letters")]
        [RegularExpression(@"^[a-zA-Z_\s]+$", ErrorMessage = "Name can only contain English letters, underscores, and spaces")]
        public string FirstName { get; set; }

        [Required, MinLength(4, ErrorMessage = "Last Name can't be less than 4 letters"), MaxLength(32, ErrorMessage = "Last Name can't be more than 32 letters")]
        [RegularExpression(@"^[a-zA-Z_\s]+$", ErrorMessage = "Name can only contain English letters, underscores, and spaces")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdateEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class UpdatePasswordDto
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddressDto
    {
        public int Id { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string PostCode { get; set; }

        public static AddressDto Create(BillingAddress address)
        {
            return new AddressDto
            {
                Id = address.Id,
                Address = address.Address,
                City = address.City,
                Country = address.Country,
                PostCode = address.PostCode
            };
        }
    }

    public class UserDto
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Type { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public int NumberOfProducts { get; set; }

        public int NumberOfFinishedPurchases { get; set; }

        public int NumberOfAwaitPurchases { get; set; }

        public double TotalPaidMoney { get; set; }

        public double TotalAwaitMoney { get; set; }

        public double TotalGainedMoney { get; set; }

        public static UserDto Create(ApplicationUser user, string type, int numberOfProducts, List<Purchase> purchases, double totalGainedMoney)
        {
            var finishedPurchases = purchases.Where(p => p.IsFinished).ToList();
            var awaitPurchases = purchases.Where(p => !p.IsFinished).ToList();

            return new UserDto
            {
                Name = user.GetName(),
                Email = user.Email,
                Type = type,
                CreatedDateTime = user.CreatedAt,
                NumberOfProducts = numberOfProducts,
                NumberOfFinishedPurchases = finishedPurchases.Count,
                NumberOfAwaitPurchases = awaitPurchases.Count,
                TotalPaidMoney = finishedPurchases.Select(p => p.TotalPrice).Aggregate(0d, (current, next) => current + next),
                TotalAwaitMoney = awaitPurchases.Select(p => p.TotalPrice).Aggregate(0d, (current, next) => current + next),
                TotalGainedMoney = totalGainedMoney
            };
        }
    }
}
