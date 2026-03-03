using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.DTOs;
using BE.DTOs.Cart;

namespace BE.Services.Interfaces
{
    public interface ICartsService
    {
        Task<IEnumerable<Carts>> GetAllAsync();
        Task<Carts?> GetByIdAsync(int id);
        Task<Carts> AddAsync(Carts model);
        Task<Carts?> UpdateAsync(int id, Carts model);
        Task<bool> DeleteAsync(int id);

        // Per-user cart operations
        Task<CartDetailDto> GetCartDetailAsync(long customerId);
        Task<CartItemDetailDto> AddToCartAsync(long customerId, AddToCartDto dto);
        Task<bool> RemoveCartItemAsync(long customerId, long cartItemId);
        Task ClearCartAsync(long customerId);
    }
}
