namespace Consilient.Infrastructure.ExcelImporter.Tests;

public static class TestFileHelper
{
    /// <summary>
    /// Returns the full filesystem path for a test file given a project-relative path
    /// (e.g. "Files\DoctorAssignment_SAMPLE.xlsm").
    /// If a <paramref name="testContext"/> is provided the TestRunDirectory will be used as the base path; otherwise Directory.GetCurrentDirectory() is used.
    /// The method will try several likely base directories and return the first existing file path.
    /// </summary>
    public static string GetTestFilePath(string relativePath, TestContext? testContext = null)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("relativePath must be a non-empty relative path to the test file.", nameof(relativePath));
        }

        // candidate base directories in order of likelihood
        var candidates = new[]
        {
            GetTestRunDirectory(testContext),
            AppContext.BaseDirectory ?? string.Empty,
            Path.GetDirectoryName(typeof(TestFileHelper).Assembly.Location) ?? string.Empty,
            Directory.GetCurrentDirectory()
        };

        var attempted = new System.Text.StringBuilder();
        foreach (var baseDir in candidates)
        {
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                continue;
            }

            var combined = Path.Combine(baseDir, relativePath);
            var full = Path.GetFullPath(combined);
            attempted.AppendLine(full);

            if (File.Exists(full))
            {
                return full;
            }
        }

        // None found — throw a clear, actionable exception
        var message =
            $"Test file '{relativePath}' was not found. Attempted the following locations:{Environment.NewLine}{attempted}{Environment.NewLine}" +
            "Ensure the file is included in the test project and set to be copied to the test output (in Visual Studio set __Copy to Output Directory__ = __Copy if newer__ / __Copy always__), " +
            "or use [DeploymentItem] / project file <CopyToOutputDirectory> settings.";
        throw new FileNotFoundException(message);
    }

    /// <summary>
    /// Returns the test run directory (preferred) or current directory as a fallback.
    /// </summary>
    public static string GetTestRunDirectory(TestContext? testContext = null)
    {
        if (!string.IsNullOrWhiteSpace(testContext?.TestRunDirectory))
        {
            return testContext!.TestRunDirectory!;
        }

        return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Returns a directory appropriate for test output files.
    /// By default this returns the parent of the TestRunDirectory (matches previous test behavior). If parent is not available, returns TestRunDirectory.
    /// Ensures the directory exists.
    /// </summary>
    public static string GetOutputDirectory(TestContext? testContext = null)
    {
        var runDir = GetTestRunDirectory(testContext);
        var parent = Directory.GetParent(runDir);
        var outputDir = parent?.FullName ?? runDir;

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        return outputDir;
    }

    /// <summary>
    /// Builds a unique output file path for an input test file.
    /// Example: input "Files\DoctorAssignment_SAMPLE.xlsm" -> "[outputDir]\DoctorAssignment_SAMPLE_{Guid}_suffix.csv"
    /// </summary>
    public static string CreateOutputFilePathFromInput(string inputFilePath, TestContext? testContext = null, string? suffix = "output", string extension = ".csv")
    {
        if (string.IsNullOrWhiteSpace(inputFilePath))
        {
            throw new ArgumentException("inputFilePath must be provided.", nameof(inputFilePath));
        }

        var outputDir = GetOutputDirectory(testContext);

        var baseName = Path.GetFileNameWithoutExtension(inputFilePath);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "output";
        }

        var ext = string.IsNullOrWhiteSpace(extension) ? string.Empty : (extension.StartsWith('.') ? extension : "." + extension);
        var guid = Guid.NewGuid().ToString("N");

        var fileName = string.IsNullOrWhiteSpace(suffix)
            ? $"{baseName}_{guid}{ext}"
            : $"{baseName}_{guid}_{suffix}{ext}";

        return Path.Combine(outputDir, fileName);
    }
}