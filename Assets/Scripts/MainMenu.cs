using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenu : MonoBehaviour
{
    public Button playButton;
    private void Start() { if (playButton != null) playButton.Select(); }
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) Play();
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame) Play();
    }
    public void Play() { Time.timeScale = 1; SceneManager.LoadScene("Jogo"); }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
