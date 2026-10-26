# SE3112 individual contribution: Code Quality and Metrics

## Scope

This contribution establishes automated code quality, test coverage, and static analysis verification for the Vehicle Service Management System. It spans both the backend .NET microservices (`JobMaintenanceService`) and the React 19 frontend application.

The goal is to move beyond manual inspection by enforcing repeatable, automated metric collection:
- **Code Coverage:** Statement (line) and decision (branch) coverage using Coverlet and Cobertura XML format.
- **Maintainability & Complexity:** Cyclomatic Complexity analysis based on McCabe's metric to detect high-risk methods.
- **Static Quality Checks:** Roslyn compiler diagnostics and React ESLint rules to eliminate memory leaks, cascading re-renders, and unused variables.
- **CI/CD Pipeline Integration:** Automated execution on every Pull Request with published step summaries and downloadable report artifacts.

---

## Run the checks

### One-Command Runner (PowerShell)
From the repository root:

```powershell
.\tests\quality-metrics\run-quality-checks.ps1
```

To run without launching the browser automatically:
```powershell
.\tests\quality-metrics\run-quality-checks.ps1 -NoBrowser
```

### Direct CLI Commands

To run backend test coverage directly with the .NET 10 SDK:
```powershell
dotnet test tests/JobMaintenanceService.Tests/JobMaintenanceService.Tests.csproj --no-restore --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

To run frontend static quality linting:
```powershell
cd frontend
npm run lint
```

---

## Metrics to explain in the two-minute demo

| Metric | Measured Value | What it proves | Why it matters |
|---|---|---|---|
| **Line Coverage** | **15.28% Overall**<br>*(98–100% on tested service domains)* | Proves which lines of executable code were exercised by unit tests. `MechanicAssignmentService` and `ServiceHistoryService` achieve **100%** line coverage. | Identifies untested code paths and dead code. Overall percentage reflects that other microservices (Billing, Inventory) have separate test strategies. |
| **Branch Coverage** | **13.35% Overall**<br>*(100% on mechanic assignment logic)* | Proves whether every conditional branch (`if`, `else`, `switch`, ternary) was traversed in both true and false directions. | High line coverage can mask untested edge cases. 100% branch coverage on assignment validates both valid inputs and negative guard clauses (inactive staff, invalid roles). |
| **Cyclomatic Complexity** | **6.8 Average**<br>*(Peak: 16 in ServiceHistory)* | Measures the number of linearly independent paths through methods ($M = E - N + 2P$). | Quantifies maintainability and risk. Methods with complexity under 10 are easy to maintain and test; higher complexity highlights candidates for refactoring. |
| **Static Code Quality** | **0 Errors, Clean CI Pass** | Proves code adheres to architectural, safety, and formatting standards before deployment. | Enforces clean React hook usage, prevents state updates inside effect bodies (avoiding infinite render loops), and catches unused variables. |

---

## Where reports appear

1. **Local Interactive HTML Dashboard:**
   - Generated at `TestResults/CoverageReport/index.html`.
   - Displays real-time metric cards, color-coded badges, and an expandable table of all tested classes and methods.
2. **Local Coverage Data:**
   - Raw output saved at `TestResults/**/coverage.cobertura.xml` for IDE and external tool integration.
3. **GitHub Actions CI/CD Pipeline:**
   - **Step Summary (`$GITHUB_STEP_SUMMARY`):** Directly viewable in the GitHub Actions summary tab of each PR and push run.
   - **Downloadable Artifact (`code-coverage-report`):** Zip containing the interactive HTML report, retained for 30 days.

---

## Live walkthrough (about two minutes)

1. **Execute the local runner (0:00 - 0:30):**
   - Open PowerShell in the project root.
   - Run `.\tests\quality-metrics\run-quality-checks.ps1`.
   - Point out that tests pass, Cobertura coverage is collected, frontend ESLint executes with 0 errors, and the summary table prints cleanly in the terminal.

2. **Demonstrate the interactive HTML dashboard (0:30 - 1:15):**
   - Show the browser dashboard that opened automatically (`TestResults/CoverageReport/index.html`).
   - Explain the four metric cards:
     - **Line Coverage (15.28% overall / 98-100% on tested services):** Explain why core services like `MechanicAssignmentService` and `ServiceHistoryService` have 100% coverage while untouched services await future test additions.
     - **Branch Coverage (100% on critical assignment logic):** Explain that both happy-path and rejection branches (e.g. wrong staff role, non-existent job) are verified.
     - **Cyclomatic Complexity (Avg 6.8):** Explain that low average complexity ensures high testability and maintainability.

3. **Explain Static Quality & Linting (1:15 - 1:40):**
   - Mention the frontend ESLint integration (`npm run lint`), which ensures React 19 hook safety and prevents render cascades.
   - Mention Roslyn analyzer compiler warnings that prevent dead code and unused variables (`CS0168`).

4. **Show GitHub Actions CI Integration (1:40 - 2:00):**
   - Open `.github/workflows/ci.yml` and show the `ReportGenerator` step, `$GITHUB_STEP_SUMMARY` output, and the `code-coverage-report` artifact upload.
   - Conclude: *"Quality metrics are not just a one-off report; they are permanently automated into our team's CI pipeline on every PR."*

---

For the group README, list this work under **Code Quality & Metrics** and link to this document. The group pipeline automatically executes these checks on every commit to `develop`, `main`, and `production`.
