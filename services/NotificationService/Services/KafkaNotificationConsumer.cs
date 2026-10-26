using System.Text.Json;
using Confluent.Kafka;

namespace NotificationService.Services;

public class KafkaNotificationConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<KafkaNotificationConsumer> logger) : BackgroundService
{
    private const string ConsumerGroup = "notification-group";

    private static readonly string[] Topics =
    [
        "vsc.booking.created",
        "vsc.vehicle.checked-in",
        "vsc.service.completed",
        "vsc.vehicle.ready-for-collection",
        "vsc.invoice.generated",
        "vsc.payment.recorded"
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var bootstrapServers = configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            logger.LogError("Kafka:BootstrapServers is missing. Notification consumer will not start.");
            return;
        }

        IConsumer<Ignore, string>? consumer = null;
        try
        {
            consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = ConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            }).Build();
            consumer.Subscribe(Topics);
            logger.LogInformation(
                "Subscribed to Kafka topics {Topics} with group {ConsumerGroup}.",
                string.Join(", ", Topics), ConsumerGroup);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    using var scope = scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IKafkaNotificationProcessor>();
                    var outcome = await processor.ProcessAsync(result.Topic, result.Message.Value, stoppingToken);

                    consumer.Commit(result);
                    if (outcome == KafkaNotificationProcessingResult.MissingRecipient)
                        logger.LogWarning(
                            "Committed {Topic} offset {Offset} without a notification because the event had no recipient user ID.",
                            result.Topic, result.Offset.Value);
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Kafka consume error. The notification API remains available.");
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Invalid Kafka notification event. The message was not committed.");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Kafka notification processing failed; the message was not committed.");
                }
            }
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Notification Kafka consumer could not start. The notification API remains available.");
        }
        finally
        {
            consumer?.Close();
            consumer?.Dispose();
        }
    }
}
