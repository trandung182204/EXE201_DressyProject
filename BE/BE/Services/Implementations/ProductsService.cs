using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using BE.Data;
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
    }
}
