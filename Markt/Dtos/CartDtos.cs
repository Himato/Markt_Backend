using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Markt.Models;

namespace Markt.Dtos
{
    public class CartDto
    {
        public double TotalPrice { get; set; }

        public List<PurchaseDto> Purchases { get; set; }

        public static CartDto Create(List<Purchase> purchases)
        {
            var list = purchases.Select(PurchaseDto.Create).ToList();

            return new CartDto
            {
                TotalPrice = list.Select(p => p.TotalPrice).Aggregate(0d, (current, next) => current + next),
                Purchases = list
            };
        }
    }

    public class PurchaseDto
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public double TotalPrice { get; set; }

        public ProductResultDto Product { get; set; }

        public static PurchaseDto Create(Purchase purchase)
        {
            return new PurchaseDto
            {
                Id = purchase.Id,
                Quantity = purchase.Quantity,
                TotalPrice = purchase.TotalPrice,
                Product = ProductResultDto.Create(purchase.Product)
            };
        }
    }

    public class BillingAddressDto
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string PostCode { get; set; }

        [Required]
        public string Country { get; set; }
    }

    public class OrderDto
    {
        public DateTime DateTime { get; set; }

        public double TotalPrice { get; set; }

        public AddressDto Address { get; set; }

        public List<PurchaseDto> Purchases { get; set; }

        public static OrderDto Create(Order order, List<Purchase> purchases)
        {
            return new OrderDto
            {
                DateTime = order.DateTime,
                TotalPrice = order.TotalPrice,
                Address = AddressDto.Create(order.BillingAddress),
                Purchases = purchases.Select(PurchaseDto.Create).ToList()
            };
        }
    }
}
