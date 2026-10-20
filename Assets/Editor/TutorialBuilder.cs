using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class TutorialBuilder
{
    internal static readonly Color Ink = new Color32(22, 18, 30, 255);
    internal static readonly Color Panel = new Color32(43, 34, 48, 255);
    internal static readonly Color Muted = new Color32(182, 168, 178, 255);
    internal static readonly Color Mint = new Color32(230, 194, 115, 255);
    internal static readonly Color Red = new Color32(239, 82, 104, 255);
    internal static readonly Color Gold = new Color32(244, 209, 138, 255);
    internal static Font font;
    internal static Sprite circle, square, triangle;
    internal static Material spriteMaterial;

    // Historical tutorial generator; use the Blood Warfare menu for the final project.
    public static void BuildProject()
    {
        foreach (string folder in new[] { "Scenes", "Prefabs", "Art", "Audio" })
            Directory.CreateDirectory("Assets/" + folder);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        circle = MakeSprite("Circle", 0);
        square = MakeSprite("Square", 1);
        triangle = MakeSprite("Triangle", 2);
        spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/SpriteUnlit.mat");
        if (spriteMaterial == null)
        {
            spriteMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            AssetDatabase.CreateAsset(spriteMaterial, "Assets/Art/SpriteUnlit.mat");
        }
        WritePickupSound();
        AddTag("Coletavel");
        PlayerSettings.companyName = "Estudo Unity";
        PlayerSettings.productName = "Introdução Rápida • Coleta 2D";
        PlayerSettings.defaultScreenWidth = 960;
        PlayerSettings.defaultScreenHeight = 600;
        PlayerSettings.runInBackground = true;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        BuildGame();
        BuildMenu();
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene("Assets/Scenes/MenuInicial.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Jogo.unity", true)
        };
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/MenuInicial.unity");
        Debug.Log("TUTORIAL_BUILD_OK: MenuInicial, Jogo, Player and Coletavel prefabs created.");
    }

    internal static Camera NewScene(string name)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Ink;
        camera.transform.position = new Vector3(0, 0, -10);
        camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        return camera;
    }

    internal static Canvas NewCanvas(Camera camera)
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 5;
        canvas.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 600);
        scaler.matchWidthOrHeight = .5f;
        return canvas;
    }

    internal static void BuildGame()
    {
        var camera = NewScene("Jogo");
        var canvas = NewCanvas(camera);
        var manager = new GameObject("GameManager").AddComponent<GameManager>();
        var walls = new GameObject("Paredes").transform;
        Wall("Parede esquerda", new Vector2(-7.65f, -.1f), new Vector2(.3f, 7.4f), walls);
        Wall("Parede direita", new Vector2(7.65f, -.1f), new Vector2(.3f, 7.4f), walls);
        Wall("Parede superior", new Vector2(0, 3.6f), new Vector2(15.6f, .3f), walls);
        Wall("Parede inferior", new Vector2(0, -3.8f), new Vector2(15.6f, .3f), walls);
        var grid = new GameObject("Grade visual").transform;
        for (int x = -7; x <= 7; x++) SpriteObject("Linha vertical", square, new Vector2(x, -.1f), new Vector2(.012f, 7.1f), new Color32(31, 43, 58, 255), -2, grid);
        for (int y = -3; y <= 3; y++) SpriteObject("Linha horizontal", square, new Vector2(0, y), new Vector2(15.1f, .012f), new Color32(31, 43, 58, 255), -2, grid);

        var player = SpriteObject("Player", circle, Vector2.zero, Vector2.one * .7f, Red, 2);
        player.tag = "Player";
        var body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<CircleCollider2D>().radius = .49f;
        var audio = player.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.loop = false;
        audio.spatialBlend = 0;
        audio.volume = .35f;
        audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Coleta.wav");
        player.AddComponent<PlayerMovement>();
        var playerPrefab = PrefabUtility.SaveAsPrefabAsset(player, "Assets/Prefabs/Player.prefab");
        UnityEngine.Object.DestroyImmediate(player);
        PrefabUtility.InstantiatePrefab(playerPrefab);

        var pickup = SpriteObject("Coletavel", triangle, Vector2.zero, Vector2.one * .42f, Gold, 1);
        pickup.tag = "Coletavel";
        var polygon = pickup.AddComponent<PolygonCollider2D>();
        polygon.SetPath(0, new[] { new Vector2(0, .48f), new Vector2(-.48f, -.48f), new Vector2(.48f, -.48f) });
        polygon.isTrigger = true;
        pickup.AddComponent<Collectible>();
        var pickupPrefab = PrefabUtility.SaveAsPrefabAsset(pickup, "Assets/Prefabs/Coletavel.prefab");
        UnityEngine.Object.DestroyImmediate(pickup);
        var parent = new GameObject("Coletáveis").transform;
        Vector2[] points = { new Vector2(-6, 2.4f), new Vector2(-3, 2.4f), new Vector2(0, 2.4f), new Vector2(3, 2.4f), new Vector2(6, 2.4f),
            new Vector2(-6, -2.5f), new Vector2(-3, -2.5f), new Vector2(0, -2.5f), new Vector2(3, -2.5f), new Vector2(6, -2.5f), new Vector2(-5, 0), new Vector2(5, 0) };
        for (int i = 0; i < points.Length; i++)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(pickupPrefab, parent);
            instance.name = $"Coletavel {i + 1:00}";
            instance.transform.position = points[i];
        }
        Label(canvas.transform, "Label", "INTRODUÇÃO RÁPIDA  /  UNITY 2D", new Vector2(40, 268), new Vector2(880, 24), 14, Mint, TextAnchor.MiddleLeft);
        manager.counterText = Label(canvas.transform, "Contador", "COLETADOS  00 / 12", new Vector2(40, 234), new Vector2(600, 36), 26, Color.white, TextAnchor.MiddleLeft);
        manager.timerText = Label(canvas.transform, "Tempo", "TEMPO  000.0s", new Vector2(680, 234), new Vector2(240, 36), 19, Muted, TextAnchor.MiddleRight);
        Label(canvas.transform, "Instruções", "WASD / SETAS  mover     •     Colete todos os triângulos     •     ESC  menu", new Vector2(40, -270), new Vector2(880, 32), 16, Muted, TextAnchor.MiddleCenter);
        manager.victoryPanel = Card(canvas.transform, "Fim de jogo", Vector2.zero, new Vector2(600, 280), Panel);
        Label(manager.victoryPanel.transform, "Título", "VOCÊ CONSEGUIU!", new Vector2(0, 83), new Vector2(550, 40), 32, Mint, TextAnchor.MiddleCenter);
        manager.victoryText = Label(manager.victoryPanel.transform, "Resultado", "", new Vector2(0, 15), new Vector2(550, 60), 19, Color.white, TextAnchor.MiddleCenter);
        var restart = Button(manager.victoryPanel.transform, "Jogar novamente", new Vector2(-130, -83), new Vector2(230, 48), Mint, Ink);
        UnityEventTools.AddPersistentListener(restart.onClick, manager.Restart);
        var menu = Button(manager.victoryPanel.transform, "Menu inicial", new Vector2(130, -83), new Vector2(230, 48), new Color32(57, 73, 91, 255), Color.white);
        UnityEventTools.AddPersistentListener(menu.onClick, manager.ReturnToMenu);
        manager.victoryPanel.SetActive(false);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Jogo.unity");
    }

    internal static void BuildMenu()
    {
        var camera = NewScene("MenuInicial");
        var canvas = NewCanvas(camera);
        var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
        Label(canvas.transform, "Eyebrow", "PRIMEIRO PROJETO  /  UNITY 2D", new Vector2(70, 233), new Vector2(820, 26), 16, Mint, TextAnchor.MiddleLeft);
        Label(canvas.transform, "Título", "COLETA\n2D", new Vector2(70, 87), new Vector2(520, 230), 92, Color.white, TextAnchor.MiddleLeft);
        Label(canvas.transform, "Descrição", "Um círculo. Doze triângulos.\nExplore a arena e colete todos eles.", new Vector2(75, -72), new Vector2(500, 65), 22, Muted, TextAnchor.MiddleLeft);
        var play = Button(canvas.transform, "JOGAR", new Vector2(-280, -174), new Vector2(250, 56), Mint, Ink);
        UnityEventTools.AddPersistentListener(play.onClick, menu.Play);
        var quit = Button(canvas.transform, "SAIR", new Vector2(-83, -174), new Vector2(110, 56), Panel, Muted);
        UnityEventTools.AddPersistentListener(quit.onClick, menu.Quit);
        Label(canvas.transform, "Atalho", "ENTER  jogar     •     WASD / SETAS  mover", new Vector2(75, -252), new Vector2(810, 28), 15, Muted, TextAnchor.MiddleLeft);
        SpriteObject("Jogador ilustrativo", circle, new Vector2(4.7f, .3f), Vector2.one * 1.65f, Red, 0);
        SpriteObject("Coletável ilustrativo 1", triangle, new Vector2(4.7f, 2.35f), Vector2.one * .62f, Gold, 0);
        SpriteObject("Coletável ilustrativo 2", triangle, new Vector2(6.5f, -.75f), Vector2.one * .62f, Gold, 0);
        SpriteObject("Coletável ilustrativo 3", triangle, new Vector2(2.85f, -.75f), Vector2.one * .62f, Gold, 0);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/MenuInicial.unity");
    }

    internal static GameObject SpriteObject(string name, Sprite sprite, Vector2 position, Vector2 scale, Color color, int order, Transform parent = null)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = new Vector3(scale.x, scale.y, 1);
        var renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sharedMaterial = spriteMaterial;
        renderer.sortingOrder = order;
        return go;
    }
    internal static void Wall(string name, Vector2 position, Vector2 scale, Transform parent)
    {
        var wall = SpriteObject(name, square, position, scale, new Color32(89, 119, 143, 255), 0, parent);
        wall.AddComponent<BoxCollider2D>();
    }
    internal static RectTransform Rect(GameObject go, Transform parent, Vector2 position, Vector2 size, bool left = false)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f);
        rt.pivot = left ? new Vector2(0, .5f) : new Vector2(.5f, .5f);
        rt.anchoredPosition = left ? new Vector2(position.x - 480, position.y) : position;
        rt.sizeDelta = size;
        return rt;
    }
    internal static Text Label(Transform parent, string name, string text, Vector2 pos, Vector2 size, int fontSize, Color color, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        bool left = align == TextAnchor.MiddleLeft || align == TextAnchor.MiddleRight;
        Rect(go, parent, pos, size, left);
        var label = go.GetComponent<Text>();
        label.font = font;
        label.fontSize = fontSize;
        label.text = text;
        label.color = color;
        label.alignment = align;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        return label;
    }
    internal static GameObject Card(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Rect(go, parent, pos, size);
        go.GetComponent<Image>().color = color;
        return go;
    }
    internal static Button Button(Transform parent, string text, Vector2 pos, Vector2 size, Color color, Color textColor)
    {
        var go = Card(parent, text, pos, size, color);
        var button = go.AddComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        var colors = button.colors;
        colors.highlightedColor = new Color(.8f, 1f, .94f);
        colors.pressedColor = new Color(.65f, .8f, .75f);
        button.colors = colors;
        Label(go.transform, "Texto", text, Vector2.zero, size, 19, textColor, TextAnchor.MiddleCenter);
        return button;
    }
    internal static Sprite MakeSprite(string name, int shape)
    {
        const int n = 128;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
        {
            float px = (x + .5f) / n - .5f, py = (y + .5f) / n - .5f;
            bool inside = shape == 1 || (shape == 0 ? px * px + py * py < .245f : py >= -.49f && py <= .49f && Mathf.Abs(px) <= (.49f - py) * .5f);
            tex.SetPixel(x, y, inside ? Color.white : Color.clear);
        }
        tex.Apply();
        string path = "Assets/Art/" + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = n;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    internal static void WritePickupSound()
    {
        const int rate = 44100, count = 7938;
        using (var writer = new BinaryWriter(File.Create("Assets/Audio/Coleta.wav")))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
            writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2);
            writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
            for (int i = 0; i < count; i++)
            {
                double t = (double)i / rate, envelope = Math.Sin(Math.PI * i / count) * Math.Exp(-18 * t);
                double phase = 2 * Math.PI * (660 * t + 1400 * t * t);
                writer.Write((short)(15000 * envelope * Math.Sin(phase)));
            }
        }
        AssetDatabase.ImportAsset("Assets/Audio/Coleta.wav");
    }
    internal static void AddTag(string tag)
    {
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = settings.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++) if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        settings.ApplyModifiedProperties();
    }

    public static void BuildWindows()
    {
        Directory.CreateDirectory("Builds/Windows");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/MenuInicial.unity", "Assets/Scenes/Jogo.unity" },
            locationPathName = "Builds/Windows/Coleta2D.exe", target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("Windows build failed: " + report.summary.result);
        Debug.Log("TUTORIAL_WINDOWS_BUILD_OK");
    }
}



