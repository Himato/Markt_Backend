using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Markt.Helpers;
using Markt.Helpers.Attributes.Filterable;
using Markt.Helpers.Attributes.Searchable;
using Markt.Models;
using Microsoft.AspNetCore.Http;

namespace Markt.Dtos
{
    public class ProductDto
    {
        [Required]
        [MinLength(4, ErrorMessage = "Product name can't be less than 4 characters")]
        [StringLength(255, ErrorMessage = "Product name can't proceed 255 characters")]
        public string Name { get; set; }

        [Required]
        [MinLength(32, ErrorMessage = "Product description can't be less than 32 characters")]
        public string Description { get; set; }

        [Required]
        [MinLength(32, ErrorMessage = "Product specification can't be less than 32 characters")]
        public string Specification { get; set; }

        [Required]
        public string ReturnInfo { get; set; }

        [PositiveValue(ErrorMessage = "Price value must be a positive number")]
        public float Price { get; set; }

        public bool IsInStock { get; set; }

        [PositiveValue(ErrorMessage = "Invalid subcategory id")]
        public int SubcategoryId { get; set; }

        [PositiveValue(ErrorMessage = "Invalid brand id")]
        public int BrandId { get; set; }

        public List<IFormFile> Images { get; set; }
    }

    public class ReviewDto
    {
        public int Id { get; set; }

        public int Rate { get; set; }

        public static ReviewDto Create(Review review)
        {
            if (review == null)
            {
                return null;
            }

            return new ReviewDto
            {
                Id = review.Id,
                Rate = review.Rate
            };
        }
    }

    public class SingleProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string SellerName { get; set; }

        public string SellerUserName { get; set; }

        public string Description { get; set; }
        
        public string Specification { get; set; }
        
        public string ReturnInfo { get; set; }
        
        public float Price { get; set; }

        public bool IsInStock { get; set; }

        public List<string> Images { get; set; }

        public int NumberOfReviews { get; set; }

        public float Review { get; set; }

        public ReviewDto UserReview { get; set; }

        public static SingleProductDto Create(string userId, Product product)
        {
            var numberOfReviews = product.Reviews.Count;

            var totalReviews = (float)product.Reviews.Aggregate(0, (current, next) => current + next.Rate);

            var review = numberOfReviews > 0 ? totalReviews / numberOfReviews : 0;

            return new SingleProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SellerName = product.Seller.GetName(),
                SellerUserName = product.Seller.UserName,
                Description = product.Description,
                Specification = product.Specification,
                ReturnInfo = product.ReturnInfo,
                Price = product.Price,
                IsInStock = product.IsInStock,
                Images = product.Images.Select(c => c.Uri).ToList(),
                NumberOfReviews = numberOfReviews,
                Review = review,
                UserReview = userId != null ? ReviewDto.Create(product.Reviews.FirstOrDefault(r => r.UserId.Equals(userId))) : null
            };
        }
    }

    public class ProductResultDto
    {
        public int Id { get; set; }

        [SearchableString]
        public string Name { get; set; }

        public string SellerName { get; set; }

        public string SellerUsername { get; set; }

        public string Uri { get; set; }

        public float Price { get; set; }

        public bool IsInStock { get; set; }

        public string ImageUri { get; set; }

        [FilterableDecimal]
        public int BrandId { get; set; }

        public static ProductResultDto Create(Product product)
        {
            return new ProductResultDto
            {
                Id = product.Id,
                Name = product.Name,
                SellerName = product.Seller?.GetName(),
                SellerUsername = product.Seller?.UserName,
                Uri = product.Uri,
                Price = product.Price,
                IsInStock = product.IsInStock,
                BrandId = product.BrandId,
                ImageUri = product.Images?.First().Uri
            };
        }
    }

    public class SellerProductsDto
    {
        public string SellerName { get; set; }

        public IEnumerable<ProductResultDto> Products { get; set; }
    }

    public class ReportDto
    {
        public string Uri { get; set; }

        public string Name { get; set; }

        public int NumberOfFinishedPurchases { get; set; }

        public int NumberOfAwaitPurchases { get; set; }

        public int TotalSoldQuantity { get; set; }

        public int TotalAwaitQuantity { get; set; }

        public double TotalAvailableMoney { get; set; }

        public double TotalExpectedMoney { get; set; }

        public static ReportDto Create(Product product, List<Purchase> purchases)
        {
            var finishedPurchases = purchases.Where(p => p.IsFinished).ToList();
            var awaitPurchases = purchases.Where(p => !p.IsFinished).ToList();

            return new ReportDto
            {
                Uri = product.Uri,
                Name = product.Name,
                NumberOfFinishedPurchases = finishedPurchases.Count,
                NumberOfAwaitPurchases = awaitPurchases.Count,
                TotalSoldQuantity = finishedPurchases.Select(p => p.Quantity).Aggregate(0, (current, next) => current + next),
                TotalAwaitQuantity = awaitPurchases.Select(p => p.Quantity).Aggregate(0, (current, next) => current + next),
                TotalAvailableMoney = finishedPurchases.Select(p => p.TotalPrice).Aggregate(0d, (current, next) => current + next),
                TotalExpectedMoney = awaitPurchases.Select(p => p.TotalPrice).Aggregate(0d, (current, next) => current + next)
            };
        }
    }
}
