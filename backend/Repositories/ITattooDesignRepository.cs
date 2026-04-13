using TattooShop.Api.Models;

namespace TattooShop.Api.Repositories;

public interface ITattooDesignRepository
{
    Task AddDesignAsync(TattooDesign design);
    Task<IEnumerable<TattooDesign>> GetDesignsByCustomerAsync(string customerId);
    Task<TattooDesign?> GetDesignByIdAsync(string id, string customerId);
}