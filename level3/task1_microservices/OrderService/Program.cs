using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
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
        UserName = builder.Configuration["RabbitMQ:Username"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Password"] ?? "guest"
    };
    return factory.CreateConnection();
});

builder.Services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();

var app = builder.Build();
app.UseHealthChecks("/health");
app.MapControllers();
app.Run();

public record Order(Guid Id, string ProductId, int Quantity, string Status, DateTime CreatedAt);
public record CreateOrderRequest(string ProductId, int Quantity);

public interface IMessagePublisher
{
    void Publish(string queue, object message);
}

public class RabbitMQPublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    public RabbitMQPublisher(IConnection connection) => _connection = connection;

    public void Publish(string queue, object message)
    {
        using var channel = _connection.CreateModel();
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: props, body: body);
    }
}
