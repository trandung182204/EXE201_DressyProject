using BE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Services.Interfaces
{
    public interface IUsersService
    {
        Task<IEnumerable<Users>> GetAllAsync();
        Task<Users?> GetByIdAsync(long id);
        Task<Users> AddAsync(Users user);
        Task<Users?> UpdateAsync(long id, Users user);
        Task<bool> DeleteAsync(long id);
    }
}
