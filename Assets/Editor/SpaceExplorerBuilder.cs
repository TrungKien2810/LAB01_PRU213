using System;
using SpaceExplorer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Generates every prefab and scene of Space Explorer from the art in Assets/Art.
/// Menu: Space Explorer > Build Game. Re-running it overwrites the generated assets, so the
/// game can always be rebuilt from code (art swaps, layout tweaks) instead of hand-editing scenes.
/// </summary>
public static class SpaceExplorerBuilder
{
    const string ArtRoot = "Assets/Art/";
    const string PrefabFolder = "Assets/Prefabs";
    const string SceneFolder = "Assets/Scenes";
    static readonly Vector2 ReferenceResolution = new(1920, 1080);

    // Sorting orders, back to front.
    const int OrderBackground = -100, OrderStar = 4, OrderAsteroid = 5, OrderBoss = 8,
              OrderThruster = 9, OrderShip = 10, OrderLaser = 15, OrderBossBullet = 16, OrderExplosion = 30;

    /// <summary>Handles to the prefabs built in one pass, so the scenes can reference them.</summary>
    sealed class Prefabs
    {
        public GameObject Explosion, Laser, BossBullet, Asteroid, Star, Boss, Player;
    }

    /// <summary>
    /// Rebuilds every prefab, regenerates the three scenes and rewrites Build Settings.
    /// Entry point of the <b>Space Explorer &gt; Build Game</b> menu item.
    /// </summary>
    [MenuItem("Space Explorer/Build Game")]
    public static void BuildGame()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(SceneFolder);

