# Baseline Performance Benchmark Script for GadgetsOnline Legacy

$baseUrl = "http://localhost:8080/"
$iterations = 50
$resultsFile = "benchmark_results.csv"

Write-Host "Starting baseline performance benchmarks against $baseUrl"
Write-Host "Running $iterations iterations..."

$results = @()

for ($i = 1; $i -le $iterations; $i++) {
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $response = Invoke-WebRequest -Uri $baseUrl -UseBasicParsing -Method Get
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
        $results += [PSCustomObject]@{
            Iteration = $i
            Timestamp = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
            StatusCode = 0
            ResponseTimeMs = [Math]::Round($stopwatch.Elapsed.TotalMilliseconds, 2)
            Success = $false
        }
        Write-Error "Iteration $i failed: $_"
    }
}

# Calculate averages
$avgTime = ($results | Where-Object { $_.Success -eq $true } | Measure-Object -Property ResponseTimeMs -Average).Average
$successRate = ($results | Where-Object { $_.Success -eq $true } | Measure-Object).Count / $iterations * 100

Write-Host "`n--- Benchmark Summary ---"
Write-Host "Total Iterations: $iterations"
Write-Host "Success Rate: $successRate%"
if ($null -ne $avgTime) {
    Write-Host "Average Response Time: $($avgTime)ms"
} else {
    Write-Host "Average Response Time: N/A (No successful requests)"
}

# Export to CSV
$results | Export-Csv -Path $resultsFile -NoTypeInformation
Write-Host "`nDetailed results exported to $resultsFile"
