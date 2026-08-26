using System.Text.Json;
using Confluent.Kafka;
using CustomerBookingService.Events;
using CustomerBookingService.Models;

namespace CustomerBookingService.Services;

public interface IBookingEventPublisher
{
    Task PublishBookingCreatedAsync(
        Booking booking,
        CancellationToken cancellationToken = default);
}

public class BookingEventPublisher : IBookingEventPublisher
{
    private const string BookingCreatedTopic =
        "vsc.booking.created";

    private readonly IProducer<string, string> _producer;
    private readonly ILogger<BookingEventPublisher> _logger;

    public BookingEventPublisher(
        IConfiguration configuration,
        ILogger<BookingEventPublisher> logger)
    {
        _logger = logger;

        var bootstrapServers =
            configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            throw new InvalidOperationException(
                "Kafka:BootstrapServers is missing."
            );
        }

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,

            Acks = Acks.All
        };

        _producer =
            new ProducerBuilder<string, string>(
                producerConfig
            )
            .Build();
    }

    public async Task PublishBookingCreatedAsync(
        Booking booking,
        CancellationToken cancellationToken = default)
    {
        var domainEvent = new BookingCreatedEvent
        {
            CorrelationId =
                booking.BookingReference,

            Data = new BookingCreatedData
            {
                BookingId =
                    booking.Id,

                BookingReference =
                    booking.BookingReference,

                CustomerId =
                    booking.CustomerId,

                VehicleId =
                    booking.VehicleId,

                VehicleRegistrationNumber =
                    booking.Vehicle?.RegistrationNumber
                    ?? string.Empty,

                PreferredDate =
                    booking.PreferredDate,

                RequestedServiceOrProblem =
                    booking.RequestedServiceOrProblem,

                Status =
                    booking.Status.ToString()
            }
        };

        var json =
            JsonSerializer.Serialize(
                domainEvent,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase
                }
            );

        var message = new Message<string, string>
        {
            Key = booking.BookingReference,

            Value = json
        };

        var result =
            await _producer.ProduceAsync(
                BookingCreatedTopic,
                message,
                cancellationToken
            );

        _logger.LogInformation(
            "Published BookingCreated event for {BookingReference} to {Topic} partition {Partition} offset {Offset}",
            booking.BookingReference,
            result.Topic,
            result.Partition.Value,
            result.Offset.Value
        );
    }
}