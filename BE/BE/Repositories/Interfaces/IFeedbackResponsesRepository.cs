using BE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Repositories.Interfaces
{
    public interface IFeedbackResponsesRepository
    {
        Task<IEnumerable<FeedbackResponses>> GetAllAsync();
        Task<FeedbackResponses?> GetByIdAsync(int id);
        Task<FeedbackResponses> AddAsync(FeedbackResponses feedback);
        Task<FeedbackResponses?> UpdateAsync(int id, FeedbackResponses feedback);
        Task<bool> DeleteAsync(int id);
    }
}
