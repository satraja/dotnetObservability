# Trace ID Integration - Quick Start for Other Controllers

This guide shows how to add trace ID support to remaining controllers.

## Pattern to Follow

The trace ID pattern has been implemented in:
- ✅ `OrdersController.cs`
- ✅ `ServicesController.cs`

Follow this same pattern for other controllers.

---

## Step-by-Step Integration

### Step 1: Add Using Statement
```csharp
using ObeservabiltyTraceabilty.Extensions;
```

### Step 2: Get Trace ID at Start of Each Action
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<MyDto>>> GetAll()
{
	var traceId = HttpContext.GetTraceId();  // ← Add this line

	// ... rest of method
}
```

### Step 3: Update Logging Statements
**Before:**
```csharp
_logger.LogInformation("Retrieved {Count} items", items.Count);
```

**After:**
```csharp
_logger.LogInformation("Retrieved {Count} items - TraceId: {TraceId}", items.Count, traceId);
```

### Step 4: Include Trace ID in Responses
**Before:**
```csharp
return Ok(dtos);
```

**After:**
```csharp
return Ok(new { success = true, data = dtos, traceId = traceId, count = dtos.Count });
```

**Error Response Before:**
```csharp
return StatusCode(500, "Internal server error");
```

**Error Response After:**
```csharp
return StatusCode(500, new { success = false, message = "Internal server error", traceId = traceId });
```

---

## Template Code

Copy/paste this template for each controller method:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<MyDto>>> GetAll()
{
	var traceId = HttpContext.GetTraceId();
	var stopwatch = Stopwatch.StartNew();
	try
	{
		_logger.LogInformation("GetAll started - TraceId: {TraceId}", traceId);

		var items = await _service.GetAllAsync();
		var dtos = items.Select(x => MapToDto(x)).ToList();

		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);

		_logger.LogInformation("Retrieved {Count} items - TraceId: {TraceId}", 
			dtos.Count, traceId);

		return Ok(new { 
			success = true, 
			data = dtos, 
			traceId = traceId, 
			count = dtos.Count 
		});
	}
	catch (Exception ex)
	{
		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);

		_logger.LogError(ex, "Error retrieving items - TraceId: {TraceId}", traceId);
		MetricsService.ApiExceptions.WithLabels("GetAll", "500").Inc();

		return StatusCode(500, new { 
			success = false, 
			message = "Internal server error", 
			traceId = traceId 
		});
	}
}
```

---

## Common Patterns

