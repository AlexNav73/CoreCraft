using Nuke.Common.CI.GitHubActions;

namespace build;

[GitHubActions(
    "continuous",
    GitHubActionsImage.WindowsLatest,
    OnPullRequestBranches = new[] { "master", "release/*" },
    PublishArtifacts = false,
    AutoGenerate = true,
    InvokedTargets = new[] { nameof(RunTests) })]
[GitHubActions(
    "releasing",
    image: GitHubActionsImage.WindowsLatest,
    OnPushTags = new[] { "\"[0-9]+.[0-9]+.[0-9]+\"" },
    PublishArtifacts = true,
    AutoGenerate = false, // We use additional step to create GitHub release (do not override yaml)
    InvokedTargets = new[] { nameof(Publish) },
    ImportSecrets = new[] { nameof(RegistryApiKey) })]
internal partial class Build
{
}
