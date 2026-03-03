using BE.Models;
using BE.Repositories.Interfaces;
using BE.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Services.Implementations
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IPaymentsRepository _repo;
        public PaymentsService(IPaymentsRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Payments>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Payments?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);
        public Task<Payments> AddAsync(Payments payment) => _repo.AddAsync(payment);
        public Task<Payments?> UpdateAsync(long id, Payments payment) => _repo.UpdateAsync(id, payment);
        public Task<bool> DeleteAsync(long id) => _repo.DeleteAsync(id);
    }
}