### Single Resource by ID
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<MyDto>> GetById(int id)
{
	var traceId = HttpContext.GetTraceId();
	var stopwatch = Stopwatch.StartNew();
	try
	{
		_logger.LogInformation("GetById started for Id: {Id} - TraceId: {TraceId}", 
			id, traceId);

		var item = await _service.GetByIdAsync(id);
		if (item == null)
		{
			_logger.LogWarning("Item {Id} not found - TraceId: {TraceId}", id, traceId);
			return NotFound(new { 
				success = false, 
				message = "Item not found", 
				traceId = traceId 
			});
		}

		var dto = MapToDto(item);
		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);

		_logger.LogInformation("Item {Id} retrieved - TraceId: {TraceId}", id, traceId);

		return Ok(new { 
			success = true, 
			data = dto, 
			traceId = traceId 
		});
	}
	catch (Exception ex)
	{
		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
		_logger.LogError(ex, "Error retrieving item {Id} - TraceId: {TraceId}", id, traceId);
		MetricsService.ApiExceptions.WithLabels("GetById", "500").Inc();

		return StatusCode(500, new { 
			success = false, 
			message = "Internal server error", 
			traceId = traceId 
		});
	}
}
```

### Create Resource
```csharp
[HttpPost]
public async Task<ActionResult<MyDto>> Create([FromBody] CreateMyDto dto)
{
	var traceId = HttpContext.GetTraceId();
	var stopwatch = Stopwatch.StartNew();
	try
	{
		_logger.LogInformation("Create started - TraceId: {TraceId}", traceId);

		var entity = new MyEntity { /* map from dto */ };
		var result = await _service.CreateAsync(entity);

		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);

		_logger.LogInformation("Item created with Id: {Id} - TraceId: {TraceId}", 
			result.Id, traceId);

		return CreatedAtAction("GetById", new { id = result.Id }, new { 
			success = true, 
			data = MapToDto(result), 
			traceId = traceId 
		});
	}
	catch (Exception ex)
	{
		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
		_logger.LogError(ex, "Error creating item - TraceId: {TraceId}", traceId);
		MetricsService.ApiExceptions.WithLabels("Create", "500").Inc();

		return StatusCode(500, new { 
			success = false, 
			message = "Internal server error", 
			traceId = traceId 
		});
	}
}
```

### Update Resource
```csharp
[HttpPut("{id}")]
public async Task<ActionResult<MyDto>> Update(int id, [FromBody] UpdateMyDto dto)
{
	var traceId = HttpContext.GetTraceId();
	var stopwatch = Stopwatch.StartNew();
	try
	{
		_logger.LogInformation("Update started for Id: {Id} - TraceId: {TraceId}", 
			id, traceId);

		var entity = await _service.GetByIdAsync(id);
		if (entity == null)
		{
			return NotFound(new { 
				success = false, 
				message = "Item not found", 
				traceId = traceId 
			});
		}

		// Update entity from dto
		entity.Property = dto.Property;

		await _service.UpdateAsync(entity);

		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);

		_logger.LogInformation("Item {Id} updated - TraceId: {TraceId}", id, traceId);

		return Ok(new { 
			success = true, 
			data = MapToDto(entity), 
			traceId = traceId 
		});
	}
	catch (Exception ex)
	{
		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
		_logger.LogError(ex, "Error updating item {Id} - TraceId: {TraceId}", id, traceId);
		MetricsService.ApiExceptions.WithLabels("Update", "500").Inc();

		return StatusCode(500, new { 
			success = false, 
			message = "Internal server error", 
			traceId = traceId 
		});
	}
}
```

### Delete Resource
```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
	var traceId = HttpContext.GetTraceId();
	var stopwatch = Stopwatch.StartNew();
	try
	{
		_logger.LogInformation("Delete started for Id: {Id} - TraceId: {TraceId}", 
			id, traceId);

		var result = await _service.DeleteAsync(id);
		if (!result)
		{
			return NotFound(new { 
				success = false, 
				message = "Item not found", 
				traceId = traceId 
			});
		}

		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);

		_logger.LogInformation("Item {Id} deleted - TraceId: {TraceId}", id, traceId);

		return Ok(new { 
			success = true, 
			message = "Item deleted", 
			traceId = traceId 
		});
	}
	catch (Exception ex)
	{
		stopwatch.Stop();
		MetricsService.ApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
		_logger.LogError(ex, "Error deleting item {Id} - TraceId: {TraceId}", id, traceId);
		MetricsService.ApiExceptions.WithLabels("Delete", "500").Inc();

		return StatusCode(500, new { 
			success = false, 
			message = "Internal server error", 
			traceId = traceId 
		});
	}
}
```

---

## Controllers to Update

### Remaining Controllers:

1. **CustomersController.cs**
   - GetAllCustomers()
   - GetCustomerById(int id)
   - CreateCustomer(CreateCustomerDto dto)
   - UpdateCustomer(int id, UpdateCustomerDto dto)
   - DeleteCustomer(int id)

2. **ProductsController.cs**
   - GetAllProducts()
   - GetProductById(int id)
   - CreateProduct(CreateProductDto dto)
   - UpdateProduct(int id, UpdateProductDto dto)
   - DeleteProduct(int id)

3. **ActivitiesController.cs**
   - GetAllActivities()
   - GetActivityById(int id)
   - GetActivitiesForEntity(string entityType, int entityId)
   - GetActivitiesByType(string type)

4. **CustomerLogsController.cs**
   - GetAllCustomerLogs()
   - GetCustomerLogById(int id)
   - GetLogsForCustomer(int customerId)
   - CreateCustomerLog(CreateCustomerLogDto dto)

---

## Verification Checklist

After updating each controller method:

- [ ] Added `using ObeservabiltyTraceabilty.Extensions;`
- [ ] Get trace ID at start: `var traceId = HttpContext.GetTraceId();`
- [ ] Updated all `_logger` calls to include `TraceId: {TraceId}`
- [ ] Updated `return Ok()` to include `traceId` in response
- [ ] Updated error responses to include `traceId`
- [ ] Tested the endpoint returns trace ID
- [ ] Verified logs show trace ID
- [ ] Tested with custom X-Trace-Id header

---

## Full Example Controller

Here's a complete example of how a controller should look after updating:

```csharp
using Microsoft.AspNetCore.Mvc;
using ObeservabiltyTraceabilty.DTOs;
using ObeservabiltyTraceabilty.Extensions;  // ← Add this
using ObeservabiltyTraceabilty.Models;
using ObeservabiltyTraceabilty.Services;
using System.Diagnostics;

