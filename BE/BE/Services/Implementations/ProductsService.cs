using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using BE.Data;
using BE.DTOs;
using BE.Services.Interfaces;

namespace BE.Services.Implementations
{
    public class ProductsService : IProductsService
    {
        private readonly ApplicationDbContext _context;
        public ProductsService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Products>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }
        public async Task<Products?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }
        public async Task<Products> AddAsync(Products model)
        {
            _context.Products.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public async Task<Products?> UpdateAsync(int id, Products model)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null) return null;
            _context.Entry(item).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return item;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null) return false;
            _context.Products.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(long id, string status)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            status = status.Trim().ToUpper();

            if (status != "AVAILABLE" && status != "UNAVAILABLE")
                return false;

            product.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProductListItemDto>> GetProductsByProviderAsync(long providerId)
        {
            return await _context.Products
                .Where(p => p.ProviderId == providerId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductListItemDto
                {
                    Id = p.Id,
                    Name = p.Name,

                    CategoryName = p.Category != null
                        ? p.Category.Name
                        : null,

                    ThumbnailUrl = p.ProductImages
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

                    MinPricePerDay = p.ProductVariants
                        .Where(v => v.Status == true)
                        .Select(v => (decimal?)v.PricePerDay)
                        .Min(),

                    Status = p.Status
                })
                .ToListAsync();
        }
        public async Task<Products> AddForProviderAsync(long providerId, CreateProviderProductDto dto)
        {
            // Validate nhẹ
            if (dto.CategoryId <= 0) throw new Exception("CategoryId is required");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new Exception("Name is required");
            if (dto.Variants == null || dto.Variants.Count == 0) throw new Exception("At least 1 variant is required");

            // Optional: check category tồn tại
            bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) throw new Exception("Category not found");

            // Tạo product (KHÔNG branch)
            var product = new Products
            {
                ProviderId = providerId,
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                ProductType = dto.ProductType,
                Description = dto.Description,
                Status = "AVAILABLE",
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // lấy product.Id

            // Images
            if (dto.ImageUrls != null && dto.ImageUrls.Count > 0)
            {
                var images = dto.ImageUrls
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(url => new ProductImages
                    {
                        ProductId = product.Id,
                        ImageUrl = url.Trim()
                    });

                _context.ProductImages.AddRange(images);
            }

            // Variants
            if (dto.Variants != null && dto.Variants.Count > 0)
            {
                var variants = dto.Variants.Select(v => new ProductVariants
                {
                    ProductId = product.Id,
                    SizeLabel = v.SizeLabel,
                    ColorName = v.ColorName,
                    ColorCode = v.ColorCode,
                    Quantity = v.Quantity,
                    PricePerDay = v.PricePerDay,
                    DepositAmount = v.DepositAmount,
                    Status = v.Status
                });

                _context.ProductVariants.AddRange(variants);
            }

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<ProductDetailDto?> GetProductDetailByProviderAsync(long providerId, long productId)
        {
            return await _context.Products
                .Where(p => p.ProviderId == providerId && p.Id == productId)
                .Select(p => new ProductDetailDto
                {
                    Id = p.Id,
                    ProviderId = p.ProviderId,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,

                    Name = p.Name,
                    ProductType = p.ProductType,
                    Description = p.Description,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,

                    ImageUrls = p.ProductImages
                        .OrderBy(i => i.Id)
                        .Select(i => i.ImageUrl)
                        .ToList(),

                    Variants = p.ProductVariants
                        .OrderBy(v => v.Id)
                        .Select(v => new ProductVariantDetailDto
                        {
                            Id = v.Id,
                            SizeLabel = v.SizeLabel,
                            ColorName = v.ColorName,
                            ColorCode = v.ColorCode,
                            Quantity = v.Quantity,
                            PricePerDay = v.PricePerDay,
                            DepositAmount = v.DepositAmount,
                            Status = v.Status
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}
