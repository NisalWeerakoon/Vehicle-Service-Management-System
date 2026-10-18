# SE3112 individual contribution: Unit Testing and Mocking

## Scope

This contribution tests mechanic assignment in JobMaintenanceService. It supports
SRS requirements FR-JOB-04 (assign a mechanic) and FR-JOB-05 (view assigned jobs).
The tests focus on service logic, not the HTTP controller or a live deployment.

## Run the tests

From the repository root with the .NET 10 SDK installed:

```powershell
dotnet test tests/JobMaintenanceService.Tests/JobMaintenanceService.Tests.csproj --filter FullyQualifiedName~MechanicAssignmentServiceTests
```

To run all Job Maintenance tests:

```powershell
dotnet test tests/JobMaintenanceService.Tests/JobMaintenanceService.Tests.csproj
```

The tests require no running services, MySQL, or Kafka. They use EF Core's
in-memory provider and a recording `HttpMessageHandler` in place of Customer
Booking's staff-validation endpoint. A stub `IHttpClientFactory` supplies that
client. The recording handler also verifies the requested URL and forwarded
Bearer token. These are test doubles: they give deterministic responses while
isolating Job Maintenance logic from the network and authentication service.

## Cases to explain in the two-minute demo

| Test | What it proves |
| --- | --- |
| Valid mechanic (`[Theory]`) | Role comparison is case-insensitive; a valid assignment is stored and the staff request includes the token. |
| Invalid staff (`[Theory]`) | Wrong role, inactive staff, or a missing staff record cannot replace the current mechanic. |
| Invalid mechanic ID (`[Theory]`) | Malformed and non-positive IDs are rejected without making an HTTP call. |
| Unknown job | A nonexistent job is rejected before contacting Customer Booking. |
| Valid reassignment | The previous assignment becomes inactive; only the new mechanic appears in active-job queries. |

The invalid-staff test exposed a bug: the previous assignment was marked inactive
in the EF change tracker **before** the candidate mechanic was validated. The
service now validates first, then replaces the assignment. This is a small
production fix tied directly to the tested business rule.

## Live walkthrough (about two minutes)

1. Show `MechanicAssignmentServiceTests.cs` and point to the parameterized
   `InlineData` rows and the `RecordingHandler` test double.
2. Explain that the staff API is not contacted in the tests; the handler supplies
   predictable responses and records the request.
3. Run the filtered command above and show the passing summary.
4. Open `MechanicAssignmentService.AssignAsync` and explain why validation now
   happens before the active assignment is deactivated.
5. State the limitation: in-memory EF and stub HTTP verify service logic, but
   do not prove the real cross-service deployment or MySQL behavior. Those need
   separate integration/E2E tests.

For the group README, list this work under **Unit Testing & Mocking** and link
to this document. The group pipeline can run the all-tests command above.
