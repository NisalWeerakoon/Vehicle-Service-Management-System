using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Services;

public enum KafkaNotificationProcessingResult
{
    Created,
    Duplicate,
    MissingRecipient
}

public interface IKafkaNotificationProcessor
{
    Task<KafkaNotificationProcessingResult> ProcessAsync(string topic, string payload, CancellationToken ct = default);
}

public class KafkaNotificationProcessor(
    NotificationDbContext db,
    INotificationService notifications,
    ILogger<KafkaNotificationProcessor> logger) : IKafkaNotificationProcessor
{
    private static readonly IReadOnlyDictionary<string, string> ExpectedEventTypes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["vsc.booking.created"] = "BookingCreated",
            ["vsc.vehicle.checked-in"] = "VehicleCheckedIn",
            ["vsc.service.completed"] = "ServiceCompleted",
            ["vsc.vehicle.ready-for-collection"] = "VehicleReadyForCollection",
            ["vsc.invoice.generated"] = "InvoiceGenerated",
            ["vsc.payment.recorded"] = "PaymentRecorded"
        };

    public async Task<KafkaNotificationProcessingResult> ProcessAsync(
        string topic, string payload, CancellationToken ct = default)
    {
        if (!ExpectedEventTypes.TryGetValue(topic, out var expectedEventType))
            throw new JsonException($"Unsupported Kafka topic: {topic}");

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var eventId = ReadGuid(root, "eventId");
        var eventType = ReadRequiredString(root, "eventType");

        if (!string.Equals(eventType, expectedEventType, StringComparison.Ordinal))
            throw new JsonException($"Topic {topic} does not support event type {eventType}.");

        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            throw new JsonException("Kafka event data is missing.");

        var recipientUserId = ReadOptionalPositiveInt(data, "recipientUserId");
        if (!recipientUserId.HasValue)
        {
            logger.LogWarning(
                "Skipping {EventType} event {EventId}: no authenticated recipient user ID is available.",
                eventType, eventId);
            return KafkaNotificationProcessingResult.MissingRecipient;
        }

        if (await db.ProcessedKafkaEvents.AnyAsync(x => x.EventId == eventId, ct))
        {
            logger.LogInformation("Ignoring duplicate Kafka event {EventId} ({EventType}).", eventId, eventType);
            return KafkaNotificationProcessingResult.Duplicate;
        }

        var content = BuildContent(eventType, data);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await notifications.CreateAsync(
            recipientUserId.Value, content.Title, content.Message, content.Type, ct);

        db.ProcessedKafkaEvents.Add(new ProcessedKafkaEvent
        {
            EventId = eventId,
            EventType = eventType,
            ProcessedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        logger.LogInformation(
            "Created {NotificationType} notification for user {RecipientUserId} from event {EventId}.",
            content.Type, recipientUserId, eventId);
        return KafkaNotificationProcessingResult.Created;
    }

    private static (string Title, string Message, string Type) BuildContent(string eventType, JsonElement data) =>
        eventType switch
        {
            "BookingCreated" => (
                "Booking Received",
                WithOptionalValue("Your service booking request has been received.", "Booking reference", data, "bookingReference"),
                "Booking"),
            "VehicleCheckedIn" => (
                "Vehicle Checked In",
                WithOptionalValue("Your vehicle has been checked in and service processing can begin.", "Vehicle", data, "vehicleRegistrationNumber"),
                "Vehicle"),
            "ServiceCompleted" => (
                "Service Completed",
                WithOptionalValue("Your vehicle service has been completed.", "Vehicle", data, "vehicleRegistrationNumber"),
                "Service"),
            "VehicleReadyForCollection" => (
                "Vehicle Ready for Pickup",
                WithOptionalValue("Your vehicle service is complete and the vehicle is ready for pickup.", "Vehicle", data, "vehicleRegistrationNumber"),
                "Service"),
            "InvoiceGenerated" => (
                "Invoice Available",
                WithOptionalValue("A new invoice has been generated for your service.", "Invoice", data, "invoiceNumber"),
                "Invoice"),
            "PaymentRecorded" => (
                "Payment Recorded",
                WithOptionalValue("Your payment has been recorded successfully.", "Invoice", data, "invoiceNumber"),
                "Payment"),
            _ => throw new JsonException($"Unsupported event type: {eventType}")
        };

    private static string WithOptionalValue(string message, string label, JsonElement data, string propertyName)
    {
        if (!data.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
            return message;

        var text = value.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(text) ? message : $"{message} {label}: {text}.";
    }

    private static Guid ReadGuid(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            !Guid.TryParse(property.GetString(), out var value) || value == Guid.Empty)
            throw new JsonException($"Kafka event {propertyName} is missing or invalid.");
        return value;
    }

    private static string ReadRequiredString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
            throw new JsonException($"Kafka event {propertyName} is missing.");
        return property.GetString()!;
    }

    private static int? ReadOptionalPositiveInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
            return null;
        if (property.ValueKind != JsonValueKind.Number || !property.TryGetInt32(out var value) || value <= 0)
            throw new JsonException($"Kafka event {propertyName} is invalid.");
        return value;
    }
}
