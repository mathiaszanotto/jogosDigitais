using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TutorialBuilder;

public static class ResgateBuilder
{
    private static Sprite vampire, templar, vial, wall, floorTile, sigil, tomb, torch, banner, arch, bat, moon;
    [MenuItem("Blood Warfare/Recriar projeto final")]
    public static void BuildProject()
    {
        foreach (var folder in new[] { "Art", "Audio", "Prefabs", "Scenes" }) Directory.CreateDirectory("Assets/" + folder);
        font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/DejaVuSans.ttf");
        if (font == null) throw new Exception("Required UI font was not imported.");
        square = MakeSprite("Square", 1);
        circle = MakeSprite("Circle", 0);
        spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/SpriteUnlit.mat");
        if (spriteMaterial == null) { spriteMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")); AssetDatabase.CreateAsset(spriteMaterial, "Assets/Art/SpriteUnlit.mat"); }
        vampire = CastleArt.Sprite("Vampire"); templar = CastleArt.Sprite("Templar"); vial = CastleArt.Sprite("BloodVial");
        wall = CastleArt.Sprite("CastleWall"); floorTile = CastleArt.Sprite("CastleFloor"); sigil = CastleArt.Sprite("BloodSigil");
        tomb = CastleArt.Sprite("Sarcophagus"); torch = CastleArt.Sprite("Torch"); banner = CastleArt.Sprite("Banner");
        arch = CastleArt.Sprite("CastleArch"); bat = CastleArt.Sprite("Bat"); moon = CastleArt.Sprite("BloodMoon");
        ImportMusic("ComingHome_8bit"); ImportMusic("AceOfSpades_8bit");
        CastleArt.Audio(); AddTag("Coletavel");
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue = "Level"; tags.ApplyModifiedProperties();
        PlayerSettings.productName = "The Blood Warfare"; PlayerSettings.companyName = "Mathias Zanotto";
        PlayerSettings.defaultScreenWidth = 960; PlayerSettings.defaultScreenHeight = 600;
        PlayerSettings.runInBackground = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.initialMemorySize = 128;
        PlayerSettings.WebGL.maximumMemorySize = 512;
        EditorSettings.enterPlayModeOptionsEnabled = false;
        Game(); Menu();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/MenuInicial.unity", true), new EditorBuildSettingsScene("Assets/Scenes/Jogo.unity", true) };
        AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene("Assets/Scenes/MenuInicial.unity");
        Debug.Log("RESGATE_PROJECT_OK");
    }
    private static void ImportMusic(string name)
    {
        string path = "Assets/Audio/" + name + ".wav";
        if (!File.Exists(path)) throw new Exception("Missing supplied soundtrack: " + path);
        var importer = (AudioImporter)AssetImporter.GetAtPath(path);
        var settings = importer.defaultSampleSettings;
        if (settings.loadType != AudioClipLoadType.CompressedInMemory || settings.compressionFormat != AudioCompressionFormat.Vorbis || Mathf.Abs(settings.quality - .7f) > .001f || !settings.preloadAudioData)
        {
            settings.loadType = AudioClipLoadType.CompressedInMemory; settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = .7f; settings.preloadAudioData = true; importer.defaultSampleSettings = settings; importer.SaveAndReimport();
        }
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        for (int attempt = 0; clip == null && attempt < 4; attempt++)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        if (clip == null) throw new Exception("Audio asset was not imported: " + name);
        Debug.Log("BLOOD_MUSIC_READY: " + name + " / " + clip.length.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "s");
    }
    private static void Soundtrack(string track)
    {
        var audio = new GameObject("Trilha - " + track).AddComponent<AudioSource>();
        audio.clip = Clip(track); audio.loop = true; audio.playOnAwake = true; audio.volume = .25f;
    }
    private static AudioClip Clip(string name)
    {
        string path = "Assets/Audio/" + name;
        path += File.Exists(path + ".mp3") ? ".mp3" : ".wav";
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) throw new Exception("Audio asset was not imported: " + name);
        return clip;
    }
    private static void Game()
    {
        var camera = NewScene("Jogo"); camera.gameObject.AddComponent<ArenaCamera>();
        var canvas = NewCanvas(camera); Soundtrack("AceOfSpades_8bit");
        var manager = new GameObject("GameManager", typeof(AudioSource)).AddComponent<GameManager>();
        manager.GetComponent<AudioSource>().playOnAwake = false;
        manager.damageSound = Clip("Dano"); manager.victorySound = Clip("Vitoria"); manager.defeatSound = Clip("Derrota");
        var floor = new GameObject("Lajes do castelo").transform;
        for (int x = -7; x <= 7; x++) for (int y = -3; y <= 3; y++) SpriteObject("Laje", floorTile, new Vector2(x, y), Vector2.one, Color.white, -3, floor);
        SpriteObject("Selo de sangue", sigil, Vector2.zero, Vector2.one * 1.7f, Color.white, -2);
        var walls = new GameObject("Paredes e obstáculos").transform;
        Barrier("Muralha oeste", new Vector2(-7.65f, -.1f), new Vector2(.3f, 7.4f), walls);
        Barrier("Muralha leste", new Vector2(7.65f, -.1f), new Vector2(.3f, 7.4f), walls);
        Barrier("Muralha norte", new Vector2(0, 3.6f), new Vector2(15.6f, .3f), walls);
        Barrier("Muralha sul", new Vector2(0, -3.8f), new Vector2(15.6f, .3f), walls);
        foreach (float x in new[] { -3f, 3f }) foreach (float y in new[] { -1.15f, 1.15f }) Barrier("Sarcófago", new Vector2(x, y), new Vector2(1.15f, 1.35f), walls, tomb);
        Barrier("Altar norte", new Vector2(0, 3f), new Vector2(2f, .4f), walls);
        Barrier("Altar sul", new Vector2(0, -3.1f), new Vector2(2f, .4f), walls);

        for (int side = -1; side <= 1; side += 2)
        {
            foreach (float y in new[] { -2.3f, 0f, 2.3f }) SpriteObject("Tocha da muralha", torch, new Vector2(side * 7.35f, y), Vector2.one * .6f, Color.white, 1);
            foreach (float x in new[] { -5f, -1.5f, 1.5f, 5f }) SpriteObject("Estandarte", banner, new Vector2(x, side > 0 ? 3.25f : -3.45f), Vector2.one * .55f, Color.white, 1);
        }
        var player = SpriteObject("Player", vampire, Vector2.zero, Vector2.one * .88f, Color.white, 3);
        player.tag = "Player";
        var body = player.AddComponent<Rigidbody2D>(); body.gravityScale = 0; body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<CircleCollider2D>().radius = .37f;
        var source = player.AddComponent<AudioSource>(); source.playOnAwake = false; source.clip = Clip("Coleta"); source.volume = .45f;
        player.AddComponent<PlayerMovement>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(player, "Assets/Prefabs/Player.prefab"); UnityEngine.Object.DestroyImmediate(player); PrefabUtility.InstantiatePrefab(prefab);

        var pickup = SpriteObject("Coletavel", vial, Vector2.zero, Vector2.one * .65f, Color.white, 1); pickup.tag = "Coletavel";
        var collider = pickup.AddComponent<PolygonCollider2D>(); collider.isTrigger = true;
        collider.SetPath(0, new[] { new Vector2(0, .45f), new Vector2(.35f, 0), new Vector2(0, -.45f), new Vector2(-.35f, 0) });
        pickup.AddComponent<Collectible>();
        prefab = PrefabUtility.SaveAsPrefabAsset(pickup, "Assets/Prefabs/Coletavel.prefab"); UnityEngine.Object.DestroyImmediate(pickup);
        var pickups = new GameObject("Frascos de sangue").transform;
        Vector2[] points = { new Vector2(-6, 2.4f), new Vector2(-3, 2.5f), new Vector2(0, 2f), new Vector2(3, 2.5f), new Vector2(6, 2.4f), new Vector2(-6, -2.5f), new Vector2(-3, -2.6f), new Vector2(0, -2f), new Vector2(3, -2.6f), new Vector2(6, -2.5f), new Vector2(-6, 0), new Vector2(6, 0) };
        for (int i = 0; i < points.Length; i++) { var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pickups); go.name = $"Frasco {i + 1:00}"; go.transform.position = points[i]; }
        SpawnTemplar("Templário Leste", new Vector2(6, -1.1f), new[] { new Vector2(6, 2.6f), new Vector2(4.5f, 2.6f), new Vector2(4.5f, -2.8f), new Vector2(6, -2.8f) });
        SpawnTemplar("Templário Oeste", new Vector2(-6, 1.1f), new[] { new Vector2(-6, -2.8f), new Vector2(-4.5f, -2.8f), new Vector2(-4.5f, 2.6f), new Vector2(-6, 2.6f) });
        Label(canvas.transform, "Missão", "THE BLOOD WARFARE  /  O CASTELO DOS SEDENTOS", new Vector2(24, 278), new Vector2(900, 22), 14, Mint, TextAnchor.MiddleLeft);
        manager.counterText = Label(canvas.transform, "Frascos", "", new Vector2(24, 247), new Vector2(245, 32), 22, Color.white, TextAnchor.MiddleLeft);
        manager.scoreText = Label(canvas.transform, "Score", "", new Vector2(300, 247), new Vector2(200, 32), 20, Gold, TextAnchor.MiddleLeft);
        manager.timerText = Label(canvas.transform, "Sede e cronômetro", "", new Vector2(615, 247), new Vector2(321, 32), 18, Color.white, TextAnchor.MiddleRight);
        manager.healthText = Label(canvas.transform, "Vida", "", new Vector2(24, -270), new Vector2(220, 28), 17, Red, TextAnchor.MiddleLeft);
        Label(canvas.transform, "Impulso label", "INVESTIDA", new Vector2(260, -270), new Vector2(100, 26), 13, Mint, TextAnchor.MiddleLeft);
        var stamina = Card(canvas.transform, "Stamina", new Vector2(-35, -270), new Vector2(115, 12), new Color(.20f, .14f, .23f));
        var fill = Card(stamina.transform, "Reserva", Vector2.zero, new Vector2(120, 12), Mint);
        manager.staminaBar = stamina.AddComponent<Slider>(); manager.staminaBar.fillRect = fill.GetComponent<RectTransform>(); manager.staminaBar.interactable = false; manager.staminaBar.transition = Selectable.Transition.None; manager.staminaBar.value = 1;
        manager.staminaBar.fillRect.sizeDelta = Vector2.zero; manager.staminaBar.fillRect.anchoredPosition = Vector2.zero;
        manager.statusText = Label(canvas.transform, "Dica", "", new Vector2(520, -270), new Vector2(416, 28), 14, Muted, TextAnchor.MiddleRight);
        manager.victoryPanel = Card(canvas.transform, "Fim de jogo", Vector2.zero, new Vector2(690, 310), new Color32(32, 23, 38, 253));
        manager.resultTitle = Label(manager.victoryPanel.transform, "Título", "", new Vector2(0, 100), new Vector2(650, 44), 32, Mint, TextAnchor.MiddleCenter);
        manager.victoryText = Label(manager.victoryPanel.transform, "Resultado", "", new Vector2(0, 17), new Vector2(650, 100), 19, Color.white, TextAnchor.MiddleCenter);
        manager.restartButton = Button(manager.victoryPanel.transform, "Jogar novamente", new Vector2(-150, -105), new Vector2(260, 50), Mint, Ink);
        UnityEventTools.AddPersistentListener(manager.restartButton.onClick, manager.Restart);
        var menu = Button(manager.victoryPanel.transform, "Menu inicial", new Vector2(150, -105), new Vector2(260, 50), new Color32(81, 54, 72, 255), Color.white); UnityEventTools.AddPersistentListener(menu.onClick, manager.ReturnToMenu);
        manager.victoryPanel.SetActive(false);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Jogo.unity");
    }
    private static void Barrier(string name, Vector2 position, Vector2 scale, Transform parent, Sprite visual = null)
    {
        var go = SpriteObject(name, visual != null ? visual : wall, position, visual != null ? scale : Vector2.one, Color.white, 0, parent);
        if (visual == null) { var renderer = go.GetComponent<SpriteRenderer>(); renderer.drawMode = SpriteDrawMode.Tiled; renderer.size = scale; }
        go.layer = 8; var collider = go.AddComponent<BoxCollider2D>(); collider.size = visual != null ? Vector2.one : scale;
    }
    private static void SpawnTemplar(string name, Vector2 position, Vector2[] patrol)
    {
        var go = SpriteObject(name, templar, position, Vector2.one * .75f, Color.white, 2);
        var body = go.AddComponent<Rigidbody2D>(); body.gravityScale = 0; body.constraints = RigidbodyConstraints2D.FreezeRotation; body.interpolation = RigidbodyInterpolation2D.Interpolate;
        go.AddComponent<CircleCollider2D>().radius = .38f;
        var enemy = go.AddComponent<TemplarGuard>(); enemy.patrolPoints = patrol;
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/" + (position.x > 0 ? "TemplarLeste" : "TemplarOeste") + ".prefab");
        UnityEngine.Object.DestroyImmediate(go); PrefabUtility.InstantiatePrefab(prefab);
    }
    private static void Menu()
    {
        var camera = NewScene("MenuInicial"); camera.gameObject.AddComponent<ArenaCamera>();
        var canvas = NewCanvas(camera); Soundtrack("ComingHome_8bit"); var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
        Label(canvas.transform, "Setor", "8-BIT  /  CASTELO SOB CERCO  /  ATO I", new Vector2(48, 255), new Vector2(860, 26), 14, Mint, TextAnchor.MiddleLeft);
        Label(canvas.transform, "Título", "THE BLOOD\nWARFARE", new Vector2(48, 145), new Vector2(610, 150), 60, Color.white, TextAnchor.MiddleLeft);
        Label(canvas.transform, "Subtítulo", "A NOITE PERTENCE AOS SEDENTOS", new Vector2(52, 47), new Vector2(650, 30), 17, Mint, TextAnchor.MiddleLeft);
        Label(canvas.transform, "Missão", "Os templários invadiram o castelo. Recolha os 12 frascos\nde sangue antes que a sede consuma sua eternidade.", new Vector2(52, -20), new Vector2(740, 65), 18, Muted, TextAnchor.MiddleLeft);
        Label(canvas.transform, "Regras", "45s de reserva  •  +3s por frasco  •  3 vidas\nTemplários patrulham os salões e perseguem invasores.", new Vector2(52, -93), new Vector2(810, 60), 16, Gold, TextAnchor.MiddleLeft);
        menu.playButton = Button(canvas.transform, "INICIAR A CAÇADA", new Vector2(-265, -171), new Vector2(325, 55), Mint, Ink); UnityEventTools.AddPersistentListener(menu.playButton.onClick, menu.Play);
        Label(canvas.transform, "Controles", "WASD / SETAS  mover   •   SHIFT  investida   •   ESC  menu\nCONTROLE: analógico / direcional  •  RB investida  •  A confirmar  •  VIEW menu", new Vector2(52, -246), new Vector2(860, 55), 14, Muted, TextAnchor.MiddleLeft);
        Label(canvas.transform, "Créditos", "MATHIAS ZANOTTO  •  PROJETO INTRO UNITY  •  INSPER", new Vector2(52, -287), new Vector2(860, 18), 11, Muted, TextAnchor.MiddleLeft);
        SpriteObject("Lua de sangue", moon, new Vector2(5.7f, 2.6f), Vector2.one * 2.4f, Color.white, -3);
        SpriteObject("Arco do castelo", arch, new Vector2(5.4f, .8f), new Vector2(3.2f, 4.3f), Color.white, -1);
        SpriteObject("Vampiro ilustrado", vampire, new Vector2(5.4f, 1.3f), Vector2.one * 2.3f, Color.white, 0);
        SpriteObject("Frasco ilustrado", vial, new Vector2(6.9f, .5f), Vector2.one * .85f, Color.white, 0);
        SpriteObject("Templário ilustrado", templar, new Vector2(5.9f, -1.4f), Vector2.one * 1.1f, Color.white, 0);
        SpriteObject("Morcego", bat, new Vector2(4.1f, 2.7f), Vector2.one * 1.2f, Color.white, 0);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/MenuInicial.unity");
    }
    [MenuItem("Blood Warfare/Compilar WebGL")]
    public static void BuildWebGL()
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var uiCulture = System.Globalization.CultureInfo.CurrentUICulture;
        var invariantEnvironment = Environment.GetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT");
        try
        {
            // Build arguments must use a decimal point, independently of the editor's regional settings.
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
            Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1");
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.template = "PROJECT:Resgate";
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/Scenes/MenuInicial.unity", "Assets/Scenes/Jogo.unity" }, locationPathName = "Builds/WebGL", target = BuildTarget.WebGL, options = BuildOptions.None });
            Directory.CreateDirectory("Documentation");
            File.WriteAllText("Documentation/webgl-build.json", "{\"title\":\"The Blood Warfare\",\"success\":" + (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? "true" : "false") + ",\"result\":\"" + report.summary.result + "\",\"errors\":" + report.summary.totalErrors + "}");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("WebGL build failed: " + report.summary.result);
            File.WriteAllText("Builds/WebGL/ThirdPartyNotices.txt", File.ReadAllText("CREDITS.md") + "\n\n" + File.ReadAllText("Assets/Fonts/LICENSE-DejaVu.txt"));
            Debug.Log("RESGATE_WEBGL_OK");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = culture;
            System.Globalization.CultureInfo.CurrentUICulture = uiCulture;
            Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", invariantEnvironment);
        }
    }
}






