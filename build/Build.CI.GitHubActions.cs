using Nuke.Common.CI.GitHubActions;

namespace build;

[GitHubActions(
    "continuous",
    GitHubActionsImage.WindowsLatest,
    OnPullRequestBranches = new[] { "master", "release/*" },
    PublishArtifacts = false,
    AutoGenerate = false,
    InvokedTargets = new[] { nameof(RunTests) })]
[GitHubActions(
    "releasing",
    image: GitHubActionsImage.WindowsLatest,
    OnPushBranches = new[] { "master" },
    PublishArtifacts = true,
    AutoGenerate = false,
    InvokedTargets = new[] { nameof(Publish) },
    ImportSecrets = new[] { nameof(RegistryApiKey) })]
internal partial class Build
{
}
