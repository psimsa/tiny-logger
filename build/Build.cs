using NuGet.Versioning;

using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
    "Continuous build",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranchesIgnore = new[] { "main" },
    InvokedTargets = new[] { nameof(Clean), nameof(Compile), nameof(Test), nameof(Pack)},
    EnableGitHubToken = true
)]
[GitHubActions(
    "Manual publish to Github Nuget",
    GitHubActionsImage.UbuntuLatest,
    On = new [] { GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(Pack), nameof(PublishToGitHubNuget) },
    EnableGitHubToken = true
)]
[GitHubActions(
    "Build main and publish to nuget",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    InvokedTargets = new[]
        { nameof(Clean), nameof(Compile), nameof(Pack), nameof(PublishToGitHubNuget), nameof(Publish) },
    ImportSecrets = new[] { nameof(NuGetApiKey) },
    EnableGitHubToken = true)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter] [Secret] readonly string NuGetApiKey;

    [Solution(GenerateProjects = true)] readonly Solution Solution;
    GitHubActions GitHubActions => GitHubActions.Instance;

    [GitRepository] readonly GitRepository Repository;

    [LatestNuGetVersion(
        packageId: "Psimsa.TinyLogger",
        IncludePrerelease = false)]
    readonly NuGetVersion TinyLoggerVersion;

    readonly AbsolutePath ArtifactsDirectory = RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution)
            );
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
            );
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetNoBuild(true)
                .SetNoRestore(true)
                .SetVerbosity(DotNetVerbosity.Normal)
            );
        });

    Target Pack => _ => _
        .DependsOn(Test)
        .DependsOn(Clean)
        .Produces(ArtifactsDirectory / "*.nupkg")
        .Executes(() =>
        {
            var newMajor = 1;
            var newMinor = 0;
            var newPatch = TinyLoggerVersion.Patch + 1;

            if (newMajor > TinyLoggerVersion.Major)
            {
                newMinor = 0;
                newPatch = 0;
            }
            else if (newMinor > TinyLoggerVersion.Minor)
            {
                newPatch = 0;
            }

            var newVersion = new NuGetVersion(newMajor, newMinor, newPatch,
                Repository.IsOnMainOrMasterBranch() ? null : $"preview{GitHubActions?.RunNumber ?? 0}");

            DotNetPack(_ => _
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetNoBuild(true)
                .SetNoRestore(true)
                .SetVersion(newVersion.ToString())
                .SetVerbosity(DotNetVerbosity.Normal)
                .SetProject(Solution.src.TinyLogger)
            );
        });

    Target Publish => _ => _
        .DependsOn(Pack)
        .Consumes(Pack)
        .Requires(() => NuGetApiKey)
        .Executes(() =>
        {
            DotNetNuGetPush(_ => _
                .SetTargetPath(ArtifactsDirectory / "*.nupkg")
                .SetSource("https://api.nuget.org/v3/index.json")
                .SetApiKey(NuGetApiKey)
            );
        });

    Target PublishToGitHubNuget => _ => _
        .DependsOn(Pack)
        .Consumes(Pack)
        .Executes(() =>
        {
            DotNetNuGetPush(_ => _
                .SetTargetPath(ArtifactsDirectory / "*.nupkg")
                .SetSource("https://nuget.pkg.github.com/psimsa/index.json")
                .SetApiKey(GitHubActions.Token)
            );
        });
}
