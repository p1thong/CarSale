using ASM1.Repository.Models;

namespace ASM1.Repository.Repositories.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<IEnumerable<Feedback>> GetAllFeedbacksAsync();
        Task<Feedback?> GetFeedbackByIdAsync(int feedbackId);
        Task<IEnumerable<Feedback>> GetFeedbacksByCustomerAsync(int customerId);
        Task<IEnumerable<Feedback>> GetFeedbacksByDealerAsync(int dealerId);
        Task<IEnumerable<Feedback>> GetFeedbacksByVehicleModelAsync(int vehicleModelId);
        Task<Feedback> CreateFeedbackAsync(Feedback feedback);
        Task<Feedback> UpdateFeedbackAsync(Feedback feedback);
        Task<bool> DeleteFeedbackAsync(int feedbackId);
        Task<double> GetAverageRatingByVehicleModelAsync(int vehicleModelId);
    }
}