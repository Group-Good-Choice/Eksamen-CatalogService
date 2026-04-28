using System.Diagnostics;
using CatalogService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductRepository repository, ILogger<ProductsController> logger)
    {
        _repository = repository;
        _logger = logger;

        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var ipAddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, "Catalog Service responding from {IpAddress}", ipAddr);
    }

    [HttpGet("version")]
    public async Task<Dictionary<string, string>> GetVersion()
    {
        var properties = new Dictionary<string, string>();
        properties.Add("service", "Good Choice Catalog Service");
        var ver = FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion;
        properties.Add("version", ver!);
        try
        {
            var hostName = System.Net.Dns.GetHostName();
            var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);
            properties.Add("hosted-at-address", ips.First().MapToIPv4().ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            properties.Add("hosted-at-address", "Could not resolve IP-address");
        }
        return properties;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        _logger.LogInformation("GetAll called");
        var products = await _repository.GetAllAsync();
        _logger.LogInformation("Returning {Count} products", products.Count);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(Guid id)
    {
        _logger.LogInformation("GetById called with id {Id}", id);
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Product with id {Id} not found", id);
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        if (product.Id == Guid.Empty)
            product.Id = Guid.NewGuid();

        _logger.LogInformation("Creating product with id {Id}", product.Id);
        await _repository.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Update(Guid id, Product product)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            _logger.LogWarning("Update failed: product with id {Id} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Updating product with id {Id}", id);
        await _repository.UpdateAsync(id, product);
        product.Id = id;
        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            _logger.LogWarning("Delete failed: product with id {Id} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Deleting product with id {Id}", id);
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
