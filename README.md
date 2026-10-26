# Vehicle Service Management System (VSMS)

Enterprise microservices-based Vehicle Service Management System developed for SE3022 / SE3112.

---

## Testing Contributions Matrix (SE3112)

The project incorporates four distinct testing disciplines across the microservice architecture, managed under a unified GitHub Actions pipeline:

| # | Testing Area | Scope & Target Service | Tools Used | Lead / Branch | Documentation & Evidence |
|---|---|---|---|---|---|
| **1** | **Unit Testing & Mocking** | `JobMaintenanceService` mechanic assignment & status logic | xUnit, Moq/Stubs, In-Memory EF Core | `ase/unit-mocking` | [docs/ASE_UNIT_MOCKING.md](docs/ASE_UNIT_MOCKING.md) |
| **2** | **Load & Performance Testing** | `BillingService` `/api/invoices/me` endpoint under load | Apache JMeter 5.6.3, Docker MySQL | `ase/performance` | [tests/performance/README.md](tests/performance/README.md) |
| **3** | **UI / E2E Testing** | Frontend user journey, login, and vehicle service workflow | Playwright, Chromium, Node.js | `ase/e2e-testing` | [.github/workflows/playwright.yml](.github/workflows/playwright.yml) |
| **4** | **Code Quality & Metrics** | Backend services and React 19 frontend code quality | Coverlet, Cobertura, ReportGenerator, ESLint | `feature/quality-metrics` | [docs/ASE_CODE_QUALITY_METRICS.md](docs/ASE_CODE_QUALITY_METRICS.md) |

---

## Quick Start & Verification Commands

### 1. Code Quality & Metrics (Role 4)
Run automated unit tests, collect Cobertura code coverage, execute ESLint static quality checks, and generate the interactive quality report:
```powershell
.\tests\quality-metrics\run-quality-checks.ps1
```
*(On Linux/macOS, execute `./tests/quality-metrics/run-quality-checks.sh`)*

- **Interactive Report:** Open `TestResults/CoverageReport/index.html`
- **Metrics Evaluated:** Line Coverage, Branch Coverage, Cyclomatic Complexity, Static Analysis Diagnostics

### 2. Unit Testing & Mocking (Role 1)
Run parameterized unit tests isolating mechanic assignment:
```powershell
dotnet test tests/JobMaintenanceService.Tests/JobMaintenanceService.Tests.csproj --filter FullyQualifiedName~MechanicAssignmentServiceTests
```

### 3. Load & Performance Testing (Role 2)
Run automated JMeter load tests against Billing Service:
```powershell
.\tests\performance\run-performance.ps1 -JMeter "C:\path\to\jmeter.bat"
```

### 4. End-to-End Testing (Role 3)
Run Playwright end-to-end browser tests:
```powershell
npx playwright test
```

---

## Continuous Integration (CI/CD)

The project uses a unified GitHub Actions automation pipeline defined in `.github/workflows/ci.yml`:
- **Backend Build & Test (.NET 10):** Restores, builds, executes xUnit tests with Coverlet coverage collection, generates HTML & Markdown reports via `ReportGenerator`, publishes a rich step summary to `$GITHUB_STEP_SUMMARY`, and uploads `code-coverage-report` as a 30-day artifact.
- **Frontend Build & Static Lint (React 19 & Vite):** Runs ESLint static analysis to verify React hook safety before bundling the production frontend.
- **Performance Testing:** Runs containerized JMeter execution against Billing Service with automated HTML dashboard generation and artifact retention.
- **E2E Playwright Suite:** Executed via `.github/workflows/playwright.yml` with Playwright HTML report retention.
