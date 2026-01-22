using BE.Data;
using BE.Models;
using BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE.Repositories.Implementations;

public class ProductsCustomerRepository : IProductsCustomerRepository
{
    private readonly ApplicationDbContext _db;

    public ProductsCustomerRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Products>> GetAllAsync(string? status = null)
    {
        var q = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductVariants)
            .AsQueryable();

        // lọc theo status nếu có
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(p => p.Status == status);

        // sort mới nhất trước (nếu CreatedAt null thì rơi xuống cuối)
        q = q.OrderByDescending(p => p.CreatedAt);

        return await q.ToListAsync();
    }
}
