#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Opt-in end-to-end validation; never runs in an ordinary editor session or player build.</summary>
public sealed class TutorialSmokeTest : MonoBehaviour
{
    private readonly List<string> checks = new List<string>();
    private readonly List<string> errors = new List<string>();
    private readonly List<string> editorDiagnostics = new List<string>();
    private Keyboard keyboard;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-tutorialValidate") < 0 || FindAnyObjectByType<TutorialSmokeTest>() != null) return;
        var go = new GameObject("Tutorial smoke validation");
        DontDestroyOnLoad(go);
        go.AddComponent<TutorialSmokeTest>();
    }

    private void Start()
    {
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
        Application.logMessageReceived += OnLog;
        StartCoroutine(GuardedRun());
    }
    private void OnLog(string message, string trace, LogType type)
    {
        if (trace.Contains("UnityEditor.Search.")) { editorDiagnostics.Add(message + "\n" + trace); return; }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message);
    }
    private IEnumerator GuardedRun()
    {
        var run = Run();
        while (true)
        {
            object current = null;
            bool more;
            try { more = run.MoveNext(); if (more) current = run.Current; }
            catch (Exception exception) { errors.Add(exception.ToString()); Finish(false); yield break; }
            if (!more) break;
            yield return current;
        }
        Finish(errors.Count == 0);
    }
    private void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        checks.Add(description);
        Debug.Log("TUTORIAL_CHECK_OK: " + description);
    }
    private void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    private IEnumerator Run()
    {
        yield return null;
        yield return null;
        for (int i = 0; i < 8; i++) yield return null;
        Check(SceneManager.GetActiveScene().name == "MenuInicial", "Initial scene is MenuInicial");
        Check(FindAnyObjectByType<MainMenu>() != null, "Menu controller exists");
        Capture("menu.png");
        var playButton = GameObject.Find("JOGAR").GetComponent<Button>();
        Check(playButton.onClick.GetPersistentEventCount() == 1, "Play button has a serialized scene transition");
        playButton.onClick.Invoke();
        yield return null;
        yield return null;
        yield return new WaitForFixedUpdate();
        for (int i = 0; i < 8; i++) yield return null;
        Check(SceneManager.GetActiveScene().name == "Jogo", "Play button opens Jogo");
        var manager = GameManager.Instance;
        var player = FindAnyObjectByType<PlayerMovement>();
        var body = player.GetComponent<Rigidbody2D>();
        Check(manager != null && manager.TotalCollectibles == 12, "Twelve collectible prefab instances registered");
        Check(body.gravityScale == 0 && player.GetComponent<CircleCollider2D>() != null, "Player uses 2D physics without gravity");
        Check(player.GetComponent<AudioSource>().clip != null && !player.GetComponent<AudioSource>().playOnAwake, "Pickup AudioSource is configured");
        Capture("jogo.png");

        body.position = Vector2.zero;
        Keys(Key.D);
        yield return null;
        Vector2 start = body.position;
        for (int i = 0; i < 25; i++) yield return new WaitForFixedUpdate();
        float cardinal = Vector2.Distance(start, body.position);
        Keys(); yield return null;
        Check(cardinal > 2.2f && cardinal < 2.8f, $"Keyboard moves player at 5 world units per second (distance={cardinal:F3}, input={player.Movement}, key={keyboard.dKey.isPressed}, active={GameManager.IsPlaying}, simulation={Physics2D.simulationMode})");

        body.position = Vector2.zero;
        Keys(Key.D, Key.W); yield return null;
        start = body.position;
        for (int i = 0; i < 25; i++) yield return new WaitForFixedUpdate();
        float diagonal = Vector2.Distance(start, body.position);
        Keys(); yield return null;
        Check(Mathf.Abs(diagonal - cardinal) < .16f, $"Diagonal speed equals cardinal speed (distances {diagonal:F3}, {cardinal:F3})");

        body.position = Vector2.zero;
        Keys(Key.D); yield return null;
        for (int i = 0; i < 110; i++) yield return new WaitForFixedUpdate();
        Keys(); yield return null;
        Check(body.position.x > 6.9f && body.position.x < 7.3f, "Right wall physically contains the player");
        Check(manager.CollectedCount == 1, "Crossing a triangle triggers exactly one collection");
        Check(player.GetComponent<AudioSource>().clip.samples > 0, "Imported pickup WAV contains audio samples");

        var remaining = FindObjectsByType<Collectible>();
        foreach (var item in remaining)
        {
            string itemName = item.name;
            int before = manager.CollectedCount;
            body.position = item.transform.position;
            for (int i = 0; i < 3; i++) yield return new WaitForFixedUpdate();
            yield return null;
            Check(manager.CollectedCount == before + 1, "Physics trigger collects " + itemName);
        }
        yield return null;
        Check(manager.HasWon && manager.CollectedCount == 12, "Final pickup enters victory state");
        Check(manager.victoryPanel.activeSelf && !GameManager.IsPlaying, "Victory panel appears and movement stops");
        Check(FindObjectsByType<Collectible>().Length == 0, "Collected objects are destroyed");
        Capture("vitoria.png");
        var oldPosition = body.position;
        Keys(Key.A); yield return null;
        for (int i = 0; i < 12; i++) yield return new WaitForFixedUpdate();
        Keys(); yield return null;
        Check(Vector2.Distance(oldPosition, body.position) < .01f, "Player cannot move after victory");
        GameObject.Find("Jogar novamente").GetComponent<Button>().onClick.Invoke();
        yield return null; yield return null; yield return new WaitForFixedUpdate();
        manager = GameManager.Instance;
        Check(manager.CollectedCount == 0 && manager.TotalCollectibles == 12 && !manager.HasWon, "Replay resets state and restores collectibles");
        manager.ReturnToMenu();
        yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name == "MenuInicial" && GameManager.Instance == null, "Return to menu clears static GameManager");
        Keys(Key.Enter); yield return null; yield return null; yield return null;
        Keys(); yield return null;
        Check(SceneManager.GetActiveScene().name == "Jogo", "Enter shortcut starts another game");
        Keys(Key.Escape); yield return null; yield return null; yield return null;
        Keys(); yield return null;
        Check(SceneManager.GetActiveScene().name == "MenuInicial", "Escape returns to menu");
    }
    private void Capture(string name)
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
        Canvas.ForceUpdateCanvases();
        var target = new RenderTexture(960, 600, 24);
        target.Create();
        var camera = Camera.main;
        var oldTarget = camera.targetTexture;
        camera.targetTexture = target;
        Canvas.ForceUpdateCanvases();
        var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
        RenderPipeline.SubmitRenderRequest(camera, request);
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        var texture = new Texture2D(960, 600, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, 960, 600), 0, 0);
        texture.Apply();
        Directory.CreateDirectory("Documentation/Previews");
        File.WriteAllBytes("Documentation/Previews/" + name, texture.EncodeToPNG());
        camera.targetTexture = oldTarget;
        RenderTexture.active = previous;
        target.Release();
        Destroy(target); Destroy(texture);
    }
    private void Finish(bool success)
    {
        Application.logMessageReceived -= OnLog;
        var report = new Report { success = success, unityVersion = Application.unityVersion, graphicsDevice = SystemInfo.graphicsDeviceType.ToString(), checks = checks.ToArray(), errors = errors.ToArray(), editorDiagnostics = editorDiagnostics.ToArray() };
        File.WriteAllText("Documentation/validation.json", JsonUtility.ToJson(report, true));
        Debug.Log(success ? "TUTORIAL_VALIDATION_OK" : "TUTORIAL_VALIDATION_FAILED");
        UnityEditor.EditorApplication.Exit(success ? 0 : 1);
    }
    [Serializable] private sealed class Report { public bool success; public string unityVersion; public string graphicsDevice; public string[] checks; public string[] errors; public string[] editorDiagnostics; }
}
#endif

