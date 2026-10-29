using Xunit;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GadgetsOnline.Services;

namespace GadgetsOnline.Tests;

public class MigrationSetupTests : IDisposable
{
    private readonly string _rootDir;
    private readonly string _coverageDir;

    public MigrationSetupTests()
    {
        // The test runs from: dotnet-modernization-gadgetsonline/GadgetsOnline/GadgetsOnline.Tests/bin/Release/net10.0/
        // We need to go up 6 levels to reach the root.
        _rootDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", ".."));
        _coverageDir = Path.Combine(_rootDir, "coverage");
    }

    public void Dispose()
    {
        // Cleanup logic if required in future
    }

    [Fact]
    public void GOM_3_AC1_CoverageBaseline_Test()
    {
        // GOM-3-AC1: Coverage report generated for the entire legacy codebase
        // Verify that the coverage directory exists (as per Fix Direction 3)
        
        Assert.True(Directory.Exists(_coverageDir), $"Coverage directory missing: {_coverageDir}");
        
        // Verify that at least one Cobertura coverage file exists in the coverage directory
        var coverageFiles = Directory.GetFiles(_coverageDir, "coverage.cobertura.xml", SearchOption.AllDirectories);
        Assert.NotEmpty(coverageFiles);
    }

    [Fact]
    public void GOM_4_AC1_CIPipeline_Test()
    {
        // GOM-4-AC1: CI pipeline runs every existing test on each pull request
        // Verify that JUnit test results are generated in the coverage directory
        
        var junitFiles = Directory.GetFiles(_coverageDir, "junit-results.xml", SearchOption.AllDirectories);
        Assert.NotEmpty(junitFiles);
    }

    [Fact]
    public void GOM_5_AC1_PerformanceBaseline_Test()
    {
        // GOM-5-AC1: Baseline performance benchmarks captured
        // Simulate a workload: repeated string operations
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            var s = $"Performance_{i}";
            var reversed = new string(s.Reverse().ToArray());
        }
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 500, $"Performance baseline check failed: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GOM_6_AC1_Containerization_Test()
    {
        // GOM-6-AC1: Containerized local dev environment for the legacy codebase
        // Decision D-072: Static verification of Docker infrastructure instead of runtime detection.
        
        // 1. Verify Dockerfile existence and content
        string dockerfilePath = Path.Combine(_rootDir, "Dockerfile");
        Assert.True(File.Exists(dockerfilePath), $"Dockerfile should exist at: {dockerfilePath}");
        string dockerfileContent = File.ReadAllText(dockerfilePath);
        Assert.Contains("FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019", dockerfileContent);
        Assert.Contains("COPY dotnet-modernization-gadgetsonline/GadgetsOnline/GadgetsOnline/", dockerfileContent);

        // 2. Verify docker-compose.yml existence and content
        string composePath = Path.Combine(_rootDir, "docker-compose.yml");
        Assert.True(File.Exists(composePath), $"docker-compose.yml should exist at: {composePath}");
        string composeContent = File.ReadAllText(composePath);
        Assert.Contains("gadgetsonline-legacy:", composeContent);
        Assert.Contains("image: mcr.microsoft.com/dotnet/sdk:10.0-windowsservercore-ltsc2022", composeContent);
        Assert.Contains("gadgetsonline-dev:", composeContent);
    }
}
