using Nuke.Common.CI.GitHubActions;

namespace build;

[GitHubActions(
    "releasing",
    image: GitHubActionsImage.WindowsLatest,
    OnPushBranches = ["master", "release/*"],
    PublishArtifacts = true,
    AutoGenerate = false,
    InvokedTargets = [nameof(Publish)],
    ImportSecrets = [nameof(RegistryApiKey)])]
internal partial class Build
{
}
