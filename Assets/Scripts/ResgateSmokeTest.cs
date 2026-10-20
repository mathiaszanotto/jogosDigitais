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

public sealed class ResgateSmokeTest : MonoBehaviour
{
    private readonly List<string> checks = new List<string>(), errors = new List<string>(), diagnostics = new List<string>();
    private Keyboard keyboard;
    private Gamepad pad;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-resgateValidate") < 0 || FindAnyObjectByType<ResgateSmokeTest>() != null) return;
        var go = new GameObject("Resgate validation"); DontDestroyOnLoad(go); go.AddComponent<ResgateSmokeTest>();
    }
    private void Start()
    {
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>(); pad = InputSystem.AddDevice<Gamepad>();
        Application.logMessageReceived += Log;
        StartCoroutine(Guard());
    }
    private void Log(string message, string stack, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        if (stack.Contains("UnityEditor.Search.")) diagnostics.Add(message + "\n" + stack); else errors.Add(message + "\n" + stack);
    }
    private void Check(bool value, string description) { if (!value) throw new Exception(description); checks.Add(description); Debug.Log("RESGATE_CHECK_OK: " + description); }
    private void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    private void Pad(GamepadState state) => InputSystem.QueueStateEvent(pad, state);
    private void DisableEnemies() { foreach (var e in FindObjectsByType<TemplarGuard>()) { e.enabled = false; e.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; } }
    private IEnumerator Guard()
    {
        var run = Run();
        while (true)
        {
            bool more; object current = null;
            try { more = run.MoveNext(); if (more) current = run.Current; }
            catch (Exception e) { errors.Add(e.ToString()); Finish(); yield break; }
            if (!more) break;
            yield return current;
        }
        Finish();
    }
    private IEnumerator Run()
    {
        for (int i = 0; i < 12; i++) yield return null;
        Check(SceneManager.GetActiveScene().name == "MenuInicial", "Menu inicial presente e temático");
        Check(FindAnyObjectByType<MainMenu>().playButton != null, "Botão inicial selecionável com controle");
        Check(Array.Exists(FindObjectsByType<AudioSource>(), a => a.loop && a.clip != null && a.clip.name == "ComingHome_8bit" && a.isPlaying), "ComingHome_8bit em loop no menu");
        Capture("menu.png");
        Pad(new GamepadState().WithButton(GamepadButton.South));
        for (int i = 0; i < 6; i++) yield return null;
        Pad(new GamepadState()); yield return null;
        Check(SceneManager.GetActiveScene().name == "Jogo", "Botão A do controle inicia a missão pela UI");
        for (int i = 0; i < 8; i++) yield return null;
        var manager = GameManager.Instance; var player = FindAnyObjectByType<PlayerMovement>(); var body = player.GetComponent<Rigidbody2D>();
        Check(Array.Exists(FindObjectsByType<AudioSource>(), a => a.loop && a.clip != null && a.clip.name == "AceOfSpades_8bit" && a.isPlaying), "AceOfSpades_8bit em loop exclusivo no gameplay");
        Check(player.GetComponent<SpriteRenderer>().sprite.name == "Vampire" && FindObjectsByType<TemplarGuard>()[0].GetComponent<SpriteRenderer>().sprite.name == "Templar", "Vampiro e templários usam os novos sprites 8-bit");
        Check(FindAnyObjectByType<Collectible>().GetComponent<SpriteRenderer>().sprite.name == "BloodVial", "Coletáveis são frascos de sangue");
        Check(manager.TotalCollectibles == 12 && manager.Health == 3, "Missão inicia com 12 frascos e três vidas");
        Check(manager.RemainingTime > 40 && manager.Elapsed >= 0, "Reserva de sangue e cronômetro inicializados");
        Check(FindObjectsByType<TemplarGuard>().Length == 2, "Dois templários instanciados de prefabs");
        Check(FindObjectsByType<BoxCollider2D>().Length == 10, "Arena modificada com seis obstáculos internos e quatro paredes");
        Check(AllPickupsReachable(), "Busca em grade com raio do jogador alcança todos os frascos sem atravessar obstáculos");
        Check(manager.healthText != null && manager.scoreText != null && manager.staminaBar != null, "HUD exibe vida, score, sangue, cronômetro e stamina");
        Check(manager.healthText.font.HasCharacter('●') && manager.healthText.font.HasCharacter('○') && manager.timerText.font.HasCharacter('Ê'), "Fonte incorporada contém glifos de vida e sangue");
        Check(manager.healthText.canvas.sortingOrder >= 100, "Interface e resultado renderizam acima dos sprites da arena");
        Check(manager.damageSound != null && manager.victorySound != null && manager.defeatSound != null, "Efeitos distintos de coleta, dano, vitória e derrota");
        Capture("jogo.png"); DisableEnemies();
        body.position = Vector2.zero; Keys(Key.D); yield return null; var start = body.position;
        for (int i = 0; i < 25; i++) yield return new WaitForFixedUpdate();
        float cardinal = Vector2.Distance(start, body.position); Keys(); yield return null;
        Check(Mathf.Abs(cardinal - 2.5f) < .16f, $"Teclado: distância cardinal {cardinal:F3} em 0,5s");
        body.position = Vector2.zero; Keys(Key.D, Key.W); yield return null; start = body.position;
        for (int i = 0; i < 25; i++) yield return new WaitForFixedUpdate();
        float diagonal = Vector2.Distance(start, body.position); Keys(); yield return null;
        Check(Mathf.Abs(diagonal - cardinal) < .16f, $"Diagonal normalizada: distância {diagonal:F3}");
        body.position = Vector2.zero; Pad(new GamepadState { leftStick = Vector2.right }); yield return null; start = body.position;
        for (int i = 0; i < 25; i++) yield return new WaitForFixedUpdate();
        float joystick = Vector2.Distance(start, body.position); Pad(new GamepadState()); yield return null;
        Check(Mathf.Abs(joystick - cardinal) < .16f, "Analógico do controle tem a mesma velocidade do teclado");
        body.position = Vector2.zero; Pad(new GamepadState().WithButton(GamepadButton.DpadUp)); yield return null;
        for (int i = 0; i < 5; i++) yield return new WaitForFixedUpdate();
        Pad(new GamepadState()); yield return null; Check(body.position.y > .3f, "Direcional digital do controle funciona");
        body.position = Vector2.zero; Pad(new GamepadState { leftStick = Vector2.right }.WithButton(GamepadButton.RightShoulder)); yield return null; start = body.position;
        for (int i = 0; i < 25; i++) yield return new WaitForFixedUpdate();
        float sprint = Vector2.Distance(start, body.position); Pad(new GamepadState()); yield return null;
        Check(sprint > cardinal * 1.4f && player.Stamina < .95f, "RB ativa impulso mais rápido e consome stamina");
        float depleted = player.Stamina;
        for (int i = 0; i < 15; i++) yield return new WaitForFixedUpdate();
        Check(player.Stamina > depleted, "Stamina recupera ao soltar o impulso");
        Keys(Key.D, Key.LeftShift); yield return null;
        for (int i = 0; i < 150; i++) yield return new WaitForFixedUpdate();
        Check(player.Stamina < .03f && !player.Sprinting, "Stamina esgota e impede impulso ilimitado com Shift pressionado");
        Keys(); yield return null;
        for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
        Check(player.Stamina > .05f, "Reserva volta a carregar depois de soltar Shift");
        manager.Restart(); for (int i = 0; i < 6; i++) yield return null;
        manager = GameManager.Instance; player = FindAnyObjectByType<PlayerMovement>(); body = player.GetComponent<Rigidbody2D>(); DisableEnemies();
        body.position = Vector2.zero; Keys(Key.D);
        for (int i = 0; i < 110; i++) yield return new WaitForFixedUpdate();
        Keys(); yield return null; Check(body.position.x > 6.9f && body.position.x < 7.3f, "Colisão impede escapar pelo casco da estação");
        Check(manager.CollectedCount == 1, "Trigger físico coleta frasco uma única vez");

        body.position = Vector2.zero;
        var enemy = FindObjectsByType<TemplarGuard>()[0]; enemy.enabled = true;
        var enemyBody = enemy.GetComponent<Rigidbody2D>(); var oldEnemy = enemyBody.position;
        for (int i = 0; i < 15; i++) yield return new WaitForFixedUpdate();
        Check(Vector2.Distance(oldEnemy, enemyBody.position) > .1f && !enemy.IsChasing, "Drone patrulha sem jogador próximo");
        body.position = enemyBody.position + Vector2.left * 1.1f;
        for (int i = 0; i < 3; i++) yield return new WaitForFixedUpdate();
        Check(enemy.IsChasing, "Drone muda de patrulha para perseguição quando há linha de visão");
        body.position = enemyBody.position;
        for (int i = 0; i < 4; i++) yield return new WaitForFixedUpdate();
        Check(manager.Health == 2, "Contato físico com templário remove uma vida");
        Check(!manager.TakeDamage() && manager.Health == 2, "Invulnerabilidade impede perder várias vidas no mesmo contato");
        DisableEnemies(); body.position = Vector2.zero;
        yield return new WaitForSeconds(1.6f); manager.TakeDamage();
        yield return new WaitForSeconds(1.6f); manager.TakeDamage();
        Check(manager.Health == 0 && manager.State == GameManager.MatchState.Lost && manager.victoryPanel.activeSelf, "Perda das três vidas termina a partida com opção de reinício");
        Capture("derrota-vida.png"); manager.Restart();
        for (int i = 0; i < 6; i++) yield return null;
        manager = GameManager.Instance; player = FindAnyObjectByType<PlayerMovement>(); body = player.GetComponent<Rigidbody2D>(); DisableEnemies();
        Check(manager.Health == 3 && manager.Score == 0 && manager.CollectedCount == 0, "Reinício após derrota restaura vida, score e frascos");
        float before = manager.RemainingTime; manager.AdvanceClock(20);
        Check(Mathf.Abs(manager.RemainingTime - (before - 20)) < .01f, "Tempo funciona como recurso finito de sangue");
        var first = FindObjectsByType<Collectible>()[0]; before = manager.RemainingTime; body.position = first.transform.position;
        for (int i = 0; i < 3; i++) yield return new WaitForFixedUpdate();
        yield return null; Check(manager.RemainingTime > before + 2.5f && manager.Score == 100, "Frasco repõe três segundos e concede cem pontos");
        manager.AdvanceClock(manager.RemainingTime + 1);
        Check(manager.State == GameManager.MatchState.Lost && manager.resultTitle.text.Contains("SEDE"), "Reserva de sangue zero produz derrota explícita sem softlock");
        Check(manager.victoryText.text.Contains("TEMPO FINAL"), "Resultado de derrota mostra tempo final");
        yield return null; // Allow the next UI frame to display the final oxygen value.
        Capture("derrota-tempo.png"); manager.Restart();
        for (int i = 0; i < 6; i++) yield return null;
        manager = GameManager.Instance; player = FindAnyObjectByType<PlayerMovement>(); body = player.GetComponent<Rigidbody2D>(); DisableEnemies();
        foreach (var item in FindObjectsByType<Collectible>())
        {
            string name = item.name; int count = manager.CollectedCount; body.position = item.transform.position;
            for (int i = 0; i < 3; i++) yield return new WaitForFixedUpdate(); yield return null;
            Check(manager.CollectedCount == count + 1, "Coleta física: " + name);
        }
        Check(manager.HasWon && manager.Score >= 1200 && manager.victoryPanel.activeSelf, "Último frasco gera vitória e bônus por sangue restante");
        Check(manager.victoryText.text.Contains("TEMPO FINAL"), "Resultado de vitória mostra tempo final");
        Check(FindObjectsByType<Collectible>().Length == 0, "Frascos coletados são removidos");
        Capture("vitoria.png"); var frozen = body.position; Keys(Key.A);
        for (int i = 0; i < 12; i++) yield return new WaitForFixedUpdate(); Keys(); yield return null;
        Check(Vector2.Distance(frozen, body.position) < .01f, "Partida encerrada bloqueia movimento");
        manager.restartButton.onClick.Invoke(); for (int i = 0; i < 6; i++) yield return null;
        Check(GameManager.IsPlaying && GameManager.Instance.CollectedCount == 0, "Botão reiniciar funciona após vitória");
        Pad(new GamepadState().WithButton(GamepadButton.Select)); for (int i = 0; i < 6; i++) yield return null;
        Pad(new GamepadState()); yield return null;
        Check(SceneManager.GetActiveScene().name == "MenuInicial" && GameManager.Instance == null, "VIEW retorna ao menu e limpa o singleton");
        Keys(Key.Enter); for (int i = 0; i < 6; i++) yield return null; Keys(); yield return null;
        Check(SceneManager.GetActiveScene().name == "Jogo", "Enter inicia uma nova partida");
        Keys(Key.Escape); for (int i = 0; i < 6; i++) yield return null; Keys(); yield return null;
        Check(SceneManager.GetActiveScene().name == "MenuInicial", "Escape sempre permite retornar ao menu");
    }
    private bool AllPickupsReachable()
    {
        const float step = .25f; const int width = 59, height = 27;
        var seen = new HashSet<Vector2Int>(); var queue = new Queue<Vector2Int>();
        Vector2 World(Vector2Int p) => new Vector2(-7.25f + p.x * step, -3.4f + p.y * step);
        Vector2Int Cell(Vector2 p) => new Vector2Int(Mathf.RoundToInt((p.x + 7.25f) / step), Mathf.RoundToInt((p.y + 3.4f) / step));
        queue.Enqueue(Cell(Vector2.zero)); seen.Add(Cell(Vector2.zero));
        Vector2Int[] dirs = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        while (queue.Count > 0)
        {
            var p = queue.Dequeue(); foreach (var d in dirs)
            {
                var n = p + d; if (n.x < 0 || n.x >= width || n.y < 0 || n.y >= height || seen.Contains(n)) continue;
                if (Physics2D.OverlapCircle(World(n), .33f, 1 << 8)) continue;
                seen.Add(n); queue.Enqueue(n);
            }
        }
        foreach (var item in FindObjectsByType<Collectible>()) if (!seen.Contains(Cell(item.transform.position))) return false;
        return true;
    }
    private void Capture(string name)
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
        var camera = Camera.main; var oldTarget = camera.targetTexture;
        var target = new RenderTexture(960, 600, 24); target.Create(); camera.targetTexture = target;
        camera.orthographicSize = 5 * Mathf.Max(1, 1.6f / camera.aspect); Canvas.ForceUpdateCanvases();
        RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
        var previous = RenderTexture.active; RenderTexture.active = target;
        var image = new Texture2D(960, 600, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, 960, 600), 0, 0); image.Apply();
        Directory.CreateDirectory("Documentation/Previews"); File.WriteAllBytes("Documentation/Previews/" + name, image.EncodeToPNG());
        camera.targetTexture = oldTarget; RenderTexture.active = previous; target.Release(); Destroy(target); Destroy(image);
    }
    private void Finish()
    {
        Application.logMessageReceived -= Log;
        var report = new Report { success = errors.Count == 0, unityVersion = Application.unityVersion, graphicsDevice = SystemInfo.graphicsDeviceType.ToString(), checks = checks.ToArray(), errors = errors.ToArray(), editorDiagnostics = diagnostics.ToArray() };
        File.WriteAllText("Documentation/resgate-validation.json", JsonUtility.ToJson(report, true));
        UnityEditor.EditorApplication.Exit(errors.Count == 0 ? 0 : 1);
    }
    [Serializable] private sealed class Report { public string title = "The Blood Warfare"; public bool success; public string unityVersion, graphicsDevice; public string[] checks, errors, editorDiagnostics; }
}
#endif



