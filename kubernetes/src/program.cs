using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Codveda K8s Products API", Version = "v1" });
});
builder.Services.AddSingleton<ProductService>();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" })
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

public record Product(int Id, string Name, string Category, decimal Price, int Stock);
public record CreateProductRequest(string Name, string Category, decimal Price, int Stock);

public class ProductService
{
    private static readonly List<Product> _products = new()
    {
        new Product(1, "Laptop Pro",          "Electronics", 999.99m,  50),
        new Product(2, "Mechanical Keyboard", "Electronics", 89.99m,  200),
        new Product(3, "Standing Desk",       "Furniture",   449.99m,  30),
        new Product(4, "Monitor 4K",          "Electronics", 379.99m,  75),
        new Product(5, "Ergonomic Chair",     "Furniture",   299.99m,  20),
    };
    private static int _nextId = 6;

    public IEnumerable<Product> GetAll(string? category = null) =>
        category == null ? _products : _products.Where(p => p.Category == category);

    public Product? GetById(int id) =>
        _products.FirstOrDefault(p => p.Id == id);

    public Product Create(CreateProductRequest req)
    {
        var product = new Product(_nextId++, req.Name, req.Category, req.Price, req.Stock);
        _products.Add(product);
        return product;
    }

    public bool Delete(int id) => _products.RemoveAll(p => p.Id == id) > 0;
}

public class DatabaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        return HealthCheckResult.Healthy("Database connection OK");
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? category = null)
    {
        _logger.LogInformation("GetAll called, category={Category}", category ?? "all");
        return Ok(_service.GetAll(category));
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var product = _service.GetById(id);
        return product == null ? NotFound(new { error = $"Product {id} not found" }) : Ok(product);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductRequest req)
    {
        var product = _service.Create(req);
        _logger.LogInformation("Created product {Id}: {Name}", product.Id, product.Name);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id) =>
        _service.Delete(id) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/[controller]")]
public class InfoController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        app = "Codveda Products API",
        version = "1.0.0",
        description = "Production-ready API designed for Kubernetes",
        podName = Environment.GetEnvironmentVariable("POD_NAME") ?? "local",
        nodeName = Environment.GetEnvironmentVariable("NODE_NAME") ?? "local",
        features = new[]
        {
            "3 replicas with rolling update strategy",
            "Liveness probe at /health/live",
            "Readiness probe at /health/ready",
            "Horizontal Pod Autoscaler — 2 to 10 pods",
            "ConfigMaps and Secrets management",
            "CPU and memory resource limits"
        },
        timestamp = DateTime.UtcNow
    });
}