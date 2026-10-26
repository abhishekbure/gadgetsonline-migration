# SDD-GOM-EP-001 - Migration Setup Infrastructure

## Overview
This document defines the technical design for the modernization readiness infrastructure required to support the migration of GadgetsOnline from .NET Framework 4.8 to .NET 10. This infrastructure provides the quality baselines (coverage, performance, functional parity) and the development environment necessary for all subsequent migration work.

## Components

### 1. CI Pipeline (GOM-4)
**Description:** An automated pipeline that executes on every pull request to ensure no regression in the legacy codebase and to provide continuous feedback.
**Components:**
- **CI Runner:** GitHub Actions or equivalent (platform dependent, to be configured).
- **Test Runner:** Executes all existing NUnit/MSTest/XUnit tests within the legacy solution.
- **Coverage Engine:** Integrates with the test runner to extract code coverage metrics.
- **Artifact Store:** Stores test results, coverage reports, and benchmark data.

### 2. Coverage Reporting (GOM-3)
**Description:** Generates a comprehensive view of the code coverage for the legacy .NET 4.8 application.
**Components:**
- **Coverage Tool:** dotCover or ReportGenerator to process coverage data.
- **Report Format:** HTML/LCOV for human review; XML/JSON for machine consumption by the CI pipeline.

### 3. Performance Benchmarking (GOM-5)
**Description:** Establishes a baseline of current application performance to compare against the .NET 10 implementation.
**Components:**
- **Benchmarking Tool:** BenchmarkDotNet or custom integration tests with timing captures.
- **Metrics Captured:** Response latency (p50, p95, p99), throughput (RPS), and memory allocation.
- **Baseline Storage:** A versioned record of performance metrics tied to specific commits.

### 4. Containerized Dev Environment (GOM-6)
**Description:** Provides a reproducible local development environment that mirrors the CI environment.
**Components:**
- **Docker Compose:** Orchestrates the legacy application, its database (SQL Server), and any necessary dependencies.
- **Base Image:** A Windows Container image compatible with .NET Framework 4.8.
- **Data Seeding:** Automated scripts to populate the local database with a subset of realistic data.

### 5. Parity Test Harness (Scope)
**Description:** A mechanism to run side-by-side comparisons between the legacy and modernized versions of specific functional modules.
**Components:**
- **Request Proxy/Interceptor:** Routes identical requests to both environments.
- **Comparator:** Evaluates the responses (headers, body, status code) for functional equivalence.

## Data Flow
1. **Developer** pushes code $\rightarrow$ **CI Pipeline** triggers.
2. **CI Pipeline** $\rightarrow$ spins up **Containerized Environment**.
3. **Containerized Environment** $\rightarrow$ executes **Existing Tests**.
4. **Test Runner** $\rightarrow$ outputs **Coverage Data** & **Performance Metrics**.
5. **Coverage/Perf Tools** $\rightarrow$ generate **Reports** $\rightarrow$ upload to **Artifact Store**.
6. **CI Pipeline** $\rightarrow$ evaluates results against baselines $\rightarrow$ passes/fails PR.

## Failure Handling
| Scenario | Handling |
| :--- | :--- |
| Test Failure | Pipeline fails; developer notified via PR status. |
| Coverage Drop | Pipeline warns/fails based on threshold (to be defined). |
| Performance Regression | Pipeline flags regression compared to baseline. |
| Container Startup Failure | Pipeline fails; logs captured for debugging. |

## NFR Tactics
### Reliability
- **Tactic:** Containerized isolation. Ensures tests run in a clean, predictable environment every time, reducing flakiness.

### Maintainability
- **Tactic:** Infrastructure as Code (IaC). Dockerfiles and CI configurations are versioned alongside the code.

### Performance Efficiency
- **Tactic:** Baseline capture. Automated capturing of performance metrics at every baseline change to prevent silent degradation.

### Observability
- **Tactic:** Structured logging and artifact reporting. CI pipeline emits structured test/coverage results for historical analysis.

## Operability Design
### CI Pipeline Entrypoint
- **SLIs:** Pipeline Success Rate, Pipeline Duration, Test Coverage %.
- **Log Events:** `pipeline_started`, `test_suite_completed`, `coverage_report_generated`, `benchmark_completed`, `pipeline_failed`.
- **Alerts:** Failure of the main test suite, failure to generate coverage reports, or unexpected increase in pipeline duration.
- **Rollback:** Revert the commit that introduced the pipeline/config change.
