using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Editor tooling that generates the ARBusinessCard scene, captures the layout screenshot
/// and produces the Android / iOS builds. Everything shown on the card is set in the constants below.
/// </summary>
public static class ARBusinessCardBuilder
{
    // ----- Card content -----
    private const string FullName = "Ntare GAMA";
    private const string Initials = "NG";
    private const string JobTitle = "Software Engineer";
    private const string EmailUrl = "mailto:b.iradukund3@alustudent.com";
    private const string GitHubUrl = "https://github.com/Ntare-GAMA";
    private const string TwitterUrl = "https://x.com/Ntare_GAMA";
    private const string LinkedInUrl = "https://www.linkedin.com/in/ntare-gama-allan-a5b00a368/";

    // ----- Paths -----
    private const string ScenePath = "Assets/Scenes/ARBusinessCard.unity";
    private const string MarkerPath = "Assets/Textures/ARBusinessCardMarker.png";
    private const string HbtnMarkerPath = "Assets/Textures/HBTNARMarker.png";
    private const string IconFolder = "Assets/Textures/Icons/";
    private const string ClickPath = "Assets/Audio/ButtonClick.wav";
    private const string MarkerMaterialPath = "Assets/Materials/MarkerPreview.mat";
    private const string ScreenshotPath = "Screenshots/BusinessCardLayout.png";

    // ----- Dimensions -----
    // Printed marker width in meters (marker image is 3:2)
    private const float MarkerWidth = 0.15f;
    private const float MarkerHeight = MarkerWidth * 2f / 3f;
    // Default Holberton marker is square: same height as the custom marker so the card sits the same way on both
    private const float HbtnMarkerWidth = MarkerHeight;
    // Canvas size in canvas units, mapped to a physical card slightly wider than the marker
    private const float CanvasWidth = 1000f;
    private const float CanvasHeight = 620f;
    private const float CardWidth = MarkerWidth * 1.1f;
    private const float CanvasScale = CardWidth / CanvasWidth;

    // ----- Palette (high contrast, WCAG AA for white text) -----
    private static readonly Color CardColor = Hex("0F172A");
    private static readonly Color AccentColor = Hex("F4A261");
    private static readonly Color MutedColor = Hex("94A3B8");
    private static readonly Color DividerColor = Hex("334155");
    private static readonly Color EmailColor = Hex("C5221F");
    private static readonly Color GitHubColor = Hex("6E40C9");
    private static readonly Color TwitterColor = Hex("1A6FB0");
    private static readonly Color LinkedInColor = Hex("0A66C2");

