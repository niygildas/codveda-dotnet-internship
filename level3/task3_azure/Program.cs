using AzureIntegration.Services;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureClients(clients =>
{
    clients.AddBlobServiceClient(builder.Configuration["Azure:BlobStorage:ConnectionString"]);
});

builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
