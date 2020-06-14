using System.Threading.Tasks;
using Markt.Helpers;
using Markt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Markt.Controllers
{
    [Authorize]
    public class CartController : ApiHelper
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCart()
        {
            return await Do(async () => await _cartService.GetCart(User.Identity.GetUserId()));
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClearCart()
        {
            return await Do(async () => await _cartService.ClearCart(User.Identity.GetUserId()));
        }

        [HttpPut]
        [Route("Checkout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Checkout(int billingAddressId)
        {
            return await Do(async () => await _cartService.Checkout(User.Identity.GetUserId(), billingAddressId));
        }

        [HttpPost]
        [Route("Purchase")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPurchase(int productId, int quantity)
        {
            return await Create(nameof(GetCart), async () => await _cartService.AddToCart(User.Identity.GetUserId(), productId, quantity));
        }

        [HttpPut]
        [Route("Purchase")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePurchase(int purchaseId, int quantity)
        {
            return await Do(async () => await _cartService.UpdatePurchase(User.Identity.GetUserId(), purchaseId, quantity));
        }

        [HttpDelete]
        [Route("Purchase")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePurchase(int purchaseId)
        {
            return await Do(async () => await _cartService.DeletePurchase(User.Identity.GetUserId(), purchaseId));
        }
    }
}