namespace ObeservabiltyTraceabilty.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
	private readonly IMyService _service;
	private readonly ILogger<MyController> _logger;

	public MyController(IMyService service, ILogger<MyController> logger)
	{
		_service = service;
		_logger = logger;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<MyDto>>> GetAll()
	{
		var traceId = HttpContext.GetTraceId();  // ← Get trace ID
		var stopwatch = Stopwatch.StartNew();
		try
		{
			_logger.LogInformation("GetAll started - TraceId: {TraceId}", traceId);
			var items = await _service.GetAllAsync();
			var dtos = items.Select(x => MapToDto(x)).ToList();
			stopwatch.Stop();
			MetricsService.MyApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
			_logger.LogInformation("Retrieved {Count} items - TraceId: {TraceId}", 
				dtos.Count, traceId);

			return Ok(new { 
				success = true, 
				data = dtos, 
				traceId = traceId, 
				count = dtos.Count 
			});
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			MetricsService.MyApiDuration.Observe(stopwatch.Elapsed.TotalSeconds);
			_logger.LogError(ex, "Error retrieving items - TraceId: {TraceId}", traceId);
			MetricsService.ApiExceptions.WithLabels("GetAll", "500").Inc();

			return StatusCode(500, new { 
				success = false, 
				message = "Internal server error", 
				traceId = traceId 
			});
		}
	}

	private MyDto MapToDto(MyEntity entity) => new MyDto { /* mapping */ };
}
```

---

## Minimal Change Example

If you want **minimal changes** to existing code:

```csharp
// Just add this one line per method
var traceId = HttpContext.GetTraceId();

// Then include it in key logs and responses
_logger.LogInformation("Action completed - TraceId: {TraceId}", traceId);
return Ok(new { data = result, traceId = traceId });
```

This ensures trace ID is available without major refactoring.

---

## Tips & Tricks

### Quick Copy-Paste
1. Copy template from this guide
2. Replace `MyEntity` with your entity name
3. Replace `MyService` with your service interface
4. Replace `MapToDto()` with your mapping logic
5. Paste into controller
6. Update metric names if needed

### Validation First
Test one controller completely before moving to next:
1. Update controller
2. Build project (`run_build`)
3. Test endpoint with curl/Postman
4. Verify response includes trace ID
5. Check logs show trace ID
6. Move to next controller

### Keeping It DRY (Don't Repeat Yourself)
If you want less duplication, consider creating a base controller:

```csharp
public abstract class TracedControllerBase : ControllerBase
{
	protected string GetTraceId() => HttpContext.GetTraceId();

	protected IActionResult OkWithTrace<T>(T data, string? message = null)
	{
		return Ok(new { success = true, data, traceId = GetTraceId(), message });
	}

	protected IActionResult ErrorWithTrace(string message, int statusCode = 500)
	{
		return StatusCode(statusCode, new { success = false, message, traceId = GetTraceId() });
	}
}
```

Then controllers can use:
```csharp
public class MyController : TracedControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var items = await _service.GetAllAsync();
		return OkWithTrace(items);
	}
}
```

---

## Build & Test

After updating controllers:

```powershell
# Build project
dotnet build

# Test an endpoint
$response = Invoke-WebRequest -Uri "https://localhost:7000/api/myresource" `
	-SkipCertificateCheck
$response.Content | ConvertFrom-Json | Select-Object traceId
```

All trace IDs should be propagated across your entire API! 🚀
