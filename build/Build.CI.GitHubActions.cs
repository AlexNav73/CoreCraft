using Nuke.Common.CI.GitHubActions;

namespace build;

[GitHubActions(
    "releasing",
    image: GitHubActionsImage.WindowsLatest,
    OnPushBranches = new[] { "master", "release/*" },
    PublishArtifacts = true,
    AutoGenerate = false,
    InvokedTargets = new[] { nameof(Publish) },
    ImportSecrets = new[] { nameof(RegistryApiKey) })]
internal partial class Build
{
}
