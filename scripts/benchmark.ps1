# Baseline Performance Benchmark Script for GadgetsOnline Legacy
# Requirement: GOM-5-AC1

$baseUrl = "http://localhost:8080/"
$iterations = 50
$resultsFile = "benchmark_results.csv"

Write-Host "Starting baseline performance benchmarks against $baseUrl"
Write-Host "Running $iterations iterations..."

$results = @()

for ($i = 1; $i -le $iterations; $i++) {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        # Using Invoke-WebRequest with -UseBasicParsing for compatibility in PowerShell environments
        $response = Invoke-WebRequest -Uri $baseUrl -UseBasicParsing -Method Get -TimeoutSec 5
        $stopwatch.Stop()
        
        $elapsedMs = $stopwatch.Elapsed.TotalMilliseconds
        $statusCode = [int]$response.StatusCode
        
        $results += [PSCustomObject]@{
            Iteration = $i
            Timestamp = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
            StatusCode = $statusCode
            ResponseTimeMs = [Math]::Round($elapsedMs, 2)
            Success = $true
        }
        Write-Host "Iteration $i: Status $statusCode, Time $($elapsedMs)ms"
    }
    catch {
        $stopwatch.Stop()
        $statusCode = 0
        $elapsedMs = $stopwatch.Elapsed.TotalMilliseconds
        $results += [PSCustomObject]@{
            Iteration = $i
            Timestamp = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
            StatusCode = $statusCode
            ResponseTimeMs = [Math]::Round($elapsedMs, 2)
            Success = $false
        }
        Write-Error "Iteration $i failed: $_"
    }
}

# Calculate statistics
$successfulRequests = $results | Where-Object { $_.Success -eq $true }
$count = $successfulRequests.Count

Write-Host "`n--- Benchmark Summary ---"
Write-Host "Total Iterations: $iterations"
Write-Host "Successful Iterations: $count"

if ($count -gt 0) {
    $avgTime = ($successfulRequests | Measure-Object -Property ResponseTimeMs -Average).Average
    $successRate = ($count / $iterations) * 100
    Write-Host "Success Rate: $successRate%"
    Write-Host "Average Response Time: $($avgTime)ms"
} else {
    Write-Host "Success Rate: 0%"
    Write-Host "Average Response Time: N/A"
}

# Export results to CSV for auditability (GOM-5)
$results | Export-Csv -Path $resultsFile -NoTypeInformation
Write-Host "`nDetailed results exported to $resultsFile"
