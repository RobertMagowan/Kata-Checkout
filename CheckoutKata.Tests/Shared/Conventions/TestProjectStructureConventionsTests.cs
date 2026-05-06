using System.Reflection;
using System.Text.RegularExpressions;

namespace CheckoutKata.Tests.Shared.Conventions;

[Category("Conventions")]
public class TestProjectStructureConventionsTests
{
    private static readonly string[] TestTypeRoots = ["UnitTests", "IntegrationTests", "EndToEndTests"];

    private static readonly string[] LayerRoots = ["Core", "Application", "Api", "Console"];

    private static readonly HashSet<string> BehaviorCategories =
    [
        "Pricing",
        "Validation",
        "Lifecycle",
        "Versioning",
        "Expiry",
        "Capacity",
        "Normalization",
        "Conflicts",
        "State",
        "EndToEnd",
        "Parity",
        "Persistence"
    ];

    [Test]
    public void TestClasses_MatchFileNames_AndUseTestsSuffix()
    {
        foreach (var descriptor in DiscoverTestSourceFiles())
        {
            Assert.That(descriptor.FileName, Does.EndWith("Tests"));
            Assert.That(
                descriptor.PublicClassNames,
                Has.Count.EqualTo(1),
                $"Expected one public class in {descriptor.FilePath}.");
            Assert.That(
                descriptor.PublicClassNames.Single(),
                Is.EqualTo(descriptor.FileName),
                $"Class/file mismatch in {descriptor.FilePath}.");
        }
    }

    [Test]
    public void TestFileNamespaces_MatchLayerRootFolders()
    {
        foreach (var descriptor in DiscoverTestSourceFiles())
        {
            var expectedPrefix = $"CheckoutKata.Tests.{descriptor.TestTypeRoot}.{descriptor.LayerRoot}.";
            Assert.That(
                descriptor.Namespace,
                Does.StartWith(expectedPrefix),
                $"Namespace '{descriptor.Namespace}' does not match '{expectedPrefix}' for {descriptor.FilePath}.");
        }
    }

    [Test]
    public void TestClasses_HaveLayerAndSingleBehaviorCategory()
    {
        var testTypes = typeof(TestProjectStructureConventionsTests).Assembly
            .GetTypes()
            .Where(type => type.IsClass && type.IsPublic)
            .Where(type => type.Name.EndsWith("Tests", StringComparison.Ordinal))
            .Where(type => type.Namespace is not null)
            .Where(type => !type.Namespace!.StartsWith("CheckoutKata.Tests.Shared.", StringComparison.Ordinal))
            .ToArray();

        foreach (var testType in testTypes)
        {
            var categories = testType.GetCustomAttributes<CategoryAttribute>(inherit: true)
                .Select(attribute => attribute.Name)
                .ToHashSet(StringComparer.Ordinal);

            var namespaceParts = testType.Namespace!.Split('.', StringSplitOptions.RemoveEmptyEntries);
            Assert.That(namespaceParts.Length, Is.GreaterThanOrEqualTo(4), $"Unexpected namespace for {testType.FullName}");
            var layerCategory = namespaceParts[3];

            Assert.That(
                categories.Contains(layerCategory),
                Is.True,
                $"Missing layer category '{layerCategory}' on {testType.FullName}.");

            var behaviorCategoryCount = categories.Count(category => BehaviorCategories.Contains(category));
            Assert.That(
                behaviorCategoryCount,
                Is.EqualTo(1),
                $"Expected exactly one behavior category on {testType.FullName}.");
        }
    }

    private static IReadOnlyCollection<TestSourceDescriptor> DiscoverTestSourceFiles()
    {
        var projectRoot = FindProjectRoot();
        var testFiles = Directory
            .GetFiles(projectRoot, "*Tests.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains("\\TestResults\\", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains("\\Shared\\Conventions\\", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return testFiles.Select(path => ParseDescriptor(projectRoot, path)).ToArray();
    }

    private static TestSourceDescriptor ParseDescriptor(string projectRoot, string filePath)
    {
        var content = File.ReadAllText(filePath);
        var namespaceMatch = Regex.Match(content, @"namespace\s+([\w\.]+);", RegexOptions.Multiline);
        Assert.That(namespaceMatch.Success, Is.True, $"No namespace declaration found in {filePath}.");

        var classMatches = Regex.Matches(content, @"public\s+(?:sealed\s+)?class\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Multiline);
        var classNames = classMatches
            .Select(match => match.Groups[1].Value)
            .ToArray();

        var relativePath = Path.GetRelativePath(projectRoot, filePath);
        var pathSegments = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        Assert.That(pathSegments.Length, Is.GreaterThanOrEqualTo(3), $"Unexpected test path format: {relativePath}");
        var testTypeRoot = pathSegments[0];
        Assert.That(TestTypeRoots, Does.Contain(testTypeRoot), $"Unexpected test type root '{testTypeRoot}' for {relativePath}.");

        var layerRoot = pathSegments[1];
        Assert.That(LayerRoots, Does.Contain(layerRoot), $"Unexpected layer root '{layerRoot}' for {relativePath}.");

        return new TestSourceDescriptor(
            filePath,
            Path.GetFileNameWithoutExtension(filePath),
            namespaceMatch.Groups[1].Value,
            testTypeRoot,
            layerRoot,
            classNames);
    }

    private static string FindProjectRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "CheckoutKata.Tests.csproj")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate CheckoutKata.Tests project root.");
    }

    private sealed record TestSourceDescriptor(
        string FilePath,
        string FileName,
        string Namespace,
        string TestTypeRoot,
        string LayerRoot,
        IReadOnlyList<string> PublicClassNames);
}
