using UnityEditor;
using UnityEditor.SceneManagement;

public static class TutorialValidation
{
    public static void Start()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MenuInicial.unity");
        EditorSettings.enterPlayModeOptionsEnabled = false;
        UnityEngine.Debug.Log("TUTORIAL_VALIDATION_START");
        EditorApplication.EnterPlaymode();
    }
}
