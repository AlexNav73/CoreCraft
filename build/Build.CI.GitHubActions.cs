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
    OnPushTags = new[] { "[0-9]+.[0-9]+.[0-9]+" },
    PublishArtifacts = false,
    AutoGenerate = true,
    InvokedTargets = new[] { nameof(Publish) })]
internal partial class Build
{
}
