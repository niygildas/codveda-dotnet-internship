using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<BlogDbContext>(opt => opt.UseSqlite("Data Source=blog.db"));
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
    db.Database.EnsureCreated();
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.Run();
app.MapGet("/", () => "Codveda Blog API is running 🚀");

public class BlogPost
{
    public int Id { get; set; }
    [Required] public string Title { get; set; } = "";
    [Required] public string Content { get; set; } = "";
    [Required] public string Author { get; set; } = "";
    public string Category { get; set; } = "General";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; }
    public int ViewCount { get; set; }
}

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }
    public DbSet<BlogPost> BlogPosts { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<BlogPost>().HasData(
            new BlogPost { Id = 1, Title = "Getting Started with .NET 8", Author = "Gildas", Category = "Tech", Content = "Welcome to .NET 8!", IsPublished = true, CreatedAt = new DateTime(2026,1,1) },
            new BlogPost { Id = 2, Title = "Microservices with Docker", Author = "Gildas", Category = "DevOps", Content = "Learn Docker and .NET.", IsPublished = true, CreatedAt = new DateTime(2026,1,2) },
            new BlogPost { Id = 3, Title = "Azure Cloud Deployment", Author = "Gildas", Category = "Cloud", Content = "Deploy to Azure.", IsPublished = false, CreatedAt = new DateTime(2026,1,3) }
        );
    }
}

public interface IBlogRepository
{
    Task<IEnumerable<BlogPost>> GetAllAsync(string? category = null);
    Task<BlogPost?> GetByIdAsync(int id);
    Task<BlogPost> CreateAsync(BlogPost post);
    Task<bool> DeleteAsync(int id);
}

public class BlogRepository : IBlogRepository
{
    private readonly BlogDbContext _db;
    public BlogRepository(BlogDbContext db) => _db = db;
    public async Task<IEnumerable<BlogPost>> GetAllAsync(string? category = null)
    {
        var q = _db.BlogPosts.AsQueryable();
        if (!string.IsNullOrEmpty(category)) q = q.Where(p => p.Category == category);
        return await q.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }
    public async Task<BlogPost?> GetByIdAsync(int id) => await _db.BlogPosts.FirstOrDefaultAsync(p => p.Id == id);
    public async Task<BlogPost> CreateAsync(BlogPost post) { _db.BlogPosts.Add(post); await _db.SaveChangesAsync(); return post; }
    public async Task<bool> DeleteAsync(int id) { var p = await _db.BlogPosts.FindAsync(id); if (p == null) return false; _db.BlogPosts.Remove(p); await _db.SaveChangesAsync(); return true; }
}

[ApiController]
[Route("api/[controller]")]
public class BlogApiController : ControllerBase
{
    private readonly IBlogRepository _repo;
    public BlogApiController(IBlogRepository repo) => _repo = repo;
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] string? category = null) => Ok(await _repo.GetAllAsync(category));
    [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) { var p = await _repo.GetByIdAsync(id); return p == null ? NotFound() : Ok(p); }
    [HttpPost] public async Task<IActionResult> Create([FromBody] BlogPost post) { if (!ModelState.IsValid) return BadRequest(ModelState); var created = await _repo.CreateAsync(post); return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); }
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id) => await _repo.DeleteAsync(id) ? NoContent() : NotFound();
}




