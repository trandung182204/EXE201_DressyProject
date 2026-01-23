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

        // ================== BRANCH PRODUCT ==================

        public async Task<IEnumerable<Products>> GetByBranchAsync(long branchId)
        {
            return await _context.Products
                .Where(p =>
                    p.ProviderBranchId == branchId
                )
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }


        public async Task<Products?> GetByBranchAndIdAsync(long branchId, long productId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == productId &&
                    p.ProviderBranchId == branchId);
        }

        public async Task<Products> AddToBranchAsync(
    long branchId,
    CreateProductDto dto)
        {
            // 1️⃣ Lấy provider từ branch
            var branch = await _context.ProviderBranches
                .FirstOrDefaultAsync(b => b.Id == branchId);

            if (branch == null)
                throw new Exception("Branch not found");

            // 2️⃣ Tạo product
            var product = new Products
            {
                ProviderId = branch.ProviderId,
                ProviderBranchId = branchId,
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                ProductType = dto.ProductType,
                Description = dto.Description,
                Status = "AVAILABLE",
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // lấy product.Id

            // 3️⃣ Thêm images
            if (dto.ImageUrls.Any())
            {
                var images = dto.ImageUrls.Select(url => new ProductImages
                {
                    ProductId = product.Id,
                    ImageUrl = url
                });

                _context.ProductImages.AddRange(images);
            }

            // 4️⃣ Thêm variants
            if (dto.Variants.Any())
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
                    Status = true
                });

                _context.ProductVariants.AddRange(variants);
            }

            await _context.SaveChangesAsync();

            return product;
        }


        public async Task<Products?> UpdateInBranchAsync(
    long branchId,
    long productId,
    UpdateProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p =>
                    p.Id == productId &&
                    p.ProviderBranchId == branchId);

            if (product == null)
                return null;

            // ❌ Không cho sửa nếu đã từng booking
            bool hasBooking = await _context.BookingItems
                .AnyAsync(b => b.ProductId == productId);

            if (hasBooking)
                throw new Exception("Sản phẩm đã được đặt, không thể chỉnh sửa");

            // 1️⃣ Update PRODUCTS
            product.CategoryId = dto.CategoryId;
            product.Name = dto.Name;
            product.ProductType = dto.ProductType;
            product.Description = dto.Description;
            product.Status = dto.Status;

            // 2️⃣ Update IMAGES (replace)
            if (dto.ImageUrls != null)
            {
                _context.ProductImages.RemoveRange(product.ProductImages);

                product.ProductImages = dto.ImageUrls
                    .Select(url => new ProductImages
                    {
                        ImageUrl = url,
                        ProductId = product.Id
                    })
                    .ToList();
            }

            // 3️⃣ Update VARIANTS
            if (dto.Variants != null)
            {
                // Xoá variant không còn trong request
                var incomingIds = dto.Variants
                    .Where(v => v.Id.HasValue)
                    .Select(v => v.Id!.Value)
                    .ToList();

                var removedVariants = product.ProductVariants
                    .Where(v => !incomingIds.Contains(v.Id))
                    .ToList();

                _context.ProductVariants.RemoveRange(removedVariants);

                foreach (var v in dto.Variants)
                {
                    if (v.Id == null)
                    {
                        // ➕ Add mới
                        product.ProductVariants.Add(new ProductVariants
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
                    }
                    else
                    {
                        // ✏ Update
                        var existing = product.ProductVariants
                            .FirstOrDefault(x => x.Id == v.Id);

                        if (existing == null) continue;

                        existing.SizeLabel = v.SizeLabel;
                        existing.ColorName = v.ColorName;
                        existing.ColorCode = v.ColorCode;
                        existing.Quantity = v.Quantity;
                        existing.PricePerDay = v.PricePerDay;
                        existing.DepositAmount = v.DepositAmount;
                        existing.Status = v.Status;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return product;
        }



        public async Task<bool> DeleteInBranchAsync(long branchId, long productId)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p =>
                    p.Id == productId &&
                    p.ProviderBranchId == branchId
                );

            if (product == null)
                return false;

            // ❌ Không cho xoá nếu đã từng được booking
            bool hasBooking = await _context.BookingItems
                .AnyAsync(b => b.ProductId == productId);

            if (hasBooking)
                throw new Exception("Sản phẩm đã được đặt, không thể xoá");

            // 1️⃣ Xoá variants
            _context.ProductVariants.RemoveRange(product.ProductVariants);

            // 2️⃣ Xoá images
            _context.ProductImages.RemoveRange(product.ProductImages);

            // 3️⃣ Xoá product
            _context.Products.Remove(product);

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

    }
}
