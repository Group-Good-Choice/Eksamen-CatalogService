using Models;

namespace CatalogService.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task CreateAsync(Product product);
    Task<bool> UpdateAsync(Guid id, Product product);
    Task<bool> DeleteAsync(Guid id);
}
