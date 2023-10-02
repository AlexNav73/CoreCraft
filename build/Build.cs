using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.ReportGenerator;
using Serilog;

namespace build;

internal partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    readonly GitRepository GitRepository;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [Parameter]
    [Secret]
    readonly string RegistryApiKey;

    static AbsolutePath SourceDirectory => RootDirectory / "src";
    static AbsolutePath PackagesDirectory => RootDirectory / "packages";
    static AbsolutePath CoverageDirectory => RootDirectory / "coverage";
    static AbsolutePath ReportDirectory => RootDirectory / "coveragereport";

    [CI]
    readonly GitHubActions GitHubActions;

    string NuGetOrgRegistry => GitHubActions != null
        ? "https://api.nuget.org/v3/index.json"
        : null;

    IEnumerable<AbsolutePath> PushPackageFiles => PackagesDirectory.GlobFiles("*.nupkg");

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            Log.Information("Cleaning output folders.");
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
            Log.Information("Cleaning packages folder.");
            PackagesDirectory.CreateOrCleanDirectory();
            Log.Information("Cleaning test coverage output folder.");
            CoverageDirectory.CreateOrCleanDirectory();
            Log.Information("Cleaning test coverage report folder.");
            ReportDirectory.CreateOrCleanDirectory();
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
                .SetNoRestore(SucceededTargets.Contains(Restore)));
        });

    Target RunMemoryTests => _ => _
        .DependsOn(Compile)
        .Before(Pack, Coverage)
        .Executes(() =>
        {
            var testProject = Solution.Tests.CoreCraft_Tests_MemoryLeaks;
            var assemblyName = testProject.Name + ".dll";
            var assemblyPath = testProject.Directory / "bin" / Configuration / assemblyName;

            DotMemoryUnit($"{NUnitTasks.NUnitPath} --propagate-exit-code -- {assemblyPath}");
        });

    Target RunTests => _ => _
        .DependsOn(Compile)
        .Triggers(RunMemoryTests, Coverage)
        .Executes(() =>
        {
            CoverageDirectory.CreateOrCleanDirectory();

            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetNoBuild(SucceededTargets.Contains(Compile))
                .SetNoRestore(SucceededTargets.Contains(Restore))
                .AddProperty("CollectCoverage", true)
                .AddProperty("CoverletOutputFormat", "\\\"cobertura,json\\\"")
                .AddProperty("CoverletOutput", $"{CoverageDirectory}\\")
                .AddProperty("ExcludeByAttribute", "\\\"Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute\\\"")
                .AddProperty("Exclude", $"\\\"[{Solution.Tests.CoreCraft_Tests_Infrastructure.Name}]*,[{Solution.CoreCraft.Name}]System.*\\\"")
                .AddProperty("MergeWith", CoverageDirectory / "coverage.json")
                .SetProcessArgumentConfigurator(c => c.Add("-m:1")));
        });

    Target Perf => _ => _
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Solution.Tests.CoreCraft_Tests_Perf)
                .SetConfiguration(Configuration.Release));
        });

    Target Coverage => _ => _
        .DependsOn(RunTests)
        .Executes(() =>
        {
            ReportDirectory.CreateOrCleanDirectory();

            ReportGenerator(s => s
                .SetFramework(Solution._build.GetTargetFrameworks().Single())
                .SetReports(CoverageDirectory.GlobFiles("*.cobertura.xml").Select(x => x.ToString()))
                .SetTargetDirectory(ReportDirectory)
                .SetReportTypes(ReportTypes.Html));
        });

    Target Pack => _ => _
        .DependsOn(RunTests)
        .Produces(PackagesDirectory / "*.nupkg")
        .Executes(() =>
        {
            PackagesDirectory.CreateOrCleanDirectory();

            string MakePreviewIfNeeded(int major, int minor, int build)
            {
                if (GitRepository.IsOnMasterBranch())
                {
                    return $"{major}.{minor}.0";
                }

                return $"{major}.{minor}.{build}-preview";
            }

            DotNetPack(s => s
                .SetProject(Solution.CoreCraft)
                .Apply(PackSettingsBase)
                .SetVersion(MakePreviewIfNeeded(0, 5, 6))
                .SetDescription("A core library to build cross-platform and highly customizable domain models")
                .AddPackageTags("Model", "Domain"));

            DotNetPack(s => s
                .SetProject(Solution.CoreCraft_Generators)
                .Apply(PackSettingsBase)
                .SetVersion(MakePreviewIfNeeded(0, 5, 6))
                .SetDescription("Roslyn Source Generators for generating domain models using 'CoreCraft' library")
                .AddPackageTags("Model", "Domain", "SourceGenerator", "Generator"));

            DotNetPack(s => s
                .SetProject(Solution.CoreCraft_Storage_Sqlite)
                .Apply(PackSettingsBase)
                .SetVersion(MakePreviewIfNeeded(0, 5, 6))
                .SetDescription("SQLite storage implementation for 'CoreCraft' library")
                .AddPackageTags("Model", "Domain", "SQLite"));

            DotNetPack(s => s
                .SetProject(Solution.CoreCraft_Storage_Json)
                .Apply(PackSettingsBase)
                .SetVersion(MakePreviewIfNeeded(0, 2, 2))
                .SetDescription("Json storage implementation for 'CoreCraft' library")
                .AddPackageTags("Model", "Domain", "Json"));

            ReportSummary(_ => _
                .AddPair("Packages", PackagesDirectory.GlobFiles("*.nupkg").Count.ToString()));

            DotNetPackSettings PackSettingsBase(DotNetPackSettings settings) => settings
                .SetConfiguration(Configuration)
                .SetNoRestore(SucceededTargets.Contains(Restore))
                .SetNoBuild(SucceededTargets.Contains(Compile))
                .AddAuthors("Aliaksandr Navitski")
                .SetCopyright("Copyright (c) Aliaksandr Navitski 2023.")
                .SetPackageProjectUrl(GitRepository.HttpsUrl)
                .SetRepositoryUrl(GitRepository.HttpsUrl)
                .SetOutputDirectory(PackagesDirectory)
                .SetRepositoryType("git")
                .EnablePackageRequireLicenseAcceptance();
        });

    Target Publish => _ => _
        .DependsOn(Pack)
        .Consumes(Pack)
        .Requires(() => RegistryApiKey)
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetSource(NuGetOrgRegistry)
                .SetApiKey(RegistryApiKey)
                .SetSkipDuplicate(true)
                .CombineWith(PushPackageFiles, (_, p) => _
                    .SetTargetPath(p)));
        });
}