    /// <summary>Generates the scene, player settings and layout screenshot.</summary>
    [MenuItem("AR Business Card/Build Scene")]
    public static void BuildScene()
    {
        ConfigureImporters();
        ConfigurePlayerSettings();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientLight = Color.white;

        // AR camera
        var cameraGo = new GameObject("ARCamera");
        cameraGo.tag = "MainCamera";
        var cam = cameraGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 50f;
        cameraGo.transform.SetPositionAndRotation(new Vector3(0f, 0.3f, -0.12f), Quaternion.Euler(68f, 0f, 0f));
        cameraGo.AddComponent<VuforiaBehaviour>();
        cameraGo.AddComponent<DefaultInitializationErrorHandler>();
        var audioSource = cameraGo.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        cameraGo.AddComponent<AudioListener>();

        // Light
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Event system for touch input on the world space canvas
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();

        // Card hierarchy: CardAnchor (re-parented to the image target) > CardPivot (hinge) > Canvas
        var anchor = new GameObject("CardAnchor").transform;
        var animator = anchor.gameObject.AddComponent<BusinessCardAnimator>();

        var pivot = new GameObject("CardPivot").transform;
        pivot.SetParent(anchor, false);
        pivot.localPosition = new Vector3(0f, 0.002f, -MarkerHeight / 2f);

        var canvasGo = new GameObject("BusinessCardCanvas", typeof(RectTransform));
        canvasGo.transform.SetParent(pivot, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 3f;
        canvasGo.AddComponent<GraphicRaycaster>();
        var canvasRect = (RectTransform)canvasGo.transform;
        canvasRect.sizeDelta = new Vector2(CanvasWidth, CanvasHeight);
        canvasRect.localScale = Vector3.one * CanvasScale;
        canvasRect.localRotation = Quaternion.Euler(90f, 0f, 0f);
        canvasRect.localPosition = new Vector3(0f, 0f, CanvasHeight * CanvasScale / 2f);

        Sprite rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Background
        var bg = CreateImage("Background", canvasRect, rounded, CardColor, Vector2.zero, new Vector2(CanvasWidth, CanvasHeight));
        bg.type = Image.Type.Sliced;
        bg.pixelsPerUnitMultiplier = 0.25f;

        // Accent bar, sweeping from the left
        var accent = CreateImage("AccentBar", canvasRect, null, AccentColor, new Vector2(0f, CanvasHeight / 2f - 18f), new Vector2(CanvasWidth - 60f, 12f));
        accent.rectTransform.pivot = new Vector2(0f, 0.5f);
        accent.rectTransform.anchoredPosition = new Vector2(-(CanvasWidth - 60f) / 2f, CanvasHeight / 2f - 18f);

        // Header
        var avatarGroup = CreateGroup("Avatar", canvasRect, new Vector2(-355f, 150f), new Vector2(170f, 170f));
        CreateImage("Ring", avatarGroup.transform, circle, AccentColor, Vector2.zero, new Vector2(170f, 170f));
        CreateImage("Face", avatarGroup.transform, circle, CardColor, Vector2.zero, new Vector2(150f, 150f));
        CreateText("Initials", avatarGroup.transform, font, Initials, 64, FontStyle.Bold, AccentColor, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(150f, 150f));

        var nameGroup = CreateGroup("Name", canvasRect, new Vector2(95f, 185f), new Vector2(620f, 90f));
        CreateText("NameText", nameGroup.transform, font, FullName, 76, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(620f, 90f));

        var titleGroup = CreateGroup("JobTitle", canvasRect, new Vector2(95f, 110f), new Vector2(620f, 60f));
        CreateText("JobTitleText", titleGroup.transform, font, JobTitle, 44, FontStyle.Normal, AccentColor, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(620f, 60f));

        var dividerGroup = CreateGroup("Divider", canvasRect, new Vector2(0f, 40f), new Vector2(880f, 4f));
        CreateImage("Line", dividerGroup.transform, null, DividerColor, Vector2.zero, new Vector2(880f, 4f));

        var hintGroup = CreateGroup("Hint", canvasRect, new Vector2(0f, -283f), new Vector2(880f, 36f));
        CreateText("HintText", hintGroup.transform, font, "Tap a button to connect", 28, FontStyle.Italic, MutedColor, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(880f, 36f));

        // Link buttons in a 2x2 grid, large enough to tap individually
        AudioClip click = AssetDatabase.LoadAssetAtPath<AudioClip>(ClickPath);
        var slots = new[]
        {
            CreateLinkButton("Email", canvasRect, new Vector2(-225f, -60f), EmailColor, "icon_email", EmailUrl, rounded, font, audioSource, click),
            CreateLinkButton("GitHub", canvasRect, new Vector2(225f, -60f), GitHubColor, "icon_github", GitHubUrl, rounded, font, audioSource, click),
            CreateLinkButton("Twitter / X", canvasRect, new Vector2(-225f, -195f), TwitterColor, "icon_twitter", TwitterUrl, rounded, font, audioSource, click),
            CreateLinkButton("LinkedIn", canvasRect, new Vector2(225f, -195f), LinkedInColor, "icon_linkedin", LinkedInUrl, rounded, font, audioSource, click),
        };

        animator.cardPivot = pivot;
        animator.accentBar = accent.rectTransform;
        animator.headerElements = new[] { avatarGroup, nameGroup, titleGroup, dividerGroup, hintGroup };
        animator.buttonSlots = slots;
        animator.avatar = (RectTransform)avatarGroup.transform;

        // Editor-only preview of the marker under the card (stripped from builds)
        var preview = GameObject.CreatePrimitive(PrimitiveType.Quad);
        preview.name = "MarkerPreview (Editor Only)";
        preview.tag = "EditorOnly";
        Object.DestroyImmediate(preview.GetComponent<Collider>());
        preview.transform.SetParent(anchor, false);
        preview.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        preview.transform.localScale = new Vector3(MarkerWidth, MarkerHeight, 1f);
        preview.GetComponent<MeshRenderer>().sharedMaterial = GetMarkerMaterial();

        // Image target setup
        var targetGo = new GameObject("ImageTarget");
        var markerTarget = targetGo.AddComponent<ARMarkerTarget>();
        markerTarget.markerTextures = new[]
        {
            AssetDatabase.LoadAssetAtPath<Texture2D>(HbtnMarkerPath),
            AssetDatabase.LoadAssetAtPath<Texture2D>(MarkerPath),
        };
        markerTarget.printedWidths = new[] { HbtnMarkerWidth, MarkerWidth };
        markerTarget.cardAnchor = anchor;
        markerTarget.cardAnimator = animator;

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();

        foreach (Transform t in canvasRect.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = 5;
        EditorSceneManager.SaveScene(scene, ScenePath);

        CaptureLayout(canvasRect);
        Debug.Log("ARBusinessCardBuilder: scene generated at " + ScenePath);
    }

    /// <summary>Builds Builds/Android/ARBusinessCard.apk.</summary>
    [MenuItem("AR Business Card/Build Android")]
    public static void BuildAndroid()
    {
        ConfigurePlayerSettings();
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = false;
        Build(BuildTarget.Android, "Builds/Android/ARBusinessCard.apk");
    }

    /// <summary>Exports the Xcode project to Builds/iOS.</summary>
    [MenuItem("AR Business Card/Build iOS")]
    public static void BuildIOS()
    {
        ConfigurePlayerSettings();
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        Build(BuildTarget.iOS, "Builds/iOS");
        // Vuforia's editor Play Mode (webcam) can't load its driver while iOS is the active platform
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
    }

    // Run the build and fail the batch-mode process on error
    private static void Build(BuildTarget target, string location)
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = location,
            target = target,
            options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log("ARBusinessCardBuilder: " + target + " build " + report.summary.result + " -> " + location);
        if (Application.isBatchMode && report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    // Identity, orientation and platform requirements for Vuforia on Android / iOS
    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.companyName = "NtareGAMA";
        PlayerSettings.productName = "AR Business Card";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.ntaregama.arbusinesscard");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.ntaregama.arbusinesscard");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

        // Input System package only: "Both" is unsupported on Android, and the UI uses InputSystemUIInputModule
        var projectSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        projectSettings.FindProperty("activeInputHandler").intValue = 1;
        projectSettings.ApplyModifiedPropertiesWithoutUndo();

        PlayerSettings.iOS.cameraUsageDescription = "The camera is used to detect the business card marker.";
        PlayerSettings.iOS.targetOSVersionString = "15.0";
    }

    // Markers must be readable and uncompressed for runtime image targets; icons are UI sprites
    private static void ConfigureImporters()
    {
        AssetDatabase.Refresh();
        foreach (string path in new[] { MarkerPath, HbtnMarkerPath })
        {
            var marker = (TextureImporter)AssetImporter.GetAtPath(path);
            marker.textureType = TextureImporterType.Default;
            marker.isReadable = true;
            marker.mipmapEnabled = false;
            marker.npotScale = TextureImporterNPOTScale.None;
            marker.textureCompression = TextureImporterCompression.Uncompressed;
            marker.SaveAndReimport();
        }

        foreach (string icon in new[] { "icon_email", "icon_github", "icon_twitter", "icon_linkedin" })
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(IconFolder + icon + ".png");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }
    }

