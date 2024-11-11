using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class SceneController : EditorWindow
{
    private static List<string> scenePaths = new List<string>
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/LobbyScene.unity",
        "Assets/Scenes/CharacterSelectScene.unity",
        "Assets/Scenes/GameScene.unity",
    };

    [MenuItem("Window/SceneController")]
    public static void ShowWindow()
    {
        GetWindow<SceneController>("Scene Controller");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Controller", EditorStyles.boldLabel);
        foreach (string scenePath in scenePaths)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (GUILayout.Button(sceneName))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath);
                }
            }
        }
    }
}
