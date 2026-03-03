using BE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Repositories.Interfaces
{
    public interface IPaymentsRepository
    {
        Task<IEnumerable<Payments>> GetAllAsync();
        Task<Payments?> GetByIdAsync(long id);
        Task<Payments> AddAsync(Payments payment);
        Task<Payments?> UpdateAsync(long id, Payments payment);
        Task<bool> DeleteAsync(long id);
    }
}
