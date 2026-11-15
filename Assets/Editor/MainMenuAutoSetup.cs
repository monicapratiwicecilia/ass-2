using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public static class MainMenuAutoSetup
{
    [MenuItem("Tools/Auto Setup Main Menu")]
    [MenuItem("Tools/Auto Setup All UI")]
    public static void RunSetupMenu()
    {
        RunSetup();
    }

    public static void RunSetup()
    {
        Debug.Log("[MainMenuAutoSetup] Memulai proses setup otomatis seluruh UI...");
        bool allOk = true;

        // 1. Bangun AR HUD di scene "ASS 2.unity" terlebih dahulu
        try
        {
            ARHudBuilder.BuildARHud();
            Debug.Log("OK: AR Scanning HUD di 'ASS 2.unity' berhasil dibangun.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"GAGAL: Membangun AR HUD di ASS 2.unity: {ex.Message}");
            allOk = false;
        }

        // 2. Buka scene MainMenu.unity
        string scenePath = "Assets/Scenes/MainMenu.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"GAGAL: Buka scene {scenePath}");
            ExitBatchMode(false);
            return;
        }

        // 3. Jalankan MainMenuBuilder
        MainMenuBuilder.BuildMainMenu();

        // 4. Cari MenuController dan verifikasi komponen MainMenu
        GameObject menuControllerGo = FindInSceneRootOrChildren("MenuController");
        if (menuControllerGo == null)
        {
            Debug.LogError("GAGAL: Objek MenuController tidak ditemukan!");
            ExitBatchMode(false);
            return;
        }

        var mainMenu = menuControllerGo.GetComponent<MainMenu>();
        if (mainMenu == null)
        {
            mainMenu = menuControllerGo.AddComponent<MainMenu>();
        }

        GameObject splash = FindInSceneRootOrChildren("SplashScreenPanel");
        GameObject panelMenu = FindInSceneRootOrChildren("PanelMenu");
        GameObject panelManual = FindInSceneRootOrChildren("PanelManual");
        GameObject panelAbout = FindInSceneRootOrChildren("PanelAbout");

        // 5. Wire-up via SerializedObject
        mainMenu.splashScreenPanel = splash;
        mainMenu.panelMenu = panelMenu;
        mainMenu.panelManual = panelManual;
        mainMenu.panelAbout = panelAbout;
        mainMenu.gameScene = "ASS 2";

        var so = new SerializedObject(mainMenu);
        so.Update();

        var propSplash = so.FindProperty("splashScreenPanel");
        if (propSplash != null) propSplash.objectReferenceValue = splash;

        var propMenu = so.FindProperty("panelMenu");
        if (propMenu != null) propMenu.objectReferenceValue = panelMenu;

        var propManual = so.FindProperty("panelManual");
        if (propManual != null) propManual.objectReferenceValue = panelManual;

        var propAbout = so.FindProperty("panelAbout");
        if (propAbout != null) propAbout.objectReferenceValue = panelAbout;

        var propGameScene = so.FindProperty("gameScene");
        if (propGameScene != null) propGameScene.stringValue = "ASS 2";

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(mainMenu);

        // 6. Validasi tiap field
        if (mainMenu.splashScreenPanel != null)
            Debug.Log($"OK: splashScreenPanel -> {mainMenu.splashScreenPanel.name}");
        else
        {
            Debug.LogError("GAGAL: splashScreenPanel");
            allOk = false;
        }

        if (mainMenu.panelMenu != null)
            Debug.Log($"OK: panelMenu -> {mainMenu.panelMenu.name}");
        else
        {
            Debug.LogError("GAGAL: panelMenu");
            allOk = false;
        }

        if (mainMenu.panelManual != null)
            Debug.Log($"OK: panelManual -> {mainMenu.panelManual.name}");
        else
        {
            Debug.LogError("GAGAL: panelManual");
            allOk = false;
        }

        if (mainMenu.panelAbout != null)
            Debug.Log($"OK: panelAbout -> {mainMenu.panelAbout.name}");
        else
        {
            Debug.LogError("GAGAL: panelAbout");
            allOk = false;
        }

        if (mainMenu.gameScene == "ASS 2")
            Debug.Log($"OK: gameScene -> {mainMenu.gameScene}");
        else
        {
            Debug.LogError("GAGAL: gameScene");
            allOk = false;
        }

        // 7. Validasi EventSystem
        var allEventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (allEventSystems != null && allEventSystems.Length > 0)
        {
            Debug.Log($"OK: EventSystem -> {allEventSystems[0].gameObject.name} (Memakai InputSystemUIInputModule)");
        }
        else
        {
            Debug.LogError("GAGAL: EventSystem");
            allOk = false;
        }

        // 8. Build Settings Check
        var currentScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool hasAss2 = false;
        string ass2Path = "Assets/Scenes/ASS 2.unity";
        foreach (var s in currentScenes)
        {
            if (s.path == ass2Path || s.path.EndsWith("ASS 2.unity"))
            {
                hasAss2 = true;
                break;
            }
        }
        if (!hasAss2)
        {
            currentScenes.Add(new EditorBuildSettingsScene(ass2Path, true));
            EditorBuildSettings.scenes = currentScenes.ToArray();
            Debug.Log("[MainMenuAutoSetup] Menambahkan 'ASS 2' ke Build Settings.");
        }
        else
        {
            Debug.Log("OK: 'ASS 2' sudah terdaftar di EditorBuildSettings.scenes");
        }

        // 9. Simpan scene MainMenu
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        if (saved)
        {
            Debug.Log($"OK: Scene '{scenePath}' berhasil disimpan.");
        }
        else
        {
            Debug.LogError($"GAGAL: Menyimpan scene '{scenePath}'");
            allOk = false;
        }

        Debug.Log($"[MainMenuAutoSetup] Setup selesai. Status akhir: {(allOk ? "SEMUA BERHASIL (OK)" : "ADA KEGAGALAN")}");
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Auto Setup Selesai",
                allOk 
                    ? "Seluruh UI berhasil diperbarui!\n\n1. Splash Screen Logo HD Sequensial\n2. Main Menu Full Layar Taktis\n3. Buku Panduan Kartu Rapi & Terstruktur\n4. AR Scanning HUD di Scene 'ASS 2' Siap\n\nSilakan tekan tombol Play untuk mencoba!"
                    : "Terjadi kesalahan saat setup. Cek Console Unity untuk detailnya.", "OK");
        }
        ExitBatchMode(allOk);
    }

    private static GameObject FindInSceneRootOrChildren(string objectName)
    {
        var roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            if (root == null) continue;
            if (root.name == objectName) return root;

            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t != null && t.gameObject.name == objectName)
                {
                    return t.gameObject;
                }
            }
        }
        return null;
    }

    private static void ExitBatchMode(bool success)
    {
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
