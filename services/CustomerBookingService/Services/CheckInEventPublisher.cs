using System.Text.Json;
using Confluent.Kafka;
using CustomerBookingService.Events;
using CustomerBookingService.Models;

namespace CustomerBookingService.Services;

public interface ICheckInEventPublisher
{
    Task PublishVehicleCheckedInAsync(
        CheckIn checkIn,
        CancellationToken cancellationToken = default);
}

public class CheckInEventPublisher : ICheckInEventPublisher
{
    private const string VehicleCheckedInTopic =
        "vsc.vehicle.checked-in";

    private readonly IProducer<string, string> _producer;
    private readonly ILogger<CheckInEventPublisher> _logger;

    public CheckInEventPublisher(
        IConfiguration configuration,
        ILogger<CheckInEventPublisher> logger)
    {
        _logger = logger;

        var bootstrapServers =
            configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            throw new InvalidOperationException(
                "Kafka:BootstrapServers is missing.");
        }

        _producer = new ProducerBuilder<string, string>(
            new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All
            })
            .Build();
    }

    public async Task PublishVehicleCheckedInAsync(
        CheckIn checkIn,
        CancellationToken cancellationToken = default)
    {
        var domainEvent = new VehicleCheckedInEvent
        {
            CorrelationId =
                checkIn.Booking?.BookingReference
                ?? $"CHK-{checkIn.Id}",

            Data = new VehicleCheckedInData
            {
                CheckInId = checkIn.Id,
                BookingId = checkIn.BookingId,
                BookingReference = checkIn.Booking?.BookingReference,
                CustomerId = checkIn.CustomerId,
                VehicleId = checkIn.VehicleId,
                VehicleRegistrationNumber =
                    checkIn.Vehicle?.RegistrationNumber ?? string.Empty,
                CheckInDateTime = checkIn.CheckInDateTime,
                Mileage = checkIn.Mileage,
                ReportedProblems = checkIn.ReportedProblems,
                IsWalkIn = checkIn.IsWalkIn,
                ServiceStatus =
                    checkIn.Booking?.Status.ToString()
                    ?? "CheckedIn"
            }
        };

        var json = JsonSerializer.Serialize(
            domainEvent,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        var message = new Message<string, string>
        {
            Key = checkIn.Vehicle?.RegistrationNumber
                  ?? checkIn.Id.ToString(),
            Value = json
        };

        var result = await _producer.ProduceAsync(
            VehicleCheckedInTopic,
            message,
            cancellationToken);

        _logger.LogInformation(
            "Published VehicleCheckedIn event for check-in {CheckInId} to {Topic} partition {Partition} offset {Offset}",
            checkIn.Id,
            result.Topic,
            result.Partition.Value,
            result.Offset.Value);
    }
}
