# SE3112 - Code Quality & Metrics

## Individual Contribution

- **Testing Area:** Code Quality & Metrics (Role 4)
- **Tools:** Coverlet (XPlat Code Coverage), Cobertura, ReportGenerator, Roslyn Compiler Analyzers, ESLint (Flat Config)
- **Target Services:**
  - Backend: `JobMaintenanceService` (.NET 10 Web API / Entity Framework Core)
  - Frontend: React 19 + Vite Web Application
- **CI/CD Integration:** GitHub Actions (`.github/workflows/ci.yml`)

---

## Objective

Automate code quality checks across the microservice codebase by establishing:
1. Automated unit test code coverage analysis (Line & Branch coverage).
2. Cyclomatic Complexity metrics to quantify code risk and maintainability.
3. Static code analysis and linting to prevent runtime regressions, cascading renders, and technical debt.
4. Retention and publishing of interactive HTML and markdown reports in CI/CD pipeline and locally.

---

## Metrics Overview & Explanation

| Metric | What it Measures | Target / Achieved | Significance |
|---|---|---|---|
| **Line Coverage** | Percentage of executable C# statements traversed during automated tests. | **15.28% Overall** (4,115 valid lines)<br>**98–100% on tested domains** | Measures how thoroughly the code logic is exercised. Core services (`MechanicAssignmentService`, `ServiceHistoryService`, `JobStatusService`) achieve up to 100% line coverage. |
| **Branch Coverage** | Percentage of conditional decision outcomes (boolean branches) evaluated. | **13.35% Overall** (554 valid branches)<br>**100% on critical path** | Ensures edge cases, negative validation guards, and error branches are actively tested rather than just happy-path lines. |
| **Cyclomatic Complexity** | Number of linearly independent execution paths through methods ($M = E - N + 2P$). | **6.8 Average**<br>**16 Peak** | Identifies high-risk, error-prone methods. A low score (under 10) confirms low coupling, high modularity, and ease of maintainability. |
| **Static Quality (ESLint & Roslyn)** | Syntax rules, unused variables, memory leaks, and React hook render cycles. | **0 Errors, Clean CI Pass** | Detects defects before deployment without executing code, enforcing coding standards automatically. |

---

## Running Quality Checks Locally

### PowerShell (Windows)
From the repository root:
```powershell
.\tests\quality-metrics\run-quality-checks.ps1
```

Options:
- `-NoBrowser`: Prevents opening the HTML report in your browser automatically.
- `-SkipFrontend`: Runs backend tests and coverage only.

### Bash (Linux / macOS)
```bash
chmod +x tests/quality-metrics/run-quality-checks.sh
./tests/quality-metrics/run-quality-checks.sh
```

---

## Where Reports Appear

1. **Local Interactive HTML Dashboard:**
   - Path: `TestResults/CoverageReport/index.html`
   - Features: Summary metrics cards, color-coded progress bars, cyclomatic complexity indicators, and per-class breakdown.
2. **Local Coverage Data:**
   - Path: `TestResults/**/coverage.cobertura.xml` (standard Cobertura format compatible with SonarQube, Azure DevOps, and IDEs).
3. **GitHub Actions CI/CD Pipeline:**
   - **Step Summary:** Automatically rendered Markdown summary table on the GitHub Actions workflow run page (`$GITHUB_STEP_SUMMARY`).
   - **Artifacts:** Downloadable `code-coverage-report` zip containing the full interactive HTML report (retained for 30 days).

---

## Tested Classes Coverage Breakdown

| Class / Component | Line Coverage | Branch Coverage | Cyclomatic Complexity |
|---|---|---|---|
| `MechanicAssignmentService` | **100%** | **100%** | 2 |
| `ServiceHistoryService` | **100%** | **100%** | 2 |
| `JobStatusService` | **97.6%** | **60.0%** | 13 |
| `ServiceHistoryQueryService` | **98.1%** | **50.0%** | 12 |
| `JobMaintenanceDbContext` | **98.0%** | **100%** | 11 |
| `MechanicAssignment` | **100%** | **100%** | 7 |
| `JobCard` | **100%** | **100%** | 11 |
| `ServiceHistory` | **100%** | **100%** | 16 |
| `ServiceHistoryController` | **100%** | **100%** | 1 |
