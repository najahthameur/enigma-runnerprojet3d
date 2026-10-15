#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Genere automatiquement les 3 scenes (Menu, Level1, Level2) et le Build Settings.
public static class ProjectSetup
{
    const string Dir = "Assets/Scenes";

    [InitializeOnLoadMethod]
    static void Auto()
    {
        if (!File.Exists(Dir + "/Menu.unity"))
            EditorApplication.delayCall += () => Generate(true);
    }

    [MenuItem("Jeu 2 Joueurs/Generer les scenes")]
    static void GenerateMenu() { Generate(false); }

    static void Generate(bool auto)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Dir);

        MakeScene("Menu", go => go.AddComponent<MenuBuilder>());
        MakeScene("Level1", go => go.AddComponent<LevelBuilder>().level = 1);
        MakeScene("Level2", go => go.AddComponent<LevelBuilder>().level = 2);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Dir + "/Menu.unity", true),
            new EditorBuildSettingsScene(Dir + "/Level1.unity", true),
            new EditorBuildSettingsScene(Dir + "/Level2.unity", true),
        };

        bool restart = SetInputBoth();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Dir + "/Menu.unity");

        string msg = "Scenes creees : Menu, Level1, Level2.\nOuvrez la scene Menu et cliquez sur Play !";
        if (restart)
            msg += "\n\nIMPORTANT : le reglage 'Active Input Handling' a ete mis sur 'Both'. " +
                   "Fermez et rouvrez Unity avant de jouer.";
        EditorUtility.DisplayDialog("Jeu 2 Joueurs", msg, "OK");
    }

    static void MakeScene(string name, System.Action<GameObject> setup)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var go = new GameObject("Bootstrap");
        setup(go);
        EditorSceneManager.SaveScene(scene, Dir + "/" + name + ".unity");
    }

    // Le jeu utilise l'ancien systeme d'entrees (Input.GetKey).
    static bool SetInputBoth()
    {
        try
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return false;
            var so = new SerializedObject(assets[0]);
            var p = so.FindProperty("activeInputHandler");
            if (p != null && p.intValue == 1)   // 1 = nouveau systeme seulement
            {
                p.intValue = 2;                  // 2 = les deux
                so.ApplyModifiedProperties();
                return true;
            }
        }
        catch { }
        return false;
    }
}
#endif
