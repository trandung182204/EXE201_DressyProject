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
                    CategoryId = p.CategoryId,
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
        public async Task<bool> DeleteByProviderAsync(long providerId, long productId)
        {
            // Lấy product + check thuộc provider
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == productId && p.ProviderId == providerId);

            if (product == null) return false;

            // Nếu DB bạn không cascade delete thì remove tay:
            if (product.ProductImages != null && product.ProductImages.Count > 0)
                _context.ProductImages.RemoveRange(product.ProductImages);

            if (product.ProductVariants != null && product.ProductVariants.Count > 0)
                _context.ProductVariants.RemoveRange(product.ProductVariants);

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<ProductDetailDto?> UpdateForProviderAsync(long providerId, long productId, UpdateProviderProductDto dto)
        {
            // Validate cơ bản
            if (dto.CategoryId <= 0) throw new Exception("CategoryId is required");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new Exception("Name is required");
            if (dto.Variants == null || dto.Variants.Count == 0) throw new Exception("At least 1 variant is required");

            // Check category tồn tại
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) throw new Exception("Category not found");

            await using var tx = await _context.Database.BeginTransactionAsync();

            // Load product + navigation
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == productId && p.ProviderId == providerId);

            if (product == null) return null;

            // ===== Update fields product =====
            product.CategoryId = dto.CategoryId;
            product.Name = dto.Name!.Trim();
            product.ProductType = dto.ProductType;
            product.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                var st = dto.Status.Trim().ToUpper();
                if (st != "AVAILABLE" && st != "UNAVAILABLE")
                    throw new Exception("Status must be AVAILABLE or UNAVAILABLE");
                product.Status = st;
            }

            // ===== Replace Images =====
            // Xóa hết ảnh cũ, add ảnh mới
            if (product.ProductImages != null && product.ProductImages.Count > 0)
                _context.ProductImages.RemoveRange(product.ProductImages);

            var newImages = (dto.ImageUrls ?? new List<string?>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => new ProductImages
                {
                    ProductId = product.Id,
                    ImageUrl = x!.Trim()
                })
                .ToList();

            if (newImages.Count > 0)
                await _context.ProductImages.AddRangeAsync(newImages);

            // ===== Upsert Variants =====
            var existingVariants = product.ProductVariants?.ToDictionary(v => v.Id, v => v)
                                   ?? new Dictionary<long, ProductVariants>();

            var incomingIds = new HashSet<long>();

            foreach (var v in dto.Variants)
            {
                var incomingId = (v.Id ?? 0);

                // Update existing
                if (incomingId > 0 && existingVariants.TryGetValue(incomingId, out var ev))
                {
                    incomingIds.Add(incomingId);

                    ev.SizeLabel = v.SizeLabel;
                    ev.ColorName = v.ColorName;
                    ev.ColorCode = v.ColorCode;
                    ev.Quantity = v.Quantity ?? ev.Quantity;
                    ev.PricePerDay = v.PricePerDay ?? ev.PricePerDay;
                    ev.DepositAmount = v.DepositAmount ?? ev.DepositAmount;
                    ev.Status = v.Status ?? ev.Status;
                }
                else
                {
                    // Add new
                    var nv = new ProductVariants
                    {
                        ProductId = product.Id,
                        SizeLabel = v.SizeLabel,
                        ColorName = v.ColorName,
                        ColorCode = v.ColorCode,
                        Quantity = v.Quantity ?? 0,
                        PricePerDay = v.PricePerDay ?? 0,
                        DepositAmount = v.DepositAmount ?? 0,
                        Status = v.Status ?? true
                    };
                    await _context.ProductVariants.AddAsync(nv);
                }
            }

            // Delete variants bị remove khỏi payload
            if (product.ProductVariants != null && product.ProductVariants.Count > 0)
            {
                var toDelete = product.ProductVariants
                    .Where(ev => !incomingIds.Contains(ev.Id)) // chỉ delete những thằng có id cũ mà FE không gửi
                    .ToList();

                // Chỉ nên delete những variant thực sự “existing” (id > 0)
                if (toDelete.Count > 0)
                    _context.ProductVariants.RemoveRange(toDelete);
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            // Trả về detail mới nhất
            return await GetProductDetailByProviderAsync(providerId, productId);
        }

    }
}
