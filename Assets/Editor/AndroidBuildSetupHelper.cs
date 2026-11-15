using UnityEditor;
using UnityEngine;
using UnityEditor.Build;

public static class AndroidBuildSetupHelper
{
    [MenuItem("Tools/AR Build/Verifikasi & Terapkan Pengaturan Android")]
    public static void ApplySettings()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        PlayerSettings.companyName = "Monica";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.monica.tubesvol2");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.forceInternetPermission = true;

        if (System.IO.File.Exists("Assets/Scenes/MainMenu.unity"))
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/ASS 2.unity", true)
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[AR Build Helper] Pengaturan Android berhasil diverifikasi dan disesuaikan!");
        EditorUtility.DisplayDialog("AR Android Setup", 
            "Pengaturan Android telah berhasil disesuaikan:\n\n" +
            "✔ Format APK aktif (Build App Bundle dimatikan)\n" +
            "✔ Scripting Backend: IL2CPP\n" +
            "✔ Target Architecture: ARM64\n" +
            "✔ Minimum SDK: API Level 29 (Android 10.0+ untuk Vuforia 11)\n" +
            "✔ Package Name: com.monica.tubesvol2\n\n" +
            "Silakan buka File > Build Profiles dan klik 'Build'!", "OK");
    }
}
