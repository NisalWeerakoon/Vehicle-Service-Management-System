using System.Collections.Concurrent;
using CustomerBookingService.Models;
using CustomerBookingService.Services;

namespace CustomerBookingService.IntegrationTests.TestHelpers;

/// <summary>
/// Replaces the real Kafka-backed IBookingEventPublisher for integration
/// tests. The real BookingEventPublisher opens a Confluent.Kafka producer
/// and tries to reach a real broker on every publish, which we don't want
/// (or need) to depend on in CI. This fake just records what was published
/// so tests can assert on it if they want to.
/// </summary>
public class FakeBookingEventPublisher : IBookingEventPublisher
{
    public ConcurrentBag<Booking> PublishedBookings { get; } = new();

    public Task PublishBookingCreatedAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        PublishedBookings.Add(booking);
        return Task.CompletedTask;
    }
}
