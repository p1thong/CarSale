using ASM1.Repository.Models;

namespace ASM1.Repository.Repositories.Interfaces
{
    public interface IQuotationRepository
    {
        Task<IEnumerable<Quotation>> GetAllQuotationsAsync();
        Task<Quotation?> GetQuotationByIdAsync(int quotationId);
        Task<IEnumerable<Quotation>> GetQuotationsByCustomerAsync(int customerId);
        Task<IEnumerable<Quotation>> GetQuotationsByDealerAsync(int dealerId);
        Task<Quotation> CreateQuotationAsync(Quotation quotation);
        Task<Quotation> UpdateQuotationAsync(Quotation quotation);
        Task<bool> DeleteQuotationAsync(int quotationId);
        Task<IEnumerable<Quotation>> GetQuotationsByStatusAsync(string status);
    }
}