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
        public async Task<Products?> GetByIdAsync(long id)
        {
            return await _context.Products.FindAsync(id);
        }
        public async Task<Products> AddAsync(Products model)
        {
            _context.Products.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public async Task<Products?> UpdateAsync(long id, Products model)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null) return null;
            _context.Entry(item).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return item;
        }
        public async Task<bool> DeleteAsync(long id)
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
                    CategoryName = p.Category != null ? p.Category.Name : null,

                    // CHANGED
                    ThumbnailFileId = p.ProductImages
                        .Select(i => i.ImageFileId)
                        .FirstOrDefault(),

                    // OPTIONAL: url runtime
                    ThumbnailUrl = p.ProductImages
                        .Select(i => i.ImageFileId)
                        .Select(fid => fid != null ? ("/api/media-files/" + fid) : null)
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
            if (dto.CategoryId <= 0) throw new Exception("CategoryId is required");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new Exception("Name is required");
            if (dto.Variants == null || dto.Variants.Count == 0) throw new Exception("At least 1 variant is required");

            bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) throw new Exception("Category not found");

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
            await _context.SaveChangesAsync();

            // CHANGED: Images by fileId
            if (dto.ImageFileIds != null && dto.ImageFileIds.Count > 0)
            {
                // optional validate file ids exist
                var distinctIds = dto.ImageFileIds.Distinct().ToList();
                var existingIds = await _context.MediaFiles
                    .Where(f => distinctIds.Contains(f.Id))
                    .Select(f => f.Id)
                    .ToListAsync();

                var missing = distinctIds.Except(existingIds).ToList();
                if (missing.Count > 0)
                    throw new Exception("Some image fileIds not found: " + string.Join(",", missing));

                var images = distinctIds.Select(fid => new ProductImages
                {
                    ProductId = product.Id,
                    ImageFileId = fid
                });

                _context.ProductImages.AddRange(images);
            }

            // Variants giữ nguyên
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

                    // CHANGED
                    ImageFileIds = p.ProductImages
                        .OrderBy(i => i.Id)
                        .Select(i => i.ImageFileId)
                        .ToList(),

                    // OPTIONAL: build url runtime
                    ImageUrls = p.ProductImages
                        .OrderBy(i => i.Id)
                        .Select(i => i.ImageFileId != null ? ("/api/media-files/" + i.ImageFileId) : null)
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
            if (product.ProductImages != null && product.ProductImages.Count > 0)
                _context.ProductImages.RemoveRange(product.ProductImages);

            var incomingFileIds = (dto.ImageFileIds ?? new List<long>())
                .Distinct()
                .ToList();

            if (incomingFileIds.Count > 0)
            {
                var existingIds = await _context.MediaFiles
                    .Where(f => incomingFileIds.Contains(f.Id))
                    .Select(f => f.Id)
                    .ToListAsync();

                var missing = incomingFileIds.Except(existingIds).ToList();
                if (missing.Count > 0)
                    throw new Exception("Some image fileIds not found: " + string.Join(",", missing));

                var newImages = incomingFileIds.Select(fid => new ProductImages
                {
                    ProductId = product.Id,
                    ImageFileId = fid
                }).ToList();

                await _context.ProductImages.AddRangeAsync(newImages);
            }

            // ===== Upsert Variants =====
            var existingVariants = product.ProductVariants?.ToDictionary(v => v.Id, v => v)
                                   ?? new Dictionary<long, ProductVariants>();

            var incomingExistingIds = new HashSet<long>(); // chỉ chứa id > 0 (variant đã tồn tại)

            foreach (var v in dto.Variants)
            {
                var incomingId = (v.Id ?? 0);

                // Update existing
                if (incomingId > 0 && existingVariants.TryGetValue(incomingId, out var ev))
                {
                    incomingExistingIds.Add(incomingId);

                    ev.SizeLabel = (v.SizeLabel ?? ev.SizeLabel)?.Trim();
                    ev.ColorName = (v.ColorName ?? ev.ColorName)?.Trim();
                    ev.ColorCode = v.ColorCode ?? ev.ColorCode;

                    ev.Quantity = v.Quantity ?? ev.Quantity;
                    ev.PricePerDay = v.PricePerDay ?? ev.PricePerDay;
                    ev.DepositAmount = v.DepositAmount ?? ev.DepositAmount;
                    ev.Status = v.Status ?? ev.Status;
                }
                else
                {
                    // Add new (Id null/0)
                    var nv = new ProductVariants
                    {
                        ProductId = product.Id,
                        SizeLabel = (v.SizeLabel ?? "").Trim(),
                        ColorName = (v.ColorName ?? "").Trim(),
                        ColorCode = v.ColorCode,

                        Quantity = v.Quantity ?? 0,
                        PricePerDay = v.PricePerDay ?? 0,
                        DepositAmount = v.DepositAmount ?? 0,
                        Status = v.Status ?? true
                    };

                    await _context.ProductVariants.AddAsync(nv);
                }
            }

            // ✅ FIX BUG QUAN TRỌNG:
            // Chỉ xoá các variant CŨ trong DB (Id > 0) mà không có trong payload.
            if (product.ProductVariants != null && product.ProductVariants.Count > 0)
            {
                var toDelete = product.ProductVariants
                    .Where(ev => ev.Id > 0 && !incomingExistingIds.Contains(ev.Id))
                    .ToList();

                if (toDelete.Count > 0)
                    _context.ProductVariants.RemoveRange(toDelete);
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            // Trả về detail mới nhất
            return await GetProductDetailByProviderAsync(providerId, productId);
        }
        public async Task<IEnumerable<HomeFavoriteProductDto>> GetLatestFavoritesAsync(int limit)
        {
            if (limit <= 0) limit = 5;
            if (limit > 50) limit = 50;

            // 1) Lấy top N product mới nhất (AVAILABLE)
            var latest = await _context.Products
                .Where(p => p.Status == "AVAILABLE")
                .OrderByDescending(p => p.CreatedAt)  // CreatedAt null -> xuống cuối
                .ThenByDescending(p => p.Id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.CreatedAt,

                    // lấy 1 ảnh đầu tiên
                    ImageFileId = p.ProductImages
                        .OrderBy(i => i.Id)
                        .Select(i => i.ImageFileId)
                        .FirstOrDefault(),

                    // lấy min price theo variant active
                    MinPricePerDay = p.ProductVariants
                        .Where(v => v.Status == true)
                        .Select(v => (decimal?)v.PricePerDay)
                        .Min(),
                    DepositAmount = p.ProductVariants
  .Where(v => v.Status == true)
  .Select(v => (decimal?)v.DepositAmount)
  .Min(),
                })
                .Take(limit)
                .ToListAsync();

            // 2) Build URL ảnh runtime (đúng kiểu bạn đang làm)
            var result = latest.Select(x => new HomeFavoriteProductDto
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt,
                MinPricePerDay = x.MinPricePerDay ?? 0,
                DepositAmount = x.DepositAmount ?? 0,

                ImageFileId = x.ImageFileId,
                ImageUrl = x.ImageFileId != null ? ("/api/media-files/" + x.ImageFileId) : null
            });

            return result;
        }

    }
}
