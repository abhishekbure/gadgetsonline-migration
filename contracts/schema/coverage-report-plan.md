# Coverage Report Schema Plan

## Objective
Define the schema for the automated coverage reports generated for the legacy .NET 4.8 codebase.

## Schema Definition

### Metadata
- `timestamp`: ISO 8601 string
- `source_commit`: Git SHA
- `tool_version`: Version of the coverage tool used (e.g., OpenCover/dotCover)

### Summary Metrics
- `line_coverage`: Percentage (0.0 - 100.0)
- `branch_coverage`: Percentage (0.0 - 100.0)
- `total_lines`: Integer
- `covered_lines`: Integer

### Detailed Breakdown
- `assemblies`: List of assembly objects containing:
    - `name`: Assembly name
    - `coverage`: Percentage
    - `files`: List of file objects containing:
        - `path`: Relative file path
        - `coverage`: Percentage
