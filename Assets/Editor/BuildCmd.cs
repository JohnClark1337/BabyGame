using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildCmd
{
    public static void PerformBuild()
    {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
        options.locationPathName = "BabyGame.apk";
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception($"Build failed: {report.summary.result}");
    }
}
