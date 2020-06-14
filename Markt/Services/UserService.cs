using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Markt.Dtos;
using Markt.Helpers;
using Markt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Markt.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetUsers();

        /// <returns>Authentication Token</returns>
        Task<string> Authenticate(string email, string password);

        /// <summary>
        /// Creates a new user, and then log him in.
        /// </summary>
        /// <returns>Authentication Token</returns>
        Task<string> Create(RegisterDto registerDto);

        Task<ApplicationUser> GetUserById(string id);

        Task<ApplicationUser> GetUserByEmail(string email);

        Task<ApplicationUser> GetUserByUsername(string username);

        Task ForgetPassword(string email);

        /// <summary>
        /// Checks for the token validation, reset the password, and log the user in.
        /// </summary>
        /// <returns>Authentication Token</returns>
        Task<string> ResetPassword(ResetPasswordDto resetPasswordDto);

        Task<string> UpdateEmail(string userId, string email);
            
        Task<string> UpdatePassword(string userId, string oldPassword, string newPassword);

        Task<string> Upgrade(string userId);

        Task<int> AddBillingAddress(string userId, BillingAddressDto billingAddressDto);

        IEnumerable<AddressDto> GetBillingAddresses(string userId);

        Task<bool> IsBillingAddress(string userId, int billingAddressId);
    }

    public class UserService : ServiceHelper, IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _sender;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context, IConfiguration configuration, IEmailSender sender) : base(context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _sender = sender;
        }

        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            var users = await _context.Users.Take(100).ToListAsync();

            var output = new List<UserDto>();

            foreach (var user in users)
            {
                var isSeller = await _userManager.IsInRoleAsync(user, UserRole.Seller);
                var isAdmin = await _userManager.IsInRoleAsync(user, UserRole.Admin);

                var products = isSeller || isAdmin ? await _context.Products.Where(p => p.SellerId.Equals(user.Id)).Select(p => p.Id).ToListAsync() : null;

                var purchases = await _context.Purchases.Where(p => p.UserId.Equals(user.Id)).ToListAsync();

                var gainedPurchases = isSeller || isAdmin ? await _context.Purchases
                    .Where(p => products.Contains(p.ProductId) && p.IsFinished)
                    .Select(p => p.TotalPrice).ToListAsync() : null;

                var gained = gainedPurchases?.Aggregate(0d, (current, next) => current + next) ?? 0;

                output.Add(UserDto.Create(user, 
                    isSeller ? UserRole.Seller : isAdmin ? UserRole.Admin : "User",
                    products?.Count ?? 0, purchases, gained));
            }

            return output;
        }

        public async Task<string> Authenticate(string email, string password)
        {
            var user = await SignInByEmail(email, password);

            if (user == null)
            {
                throw new AuthenticationException();
            }

            return await GenerateJwtToken(user);
        }

        public async Task<string> Create(RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                return await Authenticate(user.Email, registerDto.Password);
            }

            throw new ArgumentException(result.Errors.FirstOrDefault()?.Description);
        }

        public async Task<ApplicationUser> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser> GetUserByUsername(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public async Task ForgetPassword(string email)
        {
            var user = await GetUserByEmail(email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _sender.SendEmailAsync(user.Email, "Reset your password", token);
        }

        public async Task<string> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await GetUserByEmail(resetPasswordDto.Email);

            if (user == null)
            {
                throw new ArgumentException("Couldn't validate the token");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);

            if (result.Succeeded)
            {
                return await Authenticate(user.Email, resetPasswordDto.Password);
            }

            throw new ArgumentException(result.Errors.FirstOrDefault()?.Description);
        }

        public async Task<string> UpdateEmail(string userId, string email)
        {
            var user = await GetUserById(userId);

            if (user == null)
            {
                throw new AuthenticationException();
            }

            var result = await _userManager.SetEmailAsync(user, email);

            if (!result.Succeeded)
                throw new ArgumentException("Failed to update");

            return await GenerateJwtToken(user);
        }

        public async  Task<string> UpdatePassword(string userId, string oldPassword, string newPassword)
        {
            var user = await GetUserById(userId);

            if (user == null)
            {
                throw new AuthenticationException();
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            if (!result.Succeeded)
                throw new ArgumentException("Invalid Password");

            return await GenerateJwtToken(user);
        }

        public async Task<string> Upgrade(string userId)
        {
            var user = await GetUserById(userId);

            if (user == null)
            {
                throw new AuthenticationException();
            }

            var result = await _userManager.AddToRoleAsync(user, UserRole.Seller);

            if (!result.Succeeded)
            {
                throw new ArgumentException("Failed to Upgrade");
            }

            return await GenerateJwtToken(user);
        }

        public async Task<int> AddBillingAddress(string userId, BillingAddressDto billingAddressDto)
        {
            var user = await GetUserById(userId);

            if (user == null)
            {
                throw new AuthenticationException();
            }

            var billingAddress = new BillingAddress
            {
                Address = billingAddressDto.Address,
                City = billingAddressDto.City,
                Country = billingAddressDto.Country,
                PostCode = billingAddressDto.PostCode,
                UserId = user.Id
            };

            await Do(async () => await _context.BillingAddresses.AddAsync(billingAddress));

            return billingAddress.Id;
        }

        public IEnumerable<AddressDto> GetBillingAddresses(string userId)
        {
            return _context.BillingAddresses
                .Where(b => b.UserId.Equals(userId)).AsEnumerable()
                .Select(AddressDto.Create);
        }

        public async Task<bool> IsBillingAddress(string userId, int billingAddressId)
        {
            var billingAddress = await _context.BillingAddresses.FindAsync(billingAddressId);

            return billingAddress != null;
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = (await _userManager.GetRolesAsync(user)).Select(r => new Claim(nameof(ClaimTypes.Role), r)).ToList();

            claims.Add(new Claim(nameof(ClaimTypes.NameIdentifier).ToCamelCase(), user.Id));
            claims.Add(new Claim(nameof(ClaimTypes.Name).ToCamelCase(), user.UserName));
            claims.Add(new Claim(nameof(ClaimTypes.Email).ToCamelCase(), user.Email));

            var tokenHandler = new JwtSecurityTokenHandler();

            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));
            var credentials =
                new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = credentials
            };

            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private async Task<ApplicationUser> SignInByEmail(string email, string password)
        {
            var user = await GetUserByEmail(email);

            if (user == null)
            {
                return null;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);

            return result.Succeeded ? user : null;
        }
    }
}
