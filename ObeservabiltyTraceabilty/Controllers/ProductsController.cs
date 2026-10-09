using Microsoft.AspNetCore.Mvc;
using ObeservabiltyTraceabilty.DTOs;
using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Services;
using System.Diagnostics;

namespace ObeservabiltyTraceabilty.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var dtos = products.Select(p => MapToDto(p)).ToList();
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Retrieved {Count} products", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error retrieving products");
            MetricsService.ApiExceptions.WithLabels("GetAllProducts", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                stopwatch.Stop();
                MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                return NotFound($"Product with id {id} not found");
            }
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            return Ok(MapToDto(product));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error retrieving product {Id}", id);
            MetricsService.ApiExceptions.WithLabels("GetProductById", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var product = new Product
            {
                ProductName = dto.ProductName,
                Description = dto.Description,
                Price = dto.Price,
                CreatedDate = DateTime.UtcNow
            };

            var createdProduct = await _productService.CreateProductAsync(product, "System");
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Product {ProductName} created successfully", createdProduct.ProductName);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.ProductId }, MapToDto(createdProduct));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error creating product");
            MetricsService.ApiExceptions.WithLabels("CreateProduct", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                stopwatch.Stop();
                MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                return NotFound($"Product with id {id} not found");
            }

            if (!string.IsNullOrEmpty(dto.ProductName))
                product.ProductName = dto.ProductName;
            if (!string.IsNullOrEmpty(dto.Description))
                product.Description = dto.Description;
            if (dto.Price.HasValue && dto.Price > 0)
                product.Price = dto.Price.Value;

            var updatedProduct = await _productService.UpdateProductAsync(product, "System");
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Product {Id} updated successfully", id);
            return Ok(MapToDto(updatedProduct));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error updating product {Id}", id);
            MetricsService.ApiExceptions.WithLabels("UpdateProduct", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                stopwatch.Stop();
                MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
                return NotFound($"Product with id {id} not found");
            }

            await _productService.DeleteProductAsync(id, "System");
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogInformation("Product {Id} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            MetricsService.ProductApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
            _logger.LogError(ex, "Error deleting product {Id}", id);
            MetricsService.ApiExceptions.WithLabels("DeleteProduct", "500").Inc();
            return StatusCode(500, "Internal server error");
        }
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Description = product.Description,
            Price = product.Price,
            CreatedDate = product.CreatedDate
        };
    }
}
