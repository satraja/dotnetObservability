using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Repositories;

namespace ObeservabiltyTraceabilty.Services;

public interface IProductService
{
    Task<Product?> GetProductByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product> CreateProductAsync(Product product, string createdBy);
    Task<Product> UpdateProductAsync(Product product, string updatedBy);
    Task DeleteProductAsync(int id, string deletedBy);
}

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Activity> _activityRepository;

    public ProductService(IRepository<Product> productRepository, IRepository<Activity> activityRepository)
    {
        _productRepository = productRepository;
        _activityRepository = activityRepository;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product> CreateProductAsync(Product product, string createdBy)
    {
        var createdProduct = await _productRepository.AddAsync(product);

        MetricsService.ProductsCreated.Inc();

        await _activityRepository.AddAsync(new Activity
        {
            EntityType = "Product",
            EntityId = createdProduct.ProductId,
            ActionType = "Create",
            ChangedBy = createdBy,
            Timestamp = DateTime.UtcNow,
            NewValue = $"Product {createdProduct.ProductName} created",
            ManagementIcon = "success",
            ProductId = createdProduct.ProductId
        });

        return createdProduct;
    }

    public async Task<Product> UpdateProductAsync(Product product, string updatedBy)
    {
        var oldProduct = await _productRepository.GetByIdAsync(product.ProductId);
        var updatedProduct = await _productRepository.UpdateAsync(product);

        MetricsService.ProductsUpdated.Inc();

        await _activityRepository.AddAsync(new Activity
        {
            EntityType = "Product",
            EntityId = updatedProduct.ProductId,
            ActionType = "Update",
            ChangedBy = updatedBy,
            Timestamp = DateTime.UtcNow,
            OldValue = oldProduct?.ProductName,
            NewValue = $"Product {updatedProduct.ProductName} updated",
            ManagementIcon = "info",
            ProductId = updatedProduct.ProductId
        });

        return updatedProduct;
    }

    public async Task DeleteProductAsync(int id, string deletedBy)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product != null)
        {
            await _productRepository.DeleteAsync(id);

            MetricsService.ProductsDeleted.Inc();

            await _activityRepository.AddAsync(new Activity
            {
                EntityType = "Product",
                EntityId = id,
                ActionType = "Delete",
                ChangedBy = deletedBy,
                Timestamp = DateTime.UtcNow,
                OldValue = $"Product {product.ProductName}",
                ManagementIcon = "warning",
                ProductId = id
            });
        }
    }
}
