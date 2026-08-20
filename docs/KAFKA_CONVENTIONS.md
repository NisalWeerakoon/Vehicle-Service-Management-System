# Kafka Conventions

This document defines the shared Kafka conventions for the Vehicle Service Center system. The conventions are intentionally simple and practical for Version 1 of the university project.

## 1. Bootstrap Server

For local development, all microservices connect to Kafka using:

```text
localhost:9092
```

## 2. Topic Naming

Kafka topic names use lowercase words and the following format:

```text
vsc.<domain>.<event>
```

Planned topic names are:

| Event | Topic |
| --- | --- |
| BookingCreated | `vsc.booking.created` |
| VehicleCheckedIn | `vsc.vehicle.checked-in` |
| InspectionCompleted | `vsc.inspection.completed` |
| PartRequested | `vsc.parts.requested` |
| PartIssued | `vsc.parts.issued` |
| LowStockDetected | `vsc.inventory.low-stock` |
| ServiceCompleted | `vsc.service.completed` |
| InvoiceGenerated | `vsc.invoice.generated` |
| PaymentRecorded | `vsc.payment.recorded` |
| VehicleReadyForCollection | `vsc.vehicle.ready-for-collection` |

## 3. Event Naming

Domain event names use PascalCase. An event name describes something that has already happened.

The planned event names are:

- `BookingCreated`
- `VehicleCheckedIn`
- `InspectionCompleted`
- `PartRequested`
- `PartIssued`
- `LowStockDetected`
- `ServiceCompleted`
- `InvoiceGenerated`
- `PaymentRecorded`
- `VehicleReadyForCollection`

## 4. Standard Event Envelope

All events should follow the same conceptual envelope:

```json
{
  "eventId": "unique GUID",
  "eventType": "BookingCreated",
  "occurredAt": "UTC timestamp",
  "source": "CustomerBookingService",
  "correlationId": "optional workflow identifier",
  "data": {
    "... event-specific fields ...": "..."
  }
}
```

The `data` object changes for each event type. The other fields provide shared information that consumers can handle consistently.

## 5. Event Rules

- `eventId` must be a unique GUID for each published event.
- `occurredAt` must use a UTC timestamp.
- `eventType` must clearly identify the domain event and match its PascalCase event name.
- `source` must identify the microservice that produced the event.
- `correlationId` is optional and may link events that belong to the same workflow.
- `data` should contain only the information consumers need.
- Passwords, access tokens, connection strings, and other secrets must never be placed in events.

## 6. Consumer Group Naming

Consumer group names use:

```text
<service-name>-group
```

Examples:

- `job-maintenance-group`
- `inventory-group`
- `billing-group`
- `notification-group`

Multiple running instances of the same service must use the same consumer group. Kafka can then distribute messages among those instances so that the service handles each message once under normal operation.

Different services must use different consumer groups. This allows each interested service to receive and independently handle the same event.

## 7. Delivery and Idempotency

Kafka consumers may receive the same message more than once. Each consumer must therefore be idempotent: handling a duplicate event must not apply the same business change twice.

Consumers should use `eventId` to identify events that they have already processed. A consumer should commit or acknowledge a message only after it has handled the event successfully.

## 8. Failure Handling

For Version 1:

- Log consumer errors with enough context to investigate the failure.
- Retry safely where appropriate, without applying the event twice.
- Do not block the producer because another service is temporarily unavailable. The producer publishes the event to Kafka and remains independent of consumer availability.
- A Dead Letter Topic may be added later if the project requires one.

## 9. REST vs Kafka

Use REST when:

- An immediate request and response are required.
- The frontend performs a query or command that requires an immediate result.

Use Kafka when:

- Communicating asynchronous domain events.
- One service needs to inform other independent services that something has happened.

Kafka should not replace REST for every operation. The communication method should match whether the caller needs an immediate result.

## 10. Planned Producer and Consumer Mapping

| Event | Producer | Consumer or consumers |
| --- | --- | --- |
| `BookingCreated` | CustomerBookingService | NotificationService |
| `VehicleCheckedIn` | CustomerBookingService | JobMaintenanceService |
| `PartRequested` | JobMaintenanceService | InventoryService |
| `PartIssued` | InventoryService | JobMaintenanceService, BillingService |
| `LowStockDetected` | InventoryService | NotificationService |
| `ServiceCompleted` | JobMaintenanceService | BillingService, NotificationService |
| `InvoiceGenerated` | BillingService | NotificationService |
| `PaymentRecorded` | BillingService | NotificationService |
| `VehicleReadyForCollection` | JobMaintenanceService | NotificationService |

This mapping describes the current plan. Producers and consumers will be implemented in later work.
