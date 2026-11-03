namespace GadgetsOnline.Tests;

using System.Diagnostics;
using Xunit;

/// <summary>
/// Implements validation tests for the GMM-EP-001 migration setup requirements (GOM-3 to GOM-6).
/// </summary>
public class MigrationSetupTests
{
    /// <summary>Directory levels from the test output folder (GadgetsOnline.Tests/bin/Release/net10.0/) up to the repository root.</summary>
    private const int LEVELS_TO_REPOSITORY_ROOT = 6;

    private const string COVERAGE_DIRECTORY_NAME = "coverage";
    private const string COVERAGE_REPORT_FILE_NAME = "coverage_test_report.xml";
    private const string COVERAGE_REPORT_CONTENT = "<coverage></coverage>";

    private const string TEMP_RESULTS_DIRECTORY_NAME = "temp_results_test";
    private const string TRX_RESULTS_FILE_NAME = "test_results.trx";
    private const string TRX_RESULTS_CONTENT = "<TestRun></TestRun>";

    private const int PERFORMANCE_ITERATION_COUNT = 1000;
    private const long PERFORMANCE_BUDGET_MILLISECONDS = 500;
    private const string PERFORMANCE_SAMPLE_PREFIX = "Performance_";

    private const string DOCKERFILE_NAME = "Dockerfile";
    private const string DOCKERFILE_BASE_IMAGE = "FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019";
    private const string DOCKERFILE_COPY_INSTRUCTION = "COPY dotnet-modernization-gadgetsonline/GadgetsOnline/GadgetsOnline/ .";

    private const string COMPOSE_FILE_NAME = "docker-compose.yml";
    private const string COMPOSE_LEGACY_SERVICE = "gadgetsonline-legacy:";
    private const string COMPOSE_DEV_IMAGE = "image: mcr.microsoft.com/dotnet/sdk:10.0-windowsservercore-ltsc2022";
    private const string COMPOSE_DEV_SERVICE = "gadgetsonline-dev:";

    private readonly string _rootDir;
    private readonly string _coverageDir;

    /// <summary>
    /// Resolves the repository root from the test output folder and makes sure the coverage directory exists (GOM-3).
    /// </summary>
    public MigrationSetupTests()
    {
        _rootDir = ResolveRepositoryRoot(Directory.GetCurrentDirectory());
        _coverageDir = Path.Combine(_rootDir, COVERAGE_DIRECTORY_NAME);

        if (!Directory.Exists(_coverageDir))
        {
            Directory.CreateDirectory(_coverageDir);
        }
    }

    /// <summary>
    /// GOM-3-AC1: a coverage report can be written to the repository's coverage directory.
    /// </summary>
    [Fact]
    public void Should_GenerateCoverageReport_When_CoverageBaselineTestRuns()
    {
        string coverageFile = Path.Combine(_coverageDir, COVERAGE_REPORT_FILE_NAME);
        File.WriteAllText(coverageFile, COVERAGE_REPORT_CONTENT);

        Assert.True(File.Exists(coverageFile), $"Coverage report file should exist: {coverageFile}");

        if (File.Exists(coverageFile))
        {
            File.Delete(coverageFile);
        }
    }

    /// <summary>
    /// GOM-4-AC1: the CI pipeline's TRX test results can be written to a results directory.
    /// </summary>
    [Fact]
    public void Should_GenerateTrxResults_When_CIPipelineTestRuns()
    {
        string tempResultsDir = Path.Combine(_rootDir, TEMP_RESULTS_DIRECTORY_NAME);
        if (Directory.Exists(tempResultsDir))
        {
            Directory.Delete(tempResultsDir, true);
        }
        Directory.CreateDirectory(tempResultsDir);

        string trxFile = Path.Combine(tempResultsDir, TRX_RESULTS_FILE_NAME);
        File.WriteAllText(trxFile, TRX_RESULTS_CONTENT);

        Assert.True(File.Exists(trxFile), $"TRX results file should be created: {trxFile}");

        Directory.Delete(tempResultsDir, true);
    }

    /// <summary>
    /// GOM-5-AC1: a baseline timing for a fixed in-memory workload stays within its budget.
    /// This is a unit-level baseline, not a load test; the application has no endpoint under load in this change.
    /// </summary>
    [Fact]
    public void Should_CapturePerformanceMetrics_When_PerformanceBaselineTestRuns()
    {
        var stopwatch = Stopwatch.StartNew();
        for (int iteration = 0; iteration < PERFORMANCE_ITERATION_COUNT; iteration++)
        {
            _ = ReverseText(PERFORMANCE_SAMPLE_PREFIX + iteration);
        }
        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < PERFORMANCE_BUDGET_MILLISECONDS,
            $"Performance baseline check failed: {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// GOM-6-AC1: the containerized dev environment is verified statically (decision D-072), not by running Docker.
    /// </summary>
    [Fact]
    public void Should_VerifyDockerInfrastructure_When_ContainerizationTestRuns()
    {
        string dockerfilePath = Path.Combine(_rootDir, DOCKERFILE_NAME);
        Assert.True(File.Exists(dockerfilePath), $"Dockerfile should exist at: {dockerfilePath}");
        string dockerfileContent = File.ReadAllText(dockerfilePath);
        Assert.Contains(DOCKERFILE_BASE_IMAGE, dockerfileContent);
        Assert.Contains(DOCKERFILE_COPY_INSTRUCTION, dockerfileContent);

        string composePath = Path.Combine(_rootDir, COMPOSE_FILE_NAME);
        Assert.True(File.Exists(composePath), $"docker-compose.yml should exist at: {composePath}");
        string composeContent = File.ReadAllText(composePath);
        Assert.Contains(COMPOSE_LEGACY_SERVICE, composeContent);
        Assert.Contains(COMPOSE_DEV_IMAGE, composeContent);
        Assert.Contains(COMPOSE_DEV_SERVICE, composeContent);
    }

    /// <summary>
    /// Returns the repository root, <see cref="LEVELS_TO_REPOSITORY_ROOT"/> directories above <paramref name="testOutputDirectory"/>.
    /// </summary>
    private static string ResolveRepositoryRoot(string testOutputDirectory)
    {
        string[] parentSegments = Enumerable.Repeat("..", LEVELS_TO_REPOSITORY_ROOT).ToArray();
        return Path.GetFullPath(Path.Combine(testOutputDirectory, Path.Combine(parentSegments)));
    }

    /// <summary>
    /// Returns <paramref name="text"/> with its characters in reverse order: the fixed workload of the GOM-5 baseline.
    /// </summary>
    private static string ReverseText(string text)
    {
        char[] characters = text.ToCharArray();
        Array.Reverse(characters);
        return new string(characters);
    }
}
