using UnityEditor;
using UnityEngine;

public static class BuildScript
{
    private const string ScenesRoot = "Assets/Scenes";
    private static readonly string[] Flow =
    {
        "MainMenu",
        "Intro",
        "CampaignLevel01",
        "CampaignLevel02",
        "CampaignLevel03",
        "Victory"
    };

    public static void BuildTest()
    {
        string[] scenes = new string[Flow.Length];
        for (int i = 0; i < Flow.Length; i++)
        {
            scenes[i] = $"{ScenesRoot}/{Flow[i]}.unity";
        }

        const string output = "Builds/Test/WhatTheHell3D.x86_64";
        System.IO.Directory.CreateDirectory("Builds/Test");

        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(
            scenes,
            output,
            BuildTarget.StandaloneLinux64,
            BuildOptions.Development);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[Build] OK: {report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalErrors} errores, {output}");
        }
        else
        {
            Debug.LogError($"[Build] Falló: {report.summary.result}, {report.summary.totalErrors} errores.");
            EditorApplication.Exit(1);
        }
    }
}
