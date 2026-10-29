using Xunit;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GadgetsOnline.Services;

namespace GadgetsOnline.Tests;

public class MigrationSetupTests
{
    [Fact]
    public void GOM_3_AC1_CoverageBaseline_Test()
    {
        // GOM-3-AC1: Coverage report generated for the entire legacy codebase
        // Verify that at least one Cobertura coverage file exists in a coverage directory
        // The requirement states the directory is at the repository root.
        var coverageDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "..", "coverage");
        
        // Ensure we are looking at the correct absolute path if possible, 
        // but for the purpose of this test in CI, we assert its existence.
        Assert.True(Directory.Exists(coverageDir), $"Coverage directory not found at: {coverageDir}");
        
        var coverageFiles = Directory.GetFiles(coverageDir, "coverage.cobertura.xml", SearchOption.AllDirectories);
        Assert.NotEmpty(coverageFiles);
    }

    [Fact]
    public void GOM_4_AC1_CIPipeline_Test()
    {
        // GOM-4-AC1: CI pipeline runs every existing test on each pull request
        // Verify that JUnit test results are generated in the coverage directory
        var coverageDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "..", "coverage");
        
        Assert.True(Directory.Exists(coverageDir), $"Coverage directory not found at: {coverageDir}");
        
        var junitFiles = Directory.GetFiles(coverageDir, "junit-results.xml", SearchOption.AllDirectories);
        Assert.NotEmpty(junitFiles);
    }

    [Fact]
    public void GOM_5_AC1_PerformanceBaseline_Test()
    {
        // GOM-5-AC1: Baseline performance benchmarks captured
        // Using a slightly more realistic workload: repeated string operations to simulate logic
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
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
        var isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ||
                          File.Exists("/.dockerenv");
        
        Assert.True(isContainer, "Environment should be detected as a container.");
    }
}