    // Material showing the marker in the editor preview
    private static Material GetMarkerMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MarkerMaterialPath);
        if (material != null)
            return material;
        Directory.CreateDirectory("Assets/Materials");
        material = new Material(Shader.Find("Unlit/Texture")) { mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(MarkerPath) };
        AssetDatabase.CreateAsset(material, MarkerMaterialPath);
        return material;
    }

    // Button slot (animated by the intro) holding the pressable button (animated on press)
    private static RectTransform CreateLinkButton(string label, Transform parent, Vector2 position, Color color, string icon,
        string url, Sprite rounded, Font font, AudioSource audioSource, AudioClip click)
    {
        var size = new Vector2(430f, 118f);
        var slot = new GameObject(label + " Slot", typeof(RectTransform)).GetComponent<RectTransform>();
        slot.SetParent(parent, false);
        slot.anchoredPosition = position;
        slot.sizeDelta = size;

        var bg = CreateImage(label + " Button", slot, rounded, color, Vector2.zero, size);
        bg.type = Image.Type.Sliced;
        bg.pixelsPerUnitMultiplier = 0.35f;
        var button = bg.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
        colors.pressedColor = new Color(0.62f, 0.62f, 0.62f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.06f;
        button.colors = colors;
        var nav = button.navigation;
        nav.mode = Navigation.Mode.None;
        button.navigation = nav;

        var iconImage = CreateImage("Icon", bg.transform, AssetDatabase.LoadAssetAtPath<Sprite>(IconFolder + icon + ".png"),
            Color.white, new Vector2(-size.x / 2f + 70f, 0f), new Vector2(76f, 76f));
        iconImage.raycastTarget = false;
        iconImage.preserveAspect = true;
        var text = CreateText("Label", bg.transform, font, label, 44, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
            new Vector2(55f, 0f), new Vector2(size.x - 150f, size.y));
        text.raycastTarget = false;

        var link = bg.gameObject.AddComponent<LinkButton>();
        link.url = url;
        link.audioSource = audioSource;
        link.clickClip = click;
        return slot;
    }

    // Empty RectTransform with a CanvasGroup so it can be faded as one element
    private static CanvasGroup CreateGroup(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var group = go.AddComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;
        return group;
    }

    // UI image helper
    private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    // UI text helper
    private static Text CreateText(string name, Transform parent, Font font, string content, int size, FontStyle style,
        Color color, TextAnchor alignment, Vector2 position, Vector2 box)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchoredPosition = position;
        rect.sizeDelta = box;
        var text = go.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    // Render the flat card straight on into Screenshots/BusinessCardLayout.png
    private static void CaptureLayout(RectTransform canvasRect)
    {
        const int width = 1600;
        const int height = 992;
        var camGo = new GameObject("ScreenshotCamera");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Hex("E2E8F0");
        cam.orthographic = true;
        cam.orthographicSize = CanvasHeight * CanvasScale * 0.56f;
        cam.aspect = (float)width / height;
        cam.transform.position = canvasRect.position + Vector3.up * 0.5f;
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 1f;
        cam.cullingMask = 1 << 5;

        var canvas = canvasRect.GetComponent<Canvas>();
        canvas.worldCamera = cam;
        Canvas.ForceUpdateCanvases();

        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 8 };
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;

        Directory.CreateDirectory(Path.GetDirectoryName(ScreenshotPath));
        File.WriteAllBytes(ScreenshotPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(camGo);

        canvas.worldCamera = Camera.main;
    }

    // Color from hex string
    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}
