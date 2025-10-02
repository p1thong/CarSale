using ASM1.Repository.Data;
using ASM1.Repository.Models;
using ASM1.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASM1.Repository.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly CarSalesDbContext _context;

        public FeedbackRepository(CarSalesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync()
        {
            return await _context.Feedbacks
                .Include(f => f.Customer)
                .OrderByDescending(f => f.FeedbackId)
                .ToListAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int feedbackId)
        {
            return await _context.Feedbacks
                .Include(f => f.Customer)
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByCustomerAsync(int customerId)
        {
            return await _context.Feedbacks
                .Where(f => f.CustomerId == customerId)
                .OrderByDescending(f => f.FeedbackId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByDealerAsync(int dealerId)
        {
            return await _context.Feedbacks
                .Where(f => f.Customer.DealerId == dealerId)
                .Include(f => f.Customer)
                .OrderByDescending(f => f.FeedbackId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByVehicleModelAsync(int vehicleModelId)
        {
            // Since Feedback doesn't directly relate to VehicleModel, we might need to relate through Orders or TestDrives
            // For now, returning empty list - this would need business logic clarification
            return await _context.Feedbacks
                .Where(f => false) // Placeholder - needs proper relationship
                .Include(f => f.Customer)
                .OrderByDescending(f => f.FeedbackId)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingByVehicleModelAsync(int vehicleModelId)
        {
            // Similar issue - need proper relationship between Feedback and VehicleModel
            var feedbacks = await GetFeedbacksByVehicleModelAsync(vehicleModelId);
            return feedbacks.Where(f => f.Rating.HasValue).Any() 
                ? feedbacks.Where(f => f.Rating.HasValue).Average(f => f.Rating!.Value) 
                : 0.0;
        }

        public async Task<Feedback> CreateFeedbackAsync(Feedback feedback)
        {
            try
            {
                // Try to get the next ID manually if IDENTITY is not working
                var lastFeedback = await _context.Feedbacks
                    .OrderByDescending(f => f.FeedbackId)
                    .FirstOrDefaultAsync();
                
                if (lastFeedback != null)
                {
                    feedback.FeedbackId = lastFeedback.FeedbackId + 1;
                }
                else
                {
                    feedback.FeedbackId = 1;
                }
                
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();
                return feedback;
            }
            catch (Exception)
            {
                // If manual ID setting fails, try without setting ID
                var feedbackWithoutId = new Feedback
                {
                    CustomerId = feedback.CustomerId,
                    Content = feedback.Content,
                    Rating = feedback.Rating,
                    FeedbackDate = feedback.FeedbackDate,
                    CreatedAt = feedback.CreatedAt
                };
                
                _context.Feedbacks.Add(feedbackWithoutId);
                await _context.SaveChangesAsync();
                return feedbackWithoutId;
            }
        }

        public async Task<Feedback> UpdateFeedbackAsync(Feedback feedback)
        {
            _context.Entry(feedback).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<bool> DeleteFeedbackAsync(int feedbackId)
        {
            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null) return false;

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Feedback>> GetFeedbacksByRatingAsync(int rating)
        {
            return await _context.Feedbacks
                .Where(f => f.Rating == rating)
                .Include(f => f.Customer)
                .ToListAsync();
        }
    }
}