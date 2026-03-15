using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq",
        UserName = "guest",
        Password = "guest",
        DispatchConsumersAsync = true
    };
    return factory.CreateConnection();
});

builder.Services.AddHostedService<OrderCreatedConsumer>();

var app = builder.Build();
app.UseHealthChecks("/health");
app.MapControllers();
app.Run();

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private IModel? _channel;

    public OrderCreatedConsumer(IConnection connection) => _connection = connection;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateModel();
        _channel.QueueDeclare("order-created", durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var order = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

            if (order is not null)
            {
                Console.WriteLine($"[Notification] New order: {order.OrderId} | Product: {order.ProductId} | Qty: {order.Quantity}");
                Console.WriteLine($"[Notification] Email/SMS sent for order {order.OrderId}");
            }

            _channel.BasicAck(ea.DeliveryTag, multiple: false);
            await Task.Yield();
        };

        _channel.BasicConsume(queue: "order-created", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose() { _channel?.Close(); base.Dispose(); }
}

public record OrderCreatedEvent(Guid OrderId, string ProductId, int Quantity, DateTime CreatedAt);
