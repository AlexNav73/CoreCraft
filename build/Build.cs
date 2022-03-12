using Nuke.Common.CI.GitHubActions;
using System.Collections.Generic;
using System.Linq;

namespace build;

[CheckBuildProjectConfigurations]
internal partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitVersion(Framework = "net6.0", NoFetch = true)]
    readonly GitVersion GitVersion;
    [GitRepository]
    readonly GitRepository GitRepository;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [Parameter]
    [Secret]
    readonly string RegistryApiKey;

    static AbsolutePath SourceDirectory => RootDirectory / "src";
    static AbsolutePath PackagesDirectory => RootDirectory / "packages";

    [CI]
    readonly GitHubActions GitHubActions;

    string GitHubRegistrySource => GitHubActions != null
        ? $"https://nuget.pkg.github.com/{GitHubActions.RepositoryOwner}/index.json"
        : null;
    IEnumerable<AbsolutePath> PushPackageFiles => PackagesDirectory.GlobFiles("*.nupkg");

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Clean, Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target RunMemoryTests => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testProject = Solution.GetProject("Navitski.Crystalized.Model.Tests.MemoryLeaks");
            var framework = testProject.GetTargetFrameworks().Single();
            var assemblyName = testProject.GetProperty("AssemblyName") + ".dll";
            var assemblyPath = testProject.Directory / "bin" / Configuration / framework / assemblyName;

            DotMemoryUnit($"{NUnitTasks.NUnitPath} --propagate-exit-code -- {assemblyPath}");
        });

    Target RunTests => _ => _
        .DependsOn(Compile)
        .Triggers(RunMemoryTests)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Produces(PackagesDirectory / "*.nupkg")
        .Executes(() =>
        {
            EnsureCleanDirectory(PackagesDirectory);

            DotNetPack(s => s
                .SetProject(Solution.Navitski_Crystalized_Model)
                .Apply(PackSettingsBase)
                .AddPackageTags("Model", "Generator", "SourceGenerator", "WPF"));

            DotNetPack(s => s
                .SetProject(Solution.Navitski_Crystalized_Model_Generators)
                .Apply(PackSettingsBase)
                .AddPackageTags("Model", "Generator", "SourceGenerator", "WPF"));

            DotNetPack(s => s
                .Apply(PackSettingsBase)
                .SetProject(Solution.Navitski_Crystalized_Model_Storage_Sqlite)
                .AddPackageTags("Model", "Generator", "SourceGenerator", "WPF"));

            ReportSummary(_ => _
                    .AddPair("Packages", PackagesDirectory.GlobFiles("*.nupkg").Count.ToString()));

            DotNetPackSettings PackSettingsBase(DotNetPackSettings settings) => settings
                .SetConfiguration(Configuration)
                    .SetNoRestore(SucceededTargets.Contains(Restore))
                .SetNoBuild(SucceededTargets.Contains(Compile))
                .AddAuthors("Alexandr Navitskiy")
                .SetRepositoryUrl(GitRepository.HttpsUrl)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetOutputDirectory(PackagesDirectory);
        });

    Target Publish => _ => _
        .DependsOn(Pack)
        .Consumes(Pack)
        .Requires(() => RegistryApiKey)
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetSource(GitHubRegistrySource)
                .SetApiKey(RegistryApiKey)
                .CombineWith(PushPackageFiles, (_, p) => _
                    .SetTargetPath(p)));
        });
}
