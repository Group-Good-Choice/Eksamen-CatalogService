using Microsoft.Extensions.Caching.Memory;
using Models;
using MongoDB.Driver;

namespace CatalogService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(IConfiguration configuration, IMemoryCache memoryCache, ILogger<ProductRepository> logger)
    {
        var connectionString = configuration.GetValue<string>("MONGODB_CONNECTION_STRING")
            ?? configuration.GetSection("MongoDb")["ConnectionString"]
            ?? "mongodb://localhost:27017";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("CatalogDb");
        _products = database.GetCollection<Product>("Products");

        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<List<Product>> GetAllAsync() =>
        await _products.Find(_ => true).ToListAsync();

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var cached = GetProductFromCache(id);
        if (cached != null)
        {
            _logger.LogInformation("Cache HIT for product {Id}", id);
            return cached;
        }

        _logger.LogInformation("Cache MISS for product {Id} — fetching from database", id);
        var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (product != null)
            SetProductInCache(product);

        return product;
    }

    public async Task CreateAsync(Product product)
    {
        await _products.InsertOneAsync(product);
        SetProductInCache(product);
        _logger.LogInformation("Product {Id} created and stored in cache", product.Id);
    }

    public async Task<bool> UpdateAsync(Guid id, Product product)
    {
        product.Id = id;
        var result = await _products.ReplaceOneAsync(p => p.Id == id, product);
        if (result.ModifiedCount > 0)
        {
            SetProductInCache(product);
            _logger.LogInformation("Product {Id} updated and cache refreshed", id);
        }
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _products.DeleteOneAsync(p => p.Id == id);
        if (result.DeletedCount > 0)
        {
            RemoveFromCache(id);
            _logger.LogInformation("Product {Id} deleted and removed from cache", id);
        }
        return result.DeletedCount > 0;
    }

    // ── Cache helpers ─────────────────────────────────────────────────────

    private void SetProductInCache(Product product)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.Now.AddHours(1),
            SlidingExpiration = TimeSpan.FromMinutes(10),
            Priority = CacheItemPriority.High
        };
        _memoryCache.Set(product.Id, product, options);
    }

    private Product? GetProductFromCache(Guid id)
    {
        _memoryCache.TryGetValue(id, out Product? product);
        return product;
    }

    private void RemoveFromCache(Guid id) => _memoryCache.Remove(id);
}
