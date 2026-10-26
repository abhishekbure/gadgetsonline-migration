# Coverage Report Plan

## Objective
Establish a baseline for code coverage across the legacy .NET Framework 4.8 codebase.

## Metrics to Capture
- **Line Coverage**: Percentage of executable lines covered by tests.
- **Branch Coverage**: Percentage of control flow branches covered.
- **Method Coverage**: Percentage of methods executed during testing.

## Toolchain
- **Test Runner**: NUnit / MSTest (existing)
- **Coverage Engine**: dotCover / OpenCover
- **Report Format**: Cobertura / LCOV

## Frequency
- Every CI pipeline run on Pull Requests.
- Weekly full repository scan.

## Storage and Visualization
- Coverage artifacts will be stored in the CI pipeline artifacts.
- Reports will be surfaced via the CI Status API.
