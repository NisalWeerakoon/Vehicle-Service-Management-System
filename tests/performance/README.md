
# SE3112 - Load & Performance Testing

## Individual Contribution

Testing Area: Load & Performance Testing

Tool: Apache JMeter 5.6.3

Target: VSMS Billing Microservice

Endpoint: GET /api/invoices/me

Environment: Local ASP.NET Core (.NET 10) with Docker MySQL 8.0

## Objective

Evaluate the response time, throughput, latency percentiles,
and reliability of the Billing Service under simulated
concurrent user requests.

## Test Environment

- Apache JMeter 5.6.3
- .NET SDK 10
- Docker Desktop
- MySQL 8.0
- Windows PowerShell
- Locally generated development JWT

All load tests run against localhost, not production Azure.

## Test Scenarios

### Scenario 1: Baseline Invoice Test

- Virtual users: 10
- Ramp-up: 10 seconds
- Loops: 5
- Total requests: 50

Results:
- Average: 20.38 ms
- P95: 26.90 ms
- Throughput: 5.59 requests/sec
- Error rate: 0%

### Scenario 2: Sustained Invoice Load Test

- Virtual users: 10
- Ramp-up: 2 seconds
- Loops: 20
- Think time: 500 ms
- Total requests: 200

Results:
- Average: 15.84 ms
- P95: 24.95 ms
- P99: 64.82 ms
- Throughput: 17.11 requests/sec
- Error rate: 0%
- APDEX: 1.000

## Setup and Execution

1. Install Java, JMeter, .NET 10, and Docker.
2. Create the local database credentials in .env.
3. Start the Docker MySQL container using compose.yml.
4. Configure Billing to use the local MySQL database.
5. Disable Kafka consumers in local performance-testing mode.
6. Configure a development-only JWT signing key.
7. Start Billing locally at http://127.0.0.1:5180.
8. Generate a local test JWT using refresh-local-token.ps1.
9. Run run-performance.ps1 with the JMeter executable path.
10. Open the generated HTML report in the reports folder.

## Automation

The PowerShell script checks API availability and
authentication, runs JMeter in non-GUI mode, and generates
a JTL results file and HTML performance dashboard.

The group CI pipeline must provision its own isolated
test environment because GitHub-hosted runners cannot
reach a developer's localhost service.

## Quality Indicators

- Average latency: Mean response time
- P95: Response time within which approximately 95%
  of requests complete
- Throughput: Requests processed per second
- Error rate: Percentage of unsuccessful requests
- APDEX: User satisfaction indicator based on configured
  response-time thresholds

## Findings

The Billing API completed both measured local test
scenarios with zero request errors.

The tests used an isolated development database and
a locally generated test token.

Results reflect the tested local workload. They do not
establish production capacity or performance against
a fully populated production-scale database.

## Security

Never commit:
- .env
- local-jwt.env
- token.csv
- Production credentials or tokens

Load testing must only be performed against authorized
local or dedicated testing environments.
