using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.Services.Interfaces;
using BE.Repositories.Interfaces;
using BE.DTOs;
using BE.DTOs.Cart;
using BE.Data;
using Microsoft.EntityFrameworkCore;

namespace BE.Services.Implementations
{
    public class CartsService : ICartsService
    {
        private readonly ICartsRepository _repo;
        private readonly ICartItemsRepository _cartItemsRepo;
        private readonly ApplicationDbContext _db;

        public CartsService(ICartsRepository repo, ICartItemsRepository cartItemsRepo, ApplicationDbContext db)
        {
            _repo = repo;
            _cartItemsRepo = cartItemsRepo;
            _db = db;
        }

        // --- Basic CRUD (keep existing) ---
        public async Task<IEnumerable<Carts>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<Carts?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);
        public async Task<Carts> AddAsync(Carts model) => await _repo.AddAsync(model);
        public async Task<Carts?> UpdateAsync(int id, Carts model) => await _repo.UpdateAsync(id, model);
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

        // --- Per-user cart operations ---

        private async Task<Carts> GetOrCreateCartAsync(long customerId)
        {
            var cart = await _repo.GetByCustomerIdAsync(customerId);
            if (cart == null)
            {
                cart = new Carts
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow
                };
                cart = await _repo.AddAsync(cart);
            }
            return cart;
        }

        public async Task<CartDetailDto> GetCartDetailAsync(long customerId)
        {
            var cart = await _repo.GetByCustomerIdAsync(customerId);
            if (cart == null)
            {
                return new CartDetailDto { CartId = 0, Items = new List<CartItemDetailDto>() };
            }

            // Query cart items with product info
            var items = await _db.CartItems
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv!.Product)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            var result = new CartDetailDto
            {
                CartId = cart.Id,
                Items = items.Select(ci =>
                {
                    var pv = ci.ProductVariant;
                    var product = pv?.Product;
                    var days = 0;
                    if (ci.StartDate.HasValue && ci.EndDate.HasValue)
                    {
                        days = ci.EndDate.Value.DayNumber - ci.StartDate.Value.DayNumber + 1;
                    }
                    var pricePerDay = pv?.PricePerDay ?? 0;
                    var qty = ci.Quantity ?? 1;

                    // Get first image for the product
                    string? imageUrl = null;
                    if (product != null)
                    {
                        var imgFileId = _db.Set<BE.Models.ProductImages>()
                            .Where(pi => pi.ProductId == product.Id)
                            .Select(pi => pi.ImageFileId)
                            .FirstOrDefault();
                        if (imgFileId != null && imgFileId > 0)
                            imageUrl = $"/api/media-files/{imgFileId}";
                    }

                    return new CartItemDetailDto
                    {
                        CartItemId = ci.Id,
                        ProductId = product?.Id ?? 0,
                        ProductName = product?.Name ?? "Unknown",
                        ImageUrl = imageUrl,
                        ProductVariantId = pv?.Id ?? 0,
                        SizeLabel = pv?.SizeLabel,
                        ColorName = pv?.ColorName,
                        Quantity = qty,
                        PricePerDay = pricePerDay,
                        DepositAmount = pv?.DepositAmount ?? 0,
                        StartDate = ci.StartDate ?? DateOnly.MinValue,
                        EndDate = ci.EndDate ?? DateOnly.MinValue,
                        RentalDays = days,
                        TotalPrice = pricePerDay * days * qty
                    };
                }).ToList()
            };

            return result;
        }

        public async Task<CartItemDetailDto> AddToCartAsync(long customerId, AddToCartDto dto)
        {
            var cart = await GetOrCreateCartAsync(customerId);

            // Check if same variant already in cart
            var existing = await _db.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cart.Id &&
                    ci.ProductVariantId == dto.ProductVariantId &&
                    ci.StartDate == dto.StartDate &&
                    ci.EndDate == dto.EndDate);

            if (existing != null)
            {
                existing.Quantity = (existing.Quantity ?? 0) + dto.Quantity;
                await _db.SaveChangesAsync();
            }
            else
            {
                existing = new CartItems
                {
                    CartId = cart.Id,
                    ProductVariantId = dto.ProductVariantId,
                    Quantity = dto.Quantity,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate
                };
                _db.CartItems.Add(existing);
                await _db.SaveChangesAsync();
            }

            // Return detail
            var pv = await _db.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId);

            var days = 0;
            if (dto.StartDate.HasValue && dto.EndDate.HasValue)
                days = dto.EndDate.Value.DayNumber - dto.StartDate.Value.DayNumber + 1;

            var pricePerDay = pv?.PricePerDay ?? 0;
            var qty = existing.Quantity ?? 1;

            return new CartItemDetailDto
            {
                CartItemId = existing.Id,
                ProductId = pv?.ProductId ?? 0,
                ProductName = pv?.Product?.Name ?? "Unknown",
                ProductVariantId = pv?.Id ?? 0,
                SizeLabel = pv?.SizeLabel,
                ColorName = pv?.ColorName,
                Quantity = qty,
                PricePerDay = pricePerDay,
                DepositAmount = pv?.DepositAmount ?? 0,
                StartDate = dto.StartDate ?? DateOnly.MinValue,
                EndDate = dto.EndDate ?? DateOnly.MinValue,
                RentalDays = days,
                TotalPrice = pricePerDay * days * qty
            };
        }

        public async Task<bool> RemoveCartItemAsync(long customerId, long cartItemId)
        {
            var cart = await _repo.GetByCustomerIdAsync(customerId);
            if (cart == null) return false;

            var item = await _db.CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);
            if (item == null) return false;

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(long customerId)
        {
            var cart = await _repo.GetByCustomerIdAsync(customerId);
            if (cart == null) return;
            await _cartItemsRepo.DeleteByCartIdAsync(cart.Id);
        }
    }
}
