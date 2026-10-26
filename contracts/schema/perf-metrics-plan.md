# Performance Metrics Schema Plan

## Objective
Define the schema for baseline performance benchmarks captured during the migration setup.

## Schema Definition

### Metadata
- `timestamp`: ISO 8601 string
- `environment`: Description of the test environment (e.g., "Docker container Windows Server Core")
- `baseline_id`: Unique identifier for the benchmark run

### Metrics
- `latency_ms`: Average latency in milliseconds
- `p95_latency_ms`: 95th percentile latency in milliseconds
- `p99_latency_ms`: 99th percentile latency in milliseconds
- `throughput_rps`: Requests per second
- `cpu_usage_percent`: Average CPU utilization during test
- `memory_usage_mb`: Average memory consumption in MB
