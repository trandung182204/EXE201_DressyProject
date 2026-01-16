using BE.Models;
using BE.Repositories.Interfaces;
using BE.Services.Interfaces;

namespace BE.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _repo;
        public UsersService(IUsersRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Users>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Users?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);
        public Task<Users> AddAsync(Users user) => _repo.AddAsync(user);
        public Task<Users?> UpdateAsync(long id, Users user) => _repo.UpdateAsync(id, user);
        public Task<bool> DeleteAsync(long id) => _repo.DeleteAsync(id);
    }
}
