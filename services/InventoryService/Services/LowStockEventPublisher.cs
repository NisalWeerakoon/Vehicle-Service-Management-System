using System.Text.Json;
using Confluent.Kafka;
using InventoryService.Events;
using InventoryService.Models;

namespace InventoryService.Services;

public interface ILowStockEventPublisher
{
    Task PublishIfTransitionedToLowStockAsync(SparePart part, bool wasLowStock, CancellationToken cancellationToken = default);
}

public class LowStockEventPublisher : ILowStockEventPublisher
{
    private const string Topic = "vsc.inventory.low-stock-detected";
    private readonly IConfiguration _configuration;
    private readonly ILogger<LowStockEventPublisher> _logger;

    public LowStockEventPublisher(IConfiguration configuration, ILogger<LowStockEventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishIfTransitionedToLowStockAsync(SparePart part, bool wasLowStock, CancellationToken cancellationToken = default)
    {
        var isLowStock = part.Quantity <= part.LowStockThreshold;
        if (wasLowStock || !isLowStock) return;

        var evt = new LowStockDetectedEvent
        {
            Data = new LowStockDetectedEventData
            {
                SparePartId = part.Id,
                SparePartName = part.Name,
                CurrentQuantity = part.Quantity,
                LowStockThreshold = part.LowStockThreshold
            }
        };

        var config = new ProducerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.All,
            MessageTimeoutMs = 2000
        };

        try
        {
            using var producer = new ProducerBuilder<Null, string>(config).Build();
            await producer.ProduceAsync(Topic, new Message<Null, string> { Value = JsonSerializer.Serialize(evt) }, cancellationToken);
            _logger.LogInformation("Published LowStockDetected event {EventId} for spare part {SparePartId}.", evt.EventId, part.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stock was updated but LowStockDetected publication failed for spare part {SparePartId}.", part.Id);
        }
    }
}
