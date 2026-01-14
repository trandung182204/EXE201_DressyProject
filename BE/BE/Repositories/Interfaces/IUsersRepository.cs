using BE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task<IEnumerable<Users>> GetAllAsync();
        Task<Users?> GetByIdAsync(long id);
        Task<Users> AddAsync(Users user);
        Task<Users?> UpdateAsync(long id, Users user);
        Task<bool> DeleteAsync(long id);
    }
}
