# Performance Metrics Schema Plan

This document defines the structure and format of the baseline performance metrics captured for the legacy GadgetsOnline application.

## Target Format
Metrics will be captured in JSON format to allow for easy comparison between legacy and modernized versions during the parallel run.

## Schema Definition

### JSON Format (Machine Readable)
```json
{
  "metadata": {
    "timestamp": "ISO-8601-datetime",
    "commit_sha": "git-sha",
    "environment": "legacy-container",
    "framework": ".NET Framework 4.8"
  },
  "benchmarks": [
    {
      "endpoint": "/api/v1/products",
      "method": "GET",
      "metrics": {
        "latency_ms": {
          "p50": "float",
          "p95": "float",
          "p99": "float"
        },
        "throughput_rps": "float",
        "error_rate": "float"
      },
      "resource_usage": {
        "cpu_percent_avg": "float",
        "memory_mb_peak": "float"
      }
    }
  ]
}
```

## Integration Points
- **Parity Test Harness:** Compares these metrics against the .NET 10 implementation metrics.
- **CI Pipeline:** Monitors for significant performance regressions.
