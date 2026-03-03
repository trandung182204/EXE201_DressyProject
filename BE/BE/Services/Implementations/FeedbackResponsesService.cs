using BE.Models;
using BE.Repositories.Interfaces;
using BE.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Services.Implementations
{
    public class FeedbackResponsesService : IFeedbackResponsesService
    {
        private readonly IFeedbackResponsesRepository _repo;
        public FeedbackResponsesService(IFeedbackResponsesRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<FeedbackResponses>> GetAllAsync() => _repo.GetAllAsync();
        public Task<FeedbackResponses?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<FeedbackResponses> AddAsync(FeedbackResponses feedback) => _repo.AddAsync(feedback);
        public Task<FeedbackResponses?> UpdateAsync(int id, FeedbackResponses feedback) => _repo.UpdateAsync(id, feedback);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
