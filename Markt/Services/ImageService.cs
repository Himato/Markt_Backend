using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Markt.Helpers;
using Markt.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Markt.Services
{
    public interface IImageService
    {
        Task<IEnumerable<Image>> GetProductImages(int productId);

        /// <returns>Image Uri</returns>
        Task<string> UploadImage(int productId, IFormFile imageFile);

        Task DeleteImage(int imageId, int productId);
    }

    public class ImageService : ServiceHelper, IImageService
    {
        private readonly ApplicationDbContext _context;

        public ImageService(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Image>> GetProductImages(int productId)
        {
            return await _context.Images.Where(i => i.ProductId == productId).ToListAsync();
        }

        public async Task<string> UploadImage(int productId, IFormFile imageFile)
        {
            var uri = await SaveImage(imageFile);

            if (uri == null)
            {
                return null;
            }

            var image = new Image
            {
                ProductId = productId,
                Uri = uri
            };

            await Do(async () => await _context.Images.AddAsync(image));

            return image.Uri;
        }

        public async Task DeleteImage(int imageId, int productId)
        {
            var image = await _context.Images.FindAsync(imageId);

            if (image == null || image.ProductId != productId)
            {
                throw new ArgumentException("Couldn't delete the image");
            }

            File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", image.Uri));

            await Do(() => _context.Images.Remove(image));
        }
        
        private static async Task<string> SaveImage(IFormFile image)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                image.CopyTo(ms);
                bytes = ms.ToArray();
            }

            if (!FileHelper.IsImage(bytes))
            {
                return null;
            }

            try
            {
                var name = Guid.NewGuid() + "." + image.FileName.Split('.').Last();

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", name);

                using (var bits = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(bits);
                }

                return name;
            }
            catch
            {
                return null;
            }
        }
    }
}