        Prefabs prefabs = BuildPrefabs();
        string menu = BuildMainMenuScene();
        string gameplay = BuildGameplayScene(prefabs);
        string end = BuildEndGameScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(menu, true),
            new EditorBuildSettingsScene(gameplay, true),
            new EditorBuildSettingsScene(end, true),
        };
        EditorSceneManager.OpenScene(menu);
        Debug.Log("[SpaceExplorer] Prefabs, scenes and Build Settings generated.");
    }

    // ───────────────────────────── Prefabs ─────────────────────────────

    static Prefabs BuildPrefabs()
    {
        var p = new Prefabs();

        p.Explosion = SavePrefab(BuildExplosion(), "Explosion");
        p.Laser = SavePrefab(BuildLaser(), "Laser");
        p.BossBullet = SavePrefab(BuildBossBullet(), "BossBullet");
        p.Asteroid = SavePrefab(BuildAsteroid(p), "Asteroid");
        p.Star = SavePrefab(BuildStar(), "Star");
        p.Boss = SavePrefab(BuildBoss(p), "Boss");
        p.Player = SavePrefab(BuildPlayer(p), "Player");
        return p;
    }

    static GameObject BuildExplosion()
    {
        GameObject go = SpriteObject("Explosion", Frames("Sprites/explosion/explosion_", 6)[0], OrderExplosion);
        AddAnimator(go, Frames("Sprites/explosion/explosion_", 6), 14f, loop: false, destroyOnComplete: true);
        return go;
    }

    static GameObject BuildLaser()
    {
        GameObject go = SpriteObject("Laser", LoadSprite("Sprites/laser.png"), OrderLaser);
        AddTriggerBody(go).gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
        go.GetComponent<BoxCollider2D>().size = new Vector2(0.12f, 0.5f);
        Set(go.AddComponent<Projectile>(), ("faction", Faction.Player), ("speed", 14f), ("lifetime", 1.5f));
        return go;
    }

    static GameObject BuildBossBullet()
    {
        Sprite[] frames = Frames("Sprites/boss-bullet/boss-bullet_", 4);
        GameObject go = SpriteObject("BossBullet", frames[0], OrderBossBullet);
        AddAnimator(go, frames, 12f);
        AddTriggerBody(go);
        AddCircleTrigger(go, 0.18f);
        Set(go.AddComponent<Projectile>(), ("faction", Faction.Enemy), ("speed", 4.5f), ("lifetime", 6f));
        return go;
    }

    static GameObject BuildAsteroid(Prefabs p)
    {
        Sprite[] variants = Frames("Sprites/asteroids/asteroids_", 4);
        GameObject go = SpriteObject("Asteroid", variants[0], OrderAsteroid);
        AddTriggerBody(go);
        AddCircleTrigger(go, 0.42f);
        Set(go.AddComponent<Asteroid>(), ("variants", variants), ("explosionPrefab", p.Explosion));
        go.AddComponent<OffscreenDespawner>();
        return go;
    }

    static GameObject BuildStar()
    {
        Sprite[] frames = Frames("Sprites/star/star_", 4);
        GameObject go = SpriteObject("Star", frames[0], OrderStar);
        AddAnimator(go, frames, 8f, randomStartFrame: true);
        AddTriggerBody(go);
        AddCircleTrigger(go, 0.26f);
        go.AddComponent<Star>();
        go.AddComponent<OffscreenDespawner>();
        return go;
    }

    static GameObject BuildBoss(Prefabs p)
    {
        Sprite[] frames = Frames("Sprites/boss/boss_", 4);
        GameObject go = SpriteObject("Boss", frames[0], OrderBoss);
        AddAnimator(go, frames, 8f);
        AddTriggerBody(go);
        go.AddComponent<PolygonCollider2D>().isTrigger = true; // shape generated from the sprite outline

        Transform muzzle = Child(go, "Muzzle", new Vector2(0f, -1f));
        Set(go.AddComponent<Boss>(),
            ("bulletPrefab", p.BossBullet.GetComponent<Projectile>()),
            ("muzzle", muzzle),
            ("hitSprite", LoadSprite("Sprites/boss/boss-hit.png")),
            ("explosionPrefab", p.Explosion));
        return go;
    }

    static GameObject BuildPlayer(Prefabs p)
    {
        GameObject go = SpriteObject("Player", LoadSprite("Sprites/ship.png"), OrderShip);
        Sprite[] flame = Frames("Sprites/ship-thruster/ship-thruster_", 4);
        GameObject thruster = SpriteObject("Thruster", flame[0], OrderThruster, go.transform);
        thruster.transform.localPosition = new Vector3(0f, -0.62f, 0f);
        AddAnimator(thruster, flame, 14f);
        Transform muzzle = Child(go, "Muzzle", new Vector2(0f, 0.6f));

        AddTriggerBody(go).freezeRotation = true;
        AddCircleTrigger(go, 0.38f);
        go.AddComponent<PlayerInputReader>();
        go.AddComponent<PlayerMovement>();
        Set(go.AddComponent<PlayerWeapon>(), ("laserPrefab", p.Laser.GetComponent<Projectile>()), ("muzzle", muzzle));
        Set(go.AddComponent<PlayerHealth>(),
            ("blinkRenderers", new Object[] { go.GetComponent<SpriteRenderer>(), thruster.GetComponent<SpriteRenderer>() }),
            ("explosionPrefab", p.Explosion));
        return go;
    }

    // ───────────────────────────── Scenes ─────────────────────────────

    static string BuildMainMenuScene()
    {
        NewScene();
        Transform canvas = CreateCanvas("Menu Canvas");
        FullScreenImage(canvas, "Background", LoadSprite("Backgrounds/bg-menu.png"));

        CreateImage(canvas, "Title Plate", LoadSprite("UI/ui-title-plate.png"), Top, new Vector2(0, -190), new Vector2(1000, 247));
        CreateImage(canvas, "Logo", LoadSprite("UI/logo.png"), Top, new Vector2(0, -190), new Vector2(720, 270));

        Button play = MenuButton(canvas, "Play Button", "PLAY", "UI/ui-icon-play.png", new Vector2(0, -40));
        Button info = MenuButton(canvas, "Instructions Button", "INSTRUCTIONS", "UI/ui-icon-info.png", new Vector2(0, -170));
        Button quit = MenuButton(canvas, "Quit Button", "QUIT", "UI/ui-icon-quit.png", new Vector2(0, -300));

        (GameObject panel, Button close) = InstructionsPanel(canvas);

        var ui = new GameObject("Main Menu UI").AddComponent<MainMenuUI>();
        Set(ui, ("playButton", play), ("instructionsButton", info), ("quitButton", quit),
            ("instructionsPanel", panel), ("closeInstructionsButton", close));
        return SaveScene(SceneNames.MainMenu);
    }

    static (GameObject panel, Button close) InstructionsPanel(Transform canvas)
    {
        // Full-screen dimmer doubles as a click blocker for the menu behind it.
        Image dim = CreateImage(canvas, "Instructions Panel", null, Center, Vector2.zero, Vector2.zero);
        Stretch(dim.rectTransform);
        dim.color = new Color(0f, 0f, 0f, 0.65f);

        Image frame = CreateImage(dim.transform, "Frame", LoadSprite("UI/panel.png"), Center, Vector2.zero, new Vector2(920, 700), preserveAspect: false);
        frame.type = Image.Type.Sliced;

        Label(frame.transform, "Title", "HOW TO PLAY", 46, Top, new Vector2(0, -105), new Vector2(700, 70), new Color(0.25f, 0.88f, 1f));
        CreateImage(frame.transform, "Arrow Keys", LoadSprite("UI/ui-key-arrows.png"), Center, new Vector2(-220, 95), new Vector2(160, 102));
        Label(frame.transform, "Move Text", "Move the ship", 32, Center, new Vector2(110, 95), new Vector2(420, 60), Color.white, TextAnchor.MiddleLeft);
        CreateImage(frame.transform, "Space Key", LoadSprite("UI/ui-key-space.png"), Center, new Vector2(-220, -20), new Vector2(217, 64));
        Label(frame.transform, "Shoot Text", "Fire lasers", 32, Center, new Vector2(110, -20), new Vector2(420, 60), Color.white, TextAnchor.MiddleLeft);
        Label(frame.transform, "Rules", "Collect stars  +50     Shoot asteroids  +10\n" +
                                       "Hit by an asteroid  -50 and lose a life\n" +
                                       "Reach 400 points to face the boss!",
              28, Center, new Vector2(0, -180), new Vector2(780, 160), new Color(1f, 0.95f, 0.66f));

        Button close = IconButton(frame.transform, "Close Button", "UI/ui-icon-close.png", TopRight, new Vector2(-95, -100));
        return (dim.gameObject, close);
    }

    static string BuildGameplayScene(Prefabs prefabs)
    {
        NewScene();

        GameObject background = SpriteObject("Background", LoadSprite("Backgrounds/bg-gameplay.png"), OrderBackground);
        background.AddComponent<ScrollingBackground>();

        var player = ((GameObject)PrefabUtility.InstantiatePrefab(prefabs.Player)).GetComponent<PlayerHealth>();
        player.transform.position = new Vector3(0f, -3.5f, 0f);

        Spawner asteroids = CreateSpawner("Asteroid Spawner", prefabs.Asteroid, new Vector2(0.7f, 1.4f));
        CreateSpawner("Star Spawner", prefabs.Star, new Vector2(1.5f, 3f));

        var systems = new GameObject("Game Systems");
        var score = systems.AddComponent<ScoreKeeper>();
        Set(systems.AddComponent<GameplayController>(), ("player", player), ("scoreKeeper", score));
        var director = systems.AddComponent<BossDirector>();
        Set(director, ("bossPrefab", prefabs.Boss.GetComponent<Boss>()), ("scoreKeeper", score), ("asteroidSpawner", asteroids));

        BuildHud(score, player, director);
        return SaveScene(SceneNames.Gameplay);
    }

    static void BuildHud(ScoreKeeper score, PlayerHealth player, BossDirector director)
    {
        Transform canvas = CreateCanvas("HUD Canvas");

        Image plate = CreateImage(canvas, "Score Plate", LoadSprite("UI/ui-score-plate.png"), TopLeft, new Vector2(190, -75), new Vector2(330, 132));
        Text scoreText = Label(plate.transform, "Score Text", "SCORE  0", 34, Center, Vector2.zero, new Vector2(300, 60), new Color(1f, 0.95f, 0.66f));
        Text livesText = Label(canvas, "Lives Text", "LIVES  3", 30, TopLeft, new Vector2(190, -160), new Vector2(300, 50), new Color(0.25f, 0.88f, 1f));

        Image warning = CreateImage(canvas, "Warning Banner", LoadSprite("UI/ui-warning.png"), Center, new Vector2(0, 120), new Vector2(560, 305));

        // Boss bar: fill sits behind the frame, sized to the frame's inner opening (x 41..275, y 18..40 of 290x58).
        const float barScale = 2.4f;
        RectTransform bar = UIObject(canvas, "Boss Health Bar", Top, new Vector2(0, -70), new Vector2(290, 58) * barScale);
        Image fill = CreateImage(bar, "Fill", LoadSprite("UI/boss-hpbar-fill.png"), Center, new Vector2(13, 0) * barScale, new Vector2(234, 22) * barScale, preserveAspect: false);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        CreateImage(bar, "Frame", LoadSprite("UI/boss-hpbar-frame.png"), Center, Vector2.zero, new Vector2(290, 58) * barScale);
        var bossBar = bar.gameObject.AddComponent<BossHealthBarUI>();
        Set(bossBar, ("fill", fill));

        Set(canvas.gameObject.AddComponent<HudUI>(),
            ("scoreKeeper", score), ("player", player), ("bossDirector", director),
            ("scoreText", scoreText), ("livesText", livesText),
            ("warningBanner", warning.gameObject), ("bossHealthBar", bossBar));
    }

    static string BuildEndGameScene()
    {
        NewScene();
        Transform canvas = CreateCanvas("End Canvas");
        FullScreenImage(canvas, "Background", LoadSprite("Backgrounds/bg-end.png"));

        CreateImage(canvas, "Game Over", LoadSprite("UI/ui-gameover.png"), Top, new Vector2(0, -210), new Vector2(540, 307));

        Image plate = CreateImage(canvas, "Score Plate", LoadSprite("UI/ui-score-plate.png"), Center, new Vector2(0, 0), new Vector2(560, 223));
        Text score = Label(plate.transform, "Score Text", "SCORE  0", 48, Center, new Vector2(0, 28), new Vector2(500, 70), new Color(1f, 0.95f, 0.66f));
        Text best = Label(plate.transform, "Best Text", "BEST  0", 32, Center, new Vector2(0, -38), new Vector2(500, 50), new Color(0.25f, 0.88f, 1f));

        Image badge = CreateImage(canvas, "New Record Badge", LoadSprite("UI/ui-new-record.png"), Center, new Vector2(300, 110), new Vector2(200, 120));
        badge.rectTransform.localRotation = Quaternion.Euler(0, 0, -12f);

        Image medal = CreateImage(canvas, "Medal", LoadSprite("UI/medals/ui-medals_0.png"), Center, new Vector2(-420, 0), new Vector2(300, 300)); // art fills ~half of its 128px cell

        // Replay sits alone on the upper row: it is what a player who just died wants next.
        Button replay = MenuButton(canvas, "Replay Button", "REPLAY", "UI/ui-icon-play.png", new Vector2(0, -270));
        Button home = MenuButton(canvas, "Home Button", "MAIN MENU", "UI/ui-icon-home.png", new Vector2(-290, -395));
        Button quit = MenuButton(canvas, "Quit Button", "QUIT", "UI/ui-icon-quit.png", new Vector2(290, -395));

        Set(new GameObject("End Game UI").AddComponent<EndGameUI>(),
            ("scoreText", score), ("bestScoreText", best), ("newRecordBadge", badge.gameObject),
            ("medalImage", medal), ("medalSprites", Frames("UI/medals/ui-medals_", 3)),
            ("replayButton", replay), ("homeButton", home), ("quitButton", quit));
        return SaveScene(SceneNames.EndGame);
    }

    // ───────────────────────────── Scene helpers ─────────────────────────────

    static void NewScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
        cameraGo.transform.position = new Vector3(0f, 0f, -10f);
        var cam = cameraGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.03f, 0.02f, 0.09f);
        cameraGo.AddComponent<AudioListener>();

        // URP 2D renderer: lit sprites are black without a global light.
        new GameObject("Global Light 2D").AddComponent<Light2D>().lightType = Light2D.LightType.Global;
    }

    static string SaveScene(string name)
    {
        string path = $"{SceneFolder}/{name}.unity";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
        return path;
    }

    static Spawner CreateSpawner(string name, GameObject prefab, Vector2 interval)
    {
        var spawner = new GameObject(name).AddComponent<Spawner>();
        Set(spawner, ("prefab", prefab), ("intervalRange", interval));
        return spawner;
    }

    // ───────────────────────────── Prefab helpers ─────────────────────────────

    static GameObject SavePrefab(GameObject root, string name)
    {
        GameObject asset = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{name}.prefab");
        Object.DestroyImmediate(root);
        return asset;
    }

    static GameObject SpriteObject(string name, Sprite sprite, int sortingOrder, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent) go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    static Transform Child(GameObject parent, string name, Vector2 localPosition)
    {
        var child = new GameObject(name).transform;
        child.SetParent(parent.transform, false);
        child.localPosition = localPosition;
        return child;
    }

    /// <summary>Gravity-free dynamic body: moved by velocity / MovePosition, reports triggers with everything.</summary>
    static Rigidbody2D AddTriggerBody(GameObject go)
    {
        var body = go.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        return body;
    }

    static void AddCircleTrigger(GameObject go, float radius)
    {
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = radius;
    }

    static void AddAnimator(GameObject go, Sprite[] frames, float fps, bool loop = true, bool destroyOnComplete = false, bool randomStartFrame = false)
    {
        Set(go.AddComponent<SpriteAnimator>(), ("frames", frames), ("framesPerSecond", fps), ("loop", loop),
            ("destroyOnComplete", destroyOnComplete), ("randomStartFrame", randomStartFrame));
    }

    // ───────────────────────────── UI helpers ─────────────────────────────

    static readonly Vector2 Center = new(0.5f, 0.5f), Top = new(0.5f, 1f), TopLeft = new(0f, 1f), TopRight = new(1f, 1f);

    static Transform CreateCanvas(string name)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        if (!Object.FindAnyObjectByType<EventSystem>())
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        return go.transform;
    }

    static RectTransform UIObject(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 position, Vector2 size, bool preserveAspect = true)
    {
        var image = UIObject(parent, name, anchor, position, size).gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = preserveAspect && sprite;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>Background that always covers the screen, cropping instead of stretching.</summary>
    static void FullScreenImage(Transform canvas, string name, Sprite sprite)
    {
        Image image = CreateImage(canvas, name, sprite, Center, Vector2.zero, ReferenceResolution, preserveAspect: false);
        var fitter = image.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
    }

    static Text Label(Transform parent, string name, string text, int fontSize, Vector2 anchor, Vector2 position, Vector2 size,
                      Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        var label = UIObject(parent, name, anchor, position, size).gameObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        var outline = label.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);
        return label;
    }

    static Button MenuButton(Transform parent, string name, string text, string iconPath, Vector2 position)
    {
        Image background = CreateImage(parent, name, LoadSprite("UI/button.png"), Center, position, new Vector2(500, 110), preserveAspect: false);
        background.type = Image.Type.Sliced;
        background.pixelsPerUnitMultiplier = 0.55f; // draw the 9-slice caps ~1.8x so they match the button's scale
        background.raycastTarget = true;

        var button = background.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = new SpriteState
        {
            highlightedSprite = LoadSprite("UI/button-hover.png"),
            selectedSprite = LoadSprite("UI/button-hover.png"),
            pressedSprite = LoadSprite("UI/button-pressed.png"),
        };

        CreateImage(background.transform, "Icon", LoadSprite(iconPath), Center, new Vector2(-170, 0), new Vector2(72, 72));
        Label(background.transform, "Label", text, 38, Center, new Vector2(30, 0), new Vector2(360, 80), Color.white);
        return button;
    }

    static Button IconButton(Transform parent, string name, string iconPath, Vector2 anchor, Vector2 position)
    {
        Image icon = CreateImage(parent, name, LoadSprite(iconPath), anchor, position, new Vector2(80, 80));
        icon.raycastTarget = true;
        var button = icon.gameObject.AddComponent<Button>();
        button.colors = new ColorBlock
        {
            normalColor = Color.white, highlightedColor = new Color(1f, 0.85f, 0.85f), pressedColor = new Color(0.7f, 0.7f, 0.7f),
            selectedColor = Color.white, disabledColor = Color.gray, colorMultiplier = 1f, fadeDuration = 0.08f,
        };
        return button;
    }

    // ───────────────────────────── Asset + serialization helpers ─────────────────────────────

    static Sprite LoadSprite(string relativePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + relativePath);
        if (!sprite) throw new InvalidOperationException($"Sprite not found: {ArtRoot}{relativePath}");
        return sprite;
    }

    /// <summary>Loads numbered frames "{prefix}0.png" .. "{prefix}{count-1}.png".</summary>
    static Sprite[] Frames(string prefix, int count)
    {
        var frames = new Sprite[count];
        for (int i = 0; i < count; i++) frames[i] = LoadSprite($"{prefix}{i}.png");
        return frames;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
    }

    /// <summary>Assigns private [SerializeField] fields the same way the Inspector would.</summary>
    static void Set(Object target, params (string field, object value)[] values)
    {
        var so = new SerializedObject(target);
        foreach ((string field, object value) in values)
        {
            SerializedProperty prop = so.FindProperty(field)
                ?? throw new InvalidOperationException($"{target.GetType().Name} has no serialized field '{field}'");
            switch (value)
            {
                case Object obj: prop.objectReferenceValue = obj; break;
                case Object[] array:
                    prop.arraySize = array.Length;
                    for (int i = 0; i < array.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = array[i];
                    break;
                case float f: prop.floatValue = f; break;
                case int n: prop.intValue = n; break;
                case bool b: prop.boolValue = b; break;
                case Vector2 v: prop.vector2Value = v; break;
                case Enum e: prop.enumValueIndex = Convert.ToInt32(e); break;
                default: throw new NotSupportedException($"Unsupported value type {value?.GetType().Name} for '{field}'");
            }
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
