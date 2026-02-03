// ProductsCustomerRepository.cs
using BE.Data;
using BE.DTOs;
using BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE.Repositories.Implementations;

public class ProductsCustomerRepository : IProductsCustomerRepository
{
    private readonly ApplicationDbContext _db;
    public ProductsCustomerRepository(ApplicationDbContext db) => _db = db;

    public async Task<PagedResult<ProductListCustomerItemDto>> GetListingAsync(ProductListQuery q)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductVariants)
            .AsQueryable();

        // 1) status sản phẩm
        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(p => p.Status == q.Status);

        // 2) filter danh mục
        if (q.CategoryIds != null && q.CategoryIds.Count > 0)
            query = query.Where(p => p.CategoryId != null && q.CategoryIds.Contains(p.CategoryId.Value));

        // 3) filter theo variant (giá/size/màu/còn hàng)
        if (q.OnlyAvailable)
            query = query.Where(p => p.ProductVariants.Any(v => (v.Status ?? false) && (v.Quantity ?? 0) > 0));

        if (q.MinPrice.HasValue)
            query = query.Where(p => p.ProductVariants.Any(v => v.PricePerDay != null && v.PricePerDay >= q.MinPrice));

        if (q.MaxPrice.HasValue)
            query = query.Where(p => p.ProductVariants.Any(v => v.PricePerDay != null && v.PricePerDay <= q.MaxPrice));

        if (q.Sizes != null && q.Sizes.Count > 0)
            query = query.Where(p => p.ProductVariants.Any(v => v.SizeLabel != null && q.Sizes.Contains(v.SizeLabel)));

        if (q.Colors != null && q.Colors.Count > 0)
            query = query.Where(p => p.ProductVariants.Any(v => v.ColorName != null && q.Colors.Contains(v.ColorName)));

        // 4) sort
        var sortBy = (q.SortBy ?? "createdAt").ToLower();
        var asc = (q.SortDir ?? "desc").ToLower() == "asc";

        query = (sortBy) switch
        {
            "name" => asc ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "price" => asc
                ? query.OrderBy(p => p.ProductVariants.Min(v => (decimal?)v.PricePerDay))
                : query.OrderByDescending(p => p.ProductVariants.Min(v => (decimal?)v.PricePerDay)),
            _ => asc ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
        };

        // 5) paging
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize is < 1 ? 9 : (q.PageSize > 60 ? 60 : q.PageSize);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListCustomerItemDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category != null ? p.Category.Name : null,
                ThumbnailUrl = p.ProductImages.Select(i => i.ImageUrl).FirstOrDefault(),
                MinPricePerDay = p.ProductVariants.Min(v => (decimal?)v.PricePerDay),
                CreatedAt = p.CreatedAt,
                Sizes = p.ProductVariants.Where(v => v.SizeLabel != null).Select(v => v.SizeLabel!).Distinct().ToList(),
                Colors = p.ProductVariants.Where(v => v.ColorName != null).Select(v => v.ColorName!).Distinct().ToList()
            })
            .ToListAsync();

        return new PagedResult<ProductListCustomerItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = items
        };
    }

    public async Task<ProductDetailDto?> GetProductDetailAsync(long id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductVariants)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        return new ProductDetailDto
        {
            Id = product.Id,
            ProviderId = product.ProviderId,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            Name = product.Name,
            ProductType = product.ProductType,
            Description = product.Description,
            Status = product.Status,
            CreatedAt = product.CreatedAt,
            ImageUrls = product.ProductImages.Select(i => i.ImageUrl).ToList(),
            Variants = product.ProductVariants.Select(v => new ProductVariantDetailDto
            {
                Id = v.Id,
                SizeLabel = v.SizeLabel,
                ColorName = v.ColorName,
                ColorCode = v.ColorCode,
                Quantity = v.Quantity,
                PricePerDay = v.PricePerDay,
                DepositAmount = v.DepositAmount,
                Status = v.Status
            }).ToList()
        };
    }
}
