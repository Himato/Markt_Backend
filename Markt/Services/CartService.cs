using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Markt.Dtos;
using Markt.Helpers;
using Markt.Models;
using Microsoft.EntityFrameworkCore;

namespace Markt.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCart(string userId);

        Task<int> AddToCart(string userId, int productId, int quantity);

        Task UpdatePurchase(string userId, int purchaseId, int quantity);

        Task DeletePurchase(string userId, int purchaseId);

        Task ClearCart(string userId);

        Task Checkout(string userId, int billingAddressId);

        Task<IEnumerable<OrderDto>> GetOrders(string userId);
    }

    public class CartService : ServiceHelper, ICartService
    {
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ApplicationDbContext _context;

        public CartService(IUserService userService, IProductService productService,
            ApplicationDbContext context) : base(context)
        {
            _userService = userService;
            _productService = productService;
            _context = context;
        }

        public async Task<CartDto> GetCart(string userId)
        {
            return CartDto.Create(await GetPurchases(userId));
        }

        public async Task<int> AddToCart(string userId, int productId, int quantity)
        {
            var user = await _userService.GetUserById(userId);

            if (user == null)
            {
                throw new AuthenticationException();
            }

            var product = await _productService.GetProductForCart(productId);

            if (product == null)
            {
                throw new ArgumentException("Product not found");
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity is not a valid number");
            }

            var old = await _context.Purchases.FirstOrDefaultAsync(p => p.UserId.Equals(userId) && p.ProductId == productId && !p.IsFinished);
            if (old != null)
            {
                old.Quantity += quantity;
                old.TotalPrice = product.Price * old.Quantity;
                await Do(() => _context.Entry(old).State = EntityState.Modified);
                return old.Id;
            }

            var purchase = new Purchase
            {
                ProductId = productId,
                UserId = user.Id,
                Quantity = quantity,
                TotalPrice = quantity * product.Price,
                IsFinished = false
            };

            await Do(async () => await _context.Purchases.AddAsync(purchase));

            return purchase.Id;
        }

        public async Task UpdatePurchase(string userId, int purchaseId, int quantity)
        {
            var purchase = await _context.Purchases.FindAsync(purchaseId);

            if (purchase == null)
            {
                throw new KeyNotFoundException("Purchase not found");
            }

            if (!purchase.UserId.Equals(userId))
            {
                throw new AuthenticationException();
            }

            if (purchase.IsFinished)
            {
                throw new ArgumentException("Can't update this purchase now");
            }

            var product = await _productService.GetProductForCart(purchase.ProductId);

            if (product == null)
            {
                throw new ArgumentException("Product not found");
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity is not a valid number");
            }

            purchase.Quantity = quantity;
            purchase.TotalPrice = quantity * product.Price;

            await Do(() => _context.Entry(purchase).State = EntityState.Modified);
        }

        public async Task DeletePurchase(string userId, int purchaseId)
        {
            var purchase = await _context.Purchases.FindAsync(purchaseId);

            if (purchase == null)
            {
                throw new KeyNotFoundException("Purchase not found");
            }

            if (!purchase.UserId.Equals(userId))
            {
                throw new AuthenticationException();
            }

            if (purchase.IsFinished)
            {
                throw new ArgumentException("Can't delete this purchase now");
            }

            await Do(() => _context.Purchases.Remove(purchase));
        }

        public async Task ClearCart(string userId)
        {
            var purchases = await GetPurchases(userId);

            if (!purchases.Any())
            {
                return;
            }

            await Do(() => _context.Purchases.RemoveRange(purchases));
        }

        public async Task Checkout(string userId, int billingAddressId)
        {
            if (!await _userService.IsBillingAddress(userId, billingAddressId))
            {
                throw new KeyNotFoundException("Billing Address not found");
            }

            await FinishCheckout(userId, billingAddressId);
        }

        private async Task FinishCheckout(string userId, int billingAddressId)
        {
            var purchases = await GetPurchases(userId);

            if (!purchases.Any())
            {
                throw new ArgumentException("Invalid number of purchases");
            }

            if (purchases.Any(p => !p.Product.IsInStock))
            {
                throw new ArgumentException("There are some products out of stock");
            }

            var orderId = await AddOrder(userId,
                purchases.Select(p => p.TotalPrice).Aggregate(0d, (current, next) => current + next), billingAddressId);

            await Do(() =>
            {
                foreach (var purchase in purchases)
                {
                    purchase.IsFinished = true;
                    purchase.OrderId = orderId;
                    _context.Entry(purchase).State = EntityState.Modified;
                }
            });
        }

        public async Task<IEnumerable<OrderDto>> GetOrders(string userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId.Equals(userId))
                .Include(o => o.BillingAddress).ToListAsync();

            var output = new List<OrderDto>();

            foreach (var order in orders)
            {
                output.Add(OrderDto.Create(order, await GetPurchases(userId, order.Id)));
            }

            return output;
        }

        private async Task<int> AddOrder(string userId, double price, int billingAddressId)
        {
            var order = new Order
            {
                DateTime = DateTime.Now,
                TotalPrice = price,
                BillingAddressId = billingAddressId,
                UserId = userId
            };

            await Do(async () => await _context.Orders.AddAsync(order));

            return order.Id;
        }

        private async Task<List<Purchase>> GetPurchases(string userId)
        {
            return await _context.Purchases
                .Where(p => p.UserId.Equals(userId) && !p.IsFinished)
                .Include(p => p.Product)
                .Include(p => p.Product.Seller)
                .Include(p => p.Product.Images).ToListAsync();
        }

        private async Task<List<Purchase>> GetPurchases(string userId, int orderId)
        {
            return await _context.Purchases
                .Where(p => p.OrderId == orderId && p.UserId.Equals(userId))
                .Include(p => p.Product).ToListAsync();
        }
    }
}
