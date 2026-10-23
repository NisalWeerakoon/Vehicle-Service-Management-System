#!/usr/bin/env bash
set -e

# ==========================================================
# SE3112 CODE QUALITY & METRICS AUTOMATION RUNNER (Bash/Linux)
# ==========================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
TEST_PROJECT="$REPO_ROOT/tests/JobMaintenanceService.Tests/JobMaintenanceService.Tests.csproj"
TEST_RESULTS_DIR="$REPO_ROOT/TestResults"
REPORT_DIR="$TEST_RESULTS_DIR/CoverageReport"
FRONTEND_DIR="$REPO_ROOT/frontend"

echo "=========================================================="
echo "  SE3112 CODE QUALITY & METRICS AUTOMATION RUNNER"
echo "=========================================================="
echo "Repository Root: $REPO_ROOT"
echo ""

# 1. Run Backend Tests with Coverage
echo "[1/3] Executing Backend Unit Tests with Coverlet Coverage..."
dotnet test "$TEST_PROJECT" --no-restore --collect:"XPlat Code Coverage" --results-directory "$TEST_RESULTS_DIR" --verbosity minimal

# 2. Locate Coverage XML
COVERAGE_FILE=$(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" | sort -r | head -n 1)
if [ -z "$COVERAGE_FILE" ]; then
  echo "Error: coverage.cobertura.xml not found."
  exit 1
fi
echo "  Coverage file located: $COVERAGE_FILE"

# 3. Frontend Static Quality Linting
echo ""
echo "[2/3] Running Frontend Static Quality Checks (ESLint)..."
if [ -d "$FRONTEND_DIR" ]; then
  cd "$FRONTEND_DIR"
  npm run lint
  cd "$REPO_ROOT"
  echo "  Frontend ESLint checks completed successfully!"
fi

echo ""
echo "[3/3] Quality checks completed."
echo "=========================================================="
echo "  Coverage File: $COVERAGE_FILE"
echo "=========================================================="
