using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.ReportGenerator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace build;

[CheckBuildProjectConfigurations]
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
    static AbsolutePath CoverageDirectory = RootDirectory / "coverage";

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
            EnsureCleanDirectory(PackagesDirectory);
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
        .Executes(() =>
        {
            var testProject = Solution.Tests.Navitski_Crystalized_Model_Tests_MemoryLeaks;
            var assemblyName = testProject.Name + ".dll";
            var assemblyPath = testProject.Directory / "bin" / Configuration / assemblyName;

            DotMemoryUnit($"{NUnitTasks.NUnitPath} --propagate-exit-code -- {assemblyPath}");
        });

    Target RunTests => _ => _
        .DependsOn(Compile)
        .Triggers(RunMemoryTests, Coverage)
        .Executes(() =>
        {
            EnsureCleanDirectory(CoverageDirectory);

            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetNoBuild(SucceededTargets.Contains(Compile))
                .SetNoRestore(SucceededTargets.Contains(Restore))
                .AddProperty("CollectCoverage", true)
                .AddProperty("CoverletOutputFormat", "\\\"cobertura,json\\\"")
                .AddProperty("CoverletOutput", CoverageDirectory + "\\")
                .AddProperty("ExcludeByAttribute", "\\\"Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute\\\"")
                .AddProperty("Exclude", $"[{Solution.Tests.Navitski_Crystalized_Model_Tests_Infrastructure.Name}]*")
                .AddProperty("MergeWith", CoverageDirectory / "coverage.json")
                .SetProcessArgumentConfigurator(c => c.Add("-m:1")));
        });

    Target Coverage => _ => _
        .DependsOn(RunTests)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Executes(() =>
        {
            var reportDirectory = RootDirectory / "coveragereport";
            EnsureCleanDirectory(reportDirectory);

            ReportGenerator(s => s
                .SetFramework("net6.0")
                .SetReports(CoverageDirectory.GlobFiles("*.cobertura.xml").Select(x => x.ToString()))
                .SetTargetDirectory(reportDirectory)
                .SetReportTypes(ReportTypes.Html));
        });

    Target Pack => _ => _
        .DependsOn(RunTests)
        .Produces(PackagesDirectory / "*.nupkg")
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution.Navitski_Crystalized_Model)
                .Apply(PackSettingsBase)
                .SetDescription("A core library to build cross-platform and highly customizable domain models")
                .AddPackageTags("Model", "Domain"));

            DotNetPack(s => s
                .SetProject(Solution.Navitski_Crystalized_Model_Generators)
                .Apply(PackSettingsBase)
                .SetDescription("Roslyn Source Generators for generating domain models using 'Navitski.Crystalized.Model' library")
                .AddPackageTags("Model", "Domain", "SourceGenerator", "Generator"));

            DotNetPack(s => s
                .SetProject(Solution.Navitski_Crystalized_Model_Storage_Sqlite)
                .Apply(PackSettingsBase)
                .SetDescription("SQLite storage implementation for 'Navitski.Crystalized.Model' library")
                .AddPackageTags("Model", "Domain", "SQLite"));

            ReportSummary(_ => _
                .AddPair("Packages", PackagesDirectory.GlobFiles("*.nupkg").Count.ToString()));

            DotNetPackSettings PackSettingsBase(DotNetPackSettings settings) => settings
                .SetConfiguration(Configuration)
                .SetNoRestore(SucceededTargets.Contains(Restore))
                .SetNoBuild(SucceededTargets.Contains(Compile))
                .AddAuthors("Aliaksandr Navitski")
                .SetCopyright("Copyright (c) Aliaksandr Navitski 2022.")
                .SetPackageProjectUrl(GitRepository.HttpsUrl)
                .SetRepositoryUrl(GitRepository.HttpsUrl)
                .SetOutputDirectory(PackagesDirectory)
                .SetRepositoryType("git")
                .EnablePackageRequireLicenseAcceptance()
                .SetPackageLicenseUrl("https://github.com/AlexNav73/Navitski.Crystalized/blob/master/LICENSE.md");
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
                .SetSkipDuplicate(true)
                .CombineWith(PushPackageFiles, (_, p) => _
                    .SetTargetPath(p)));
        });
}
