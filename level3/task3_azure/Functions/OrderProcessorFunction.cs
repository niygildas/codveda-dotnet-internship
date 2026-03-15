using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureIntegration.Functions;

public class OrderProcessorFunction
{
    private readonly ILogger<OrderProcessorFunction> _logger;
    public OrderProcessorFunction(ILogger<OrderProcessorFunction> logger) => _logger = logger;

    [Function("ProcessOrder")]
    public async Task<HttpResponseData> ProcessOrder(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ProcessOrder")] HttpRequestData req)
    {
        _logger.LogInformation("ProcessOrder triggered.");
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var order = JsonSerializer.Deserialize<OrderRequest>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (order is null || string.IsNullOrEmpty(order.OrderId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid order payload.");
            return bad;
        }

        _logger.LogInformation("Processing order {OrderId}", order.OrderId);
        var result = new { order.OrderId, Status = "Processed", ProcessedAt = DateTime.UtcNow };
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(result));
        return response;
    }

    [Function("DailyOrderCleanup")]
    public void DailyCleanup([TimerTrigger("0 0 0 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("DailyOrderCleanup triggered at {Time}", DateTime.UtcNow);
    }
}

public record OrderRequest(string OrderId, string ProductId, int Quantity);
