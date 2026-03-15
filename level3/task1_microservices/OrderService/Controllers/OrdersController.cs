using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static readonly List<Order> _orders = new();
    private readonly IMessagePublisher _publisher;

    public OrdersController(IMessagePublisher publisher) => _publisher = publisher;

    [HttpGet]
    public IActionResult GetAll() => Ok(_orders);

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductId) || request.Quantity <= 0)
            return BadRequest("Invalid order data.");

        var order = new Order(
            Id: Guid.NewGuid(),
            ProductId: request.ProductId,
            Quantity: request.Quantity,
            Status: "Pending",
            CreatedAt: DateTime.UtcNow
        );

        _orders.Add(order);

        _publisher.Publish("order-created", new
        {
            OrderId = order.Id,
            order.ProductId,
            order.Quantity,
            order.CreatedAt
        });

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order is null) return NotFound();
        _orders.Remove(order);
        return NoContent();
    }
}
