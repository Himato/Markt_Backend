using System;
using System.Threading.Tasks;
using Markt.Dtos;
using Markt.Helpers;
using Markt.Models;
using Markt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Markt.Controllers
{
    public class AccountController : ApiHelper
    {
        private readonly IUserService _userService;
        private readonly ICartService _cartService;

        public AccountController(IUserService userService, ICartService cartService)
        {
            _userService = userService;
            _cartService = cartService;
        }

        [HttpGet]
        [Route("Users")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = nameof(UserRole.Admin))]
        public async Task<IActionResult> GetUsers()
        {
            return await Do(async () => await _userService.GetUsers());
        }

        [HttpPost]
        [Route("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            return await Do(async () =>
            {
                var result = await _userService.Authenticate(loginDto.Email, loginDto.Password);

                if (result == null)
                {
                    throw new ArgumentException("Invalid Login Attempt");
                }

                return result;
            });
        }

        [HttpPost]
        [Route("Register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            return await Create(nameof(Login), async () =>
            {
                var result = await _userService.Create(registerDto);

                if (result == null)
                {
                    throw new ArgumentException("Couldn't create your account");
                }

                return result;
            });
        }
        
        [HttpPost]
        [Route("ForgetPassword")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDto forgetPasswordDto)
        {
            return await Do(async () =>
            {
                //await _userService.ForgetPassword(forgetPasswordDto.Email);
                throw new ArgumentException("This feature is not supported due to the insecure domain");
            });
        }
        
        [HttpPost]
        [Route("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            return await Do(async () =>
            {
                var result = await _userService.ResetPassword(resetPasswordDto);

                if (result == null)
                {
                    throw new ArgumentException("Invalid Attempt");
                }

                return result;
            });
        }

        [HttpPut]
        [Route("UpdateEmail")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateEmail(UpdateEmailDto updateEmailDto)
        {
            return await Do(async () => await _userService.UpdateEmail(User.Identity.GetUserId(), updateEmailDto.Email));
        }

        [HttpPut]
        [Route("UpdatePassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordDto updatePasswordDto)
        {
            return await Do(async () => await _userService.UpdatePassword(User.Identity.GetUserId(), updatePasswordDto.CurrentPassword, updatePasswordDto.NewPassword));
        }

        [HttpPut]
        [Route("Upgrade")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Upgrade()
        {
            return await Do(async () => await _userService.Upgrade(User.Identity.GetUserId()));
        }

        [HttpPost]
        [Route("Address")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddBillingAddress([FromBody] BillingAddressDto billingAddressDto)
        {
            return await Create(nameof(CartController.Checkout),
                async () => await _userService.AddBillingAddress(User.Identity.GetUserId(), billingAddressDto));
        }

        [HttpGet]
        [Route("MyOrders")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyOrders()
        {
            return await Do(async () => await _cartService.GetOrders(User.Identity.GetUserId()));
        }

        [HttpGet]
        [Route("MyAddresses")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMyBillingAddresses()
        {
            return Do(() => _userService.GetBillingAddresses(User.Identity.GetUserId()));
        }
    }
}
