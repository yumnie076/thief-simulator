#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

/// <summary>
/// One-click scene builder for Egel op Expeditie.
/// Menu: EgelGame → Build Scene
/// Creates all GameObjects, UI Canvas, Camera, and wires references.
/// </summary>
public class EgelSceneBootstrapper : Editor
{
    private const string SpriteDir = "Assets/Resources/EgelGame";

    [MenuItem("EgelGame/Build Scene")]
    public static void BuildScene()
    {
        // ── Step 0: Generate sprites if missing ──
        if (!File.Exists($"{SpriteDir}/tile_grass.png"))
        {
            Debug.Log("[EgelBoot] Generating sprites first...");
            ProcSpriteGenerator.GenerateAll();
        }

        // ── Step 1: Clean up old system objects ──
        string[] toClean = { "GameCanvas", "SystemManagers", "MainCamera", "EventSystem", "GardenGrid", "Hedgehog", "Background" };
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null) continue;
            foreach (var name in toClean)
            {
                if (go.name == name)
                {
                    Object.DestroyImmediate(go);
                    break;
                }
            }
        }

        // ── Step 1.5: Clean up default scene objects ──
        var defaultCam = GameObject.Find("Main Camera");
        if (defaultCam != null) Object.DestroyImmediate(defaultCam);

        // ── Step 2: Camera ──
        var camGO = new GameObject("MainCamera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = HexColor("#d4e8d4");
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(6f, 6f, -10f);
        // Add AudioListener
        camGO.AddComponent<AudioListener>();
        // Add CameraFollow
        camGO.AddComponent<CameraFollow>();

        // ── Step 3: Background ──
        var bgGO = new GameObject("Background");
        var bgSR = bgGO.AddComponent<SpriteRenderer>();
        // Wait, tile_grass isn't generated when Bootstrapper runs for the first time. We'll use a solid color fallback.
        var tex = Resources.Load<Sprite>("EgelGame/tile_grass");
        if (tex != null)
        {
            bgSR.sprite = tex;
            bgSR.drawMode = SpriteDrawMode.Tiled;
            bgSR.size = new Vector2(18f, 18f);
        }
        else
        {
            bgSR.sprite = LoadSprite("background");
            bgSR.drawMode = SpriteDrawMode.Simple;
            bgGO.transform.localScale = new Vector3(18f, 18f, 1f); 
        }
        bgSR.color = Color.white;
        bgSR.sortingOrder = -1000;
        bgGO.transform.position = new Vector3(6f, 6f, 0f); // Center of cozy 12x12 map

        // ── Step 4: EventSystem ──
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ── Step 5: SystemManagers ──
        var managers = new GameObject("SystemManagers");
        managers.AddComponent<GameManager>();
        managers.AddComponent<ScoreManager>();
        var phaseCtrl = managers.AddComponent<PhaseController>();
        var gardenMgr = managers.AddComponent<GardenManager>();
        managers.AddComponent<EcosystemManager>();
        managers.AddComponent<QuestManager>();
        var bioTracker = managers.AddComponent<BiodiversityTracker>();
        managers.AddComponent<AudioManager>();

        // ── Step 6: Canvas ──
        var canvasGO = new GameObject("GameCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var uiMgr = canvasGO.AddComponent<UIManager>();

        // ── Phase 1: Intro Panel ──
        var introPanel = CreatePanel(canvasGO, "IntroPanel", true);
        var introUI = introPanel.AddComponent<IntroScreenUI>();
        BuildIntroPanel(introPanel, introUI);

        // ── Phase 2: Build Panel ──
        var buildPanel = CreatePanel(canvasGO, "BuildPanel", true);
        var buildUI = buildPanel.AddComponent<BuildPhaseUI>();

        // ── Education Popup ──
        var eduPopup = CreatePanel(canvasGO, "EducationPopup", true);
        var eduUI = eduPopup.AddComponent<EducationPopup>();
        BuildEducationPopup(eduPopup, eduUI);

        // ── Phase 3: Hedgehog Panel ──
        var hedgehogPanel = CreatePanel(canvasGO, "HedgehogPanel", true);
        var hedgehogUI = hedgehogPanel.AddComponent<HedgehogPhaseUI>();

        // ── Phase 4: Result Panel ──
        var resultPanel = CreatePanel(canvasGO, "ResultPanel", true);
        var resultUI = resultPanel.AddComponent<ResultScreenUI>();

        // ── Fade Panel (full screen black overlay for transitions) ──
        var fadePanel = CreatePanel(canvasGO, "FadePanel", true);
        var fadeImg = fadePanel.GetComponent<Image>();
        if (fadeImg == null) fadeImg = fadePanel.AddComponent<Image>();
        fadeImg.color = Color.black;
        var fadeCG = fadePanel.GetComponent<CanvasGroup>();
        if (fadeCG == null) fadeCG = fadePanel.AddComponent<CanvasGroup>();
        fadeCG.alpha = 0f;
        fadeCG.blocksRaycasts = false;
        fadeCG.interactable = false;

        // ── Wire UIManager references via SerializedObject ──
        var uiMgrSO = new SerializedObject(uiMgr);
        uiMgrSO.FindProperty("introPanel").objectReferenceValue = introPanel;
        uiMgrSO.FindProperty("buildPanel").objectReferenceValue = buildPanel;
        uiMgrSO.FindProperty("hedgehogPanel").objectReferenceValue = hedgehogPanel;
        uiMgrSO.FindProperty("resultPanel").objectReferenceValue = resultPanel;
        uiMgrSO.FindProperty("fadePanel").objectReferenceValue = fadePanel;
        uiMgrSO.FindProperty("fadeCanvasGroup").objectReferenceValue = fadeCG;
        uiMgrSO.ApplyModifiedProperties();

        // ── Wire PhaseController references ──
        var pcSO = new SerializedObject(phaseCtrl);
        pcSO.FindProperty("fadeOverlay").objectReferenceValue = fadeCG;
        pcSO.ApplyModifiedProperties();

        // ── Build the Build Phase UI content ──
        BuildBuildPhasePanel(buildPanel, buildUI);

        // ── Build Hedgehog Phase UI content ──
        BuildHedgehogPhasePanel(hedgehogPanel, hedgehogUI);

        // ── Build Result Panel content ──
        BuildResultPanel(resultPanel, resultUI);

        // ── Mark scene dirty ──
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[EgelBoot] Scene built successfully! Press Play to test.");
    }

    // ════════════════════════════════════════
    // Panel builders
    // ════════════════════════════════════════

    private static void BuildIntroPanel(GameObject panel, IntroScreenUI ui)
    {
        // Background overlay
        var bg = panel.GetComponent<Image>();
        bg.color = HexColor("#d4e8d4");

        // Title
        var titleGO = CreateTMP(panel, "Title", "EGEL OP EXPEDITIE", 56,
            TextAlignmentOptions.Center, new Vector2(0, 200), new Vector2(800, 80));
        var titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.color = HexColor("#2d7a2a");
        titleTMP.fontStyle = FontStyles.Bold;

        // Subtitle
        CreateTMP(panel, "Subtitle", "Hoe egelvriendelijk is jouw tuin?", 28,
            TextAlignmentOptions.Center, new Vector2(0, 140), new Vector2(800, 50));

        // Question
        var questionGO = CreateTMP(panel, "Question", "Hoe groen is jouw tuin het meest?", 32,
            TextAlignmentOptions.Center, new Vector2(0, 60), new Vector2(800, 50));
        questionGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Buttons
        var btn1 = CreateStyledButton(panel, "BtnHard",
            "Vooral tegels en weinig natuur", HexColor("#d94a3d"),
            new Vector2(0, -30), new Vector2(700, 70));

        var btn2 = CreateStyledButton(panel, "BtnMedium",
            "Mix van tegels, gras en wat planten", HexColor("#e8c447"),
            new Vector2(0, -120), new Vector2(700, 70));

        var btn3 = CreateStyledButton(panel, "BtnEasy",
            "Veel groen, bloemen, bomen en plek voor insecten", HexColor("#5ab84a"),
            new Vector2(0, -210), new Vector2(700, 70));

        // Footer
        CreateTMP(panel, "Footer", "Een Tuinen van de Toekomst x Avans project", 16,
            TextAlignmentOptions.Center, new Vector2(0, -340), new Vector2(800, 40));

        // Wire IntroScreenUI references
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("hardButton").objectReferenceValue = btn1.GetComponent<Button>();
        uiSO.FindProperty("mediumButton").objectReferenceValue = btn2.GetComponent<Button>();
        uiSO.FindProperty("easyButton").objectReferenceValue = btn3.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();
    }

    private static void BuildEducationPopup(GameObject panel, EducationPopup ui)
    {
        // Position at top center
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.2f, 0.85f);
        rt.anchorMax = new Vector2(0.8f, 0.95f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var bg = panel.GetComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.92f);

        // Add CanvasGroup for fading
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        var textGO = CreateTMP(panel, "PopupText", "", 22,
            TextAlignmentOptions.Center, Vector2.zero, new Vector2(0, 0));
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.05f, 0.1f);
        textRT.anchorMax = new Vector2(0.95f, 0.9f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var textTMP = textGO.GetComponent<TextMeshProUGUI>();
        textTMP.color = HexColor("#333333");

        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("messageText").objectReferenceValue = textTMP;
        uiSO.FindProperty("canvasGroup").objectReferenceValue = cg;
        uiSO.ApplyModifiedProperties();
    }

    private static void BuildBuildPhasePanel(GameObject panel, BuildPhaseUI ui)
    {
        var bg = panel.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0); // transparent overlay
        bg.raycastTarget = false; // Allow mouse clicks to pass through to the game world!

        // ── Bottom Toolbar (Bottom Center/Left) ──
        var toolbar = new GameObject("Toolbar");
        toolbar.transform.SetParent(panel.transform, false);
        var toolbarRT = toolbar.AddComponent<RectTransform>();
        toolbarRT.anchorMin = new Vector2(0.02f, 0.02f);
        toolbarRT.anchorMax = new Vector2(0.70f, 0.12f);
        toolbarRT.offsetMin = Vector2.zero;
        toolbarRT.offsetMax = Vector2.zero;

        var toolbarImg = toolbar.AddComponent<Image>();
        toolbarImg.color = new Color(1, 1, 1, 0.9f);

        var hlg = toolbar.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.padding = new RectOffset(10, 10, 10, 10);
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        string[] toolLabels = { "[1]", "[2]", "[3]", "[4]", "[5]", "[6]", "[7]" };
        string[] iconNames = { "icon_hammer", "icon_flower", "icon_bush", "icon_tree", "icon_water", "icon_leaf", "icon_house" };
        Button[] toolButtons = new Button[7];
        for (int i = 0; i < toolLabels.Length; i++)
        {
            var btnGO = CreateToolButton(toolbar, $"ToolBtn_{i}", toolLabels[i], iconNames[i]);
            toolButtons[i] = btnGO.GetComponent<Button>();
        }

        // ── Top-right HUD container (Quest + Counters) ──
        var hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(panel.transform, false);
        var hudRT = hudGO.AddComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0.70f, 0.65f);
        hudRT.anchorMax = new Vector2(0.98f, 0.98f);
        hudRT.offsetMin = Vector2.zero;
        hudRT.offsetMax = Vector2.zero;

        var hudBg = hudGO.AddComponent<Image>();
        hudBg.color = new Color(0.15f, 0.22f, 0.15f, 0.92f); // Sleek modern dark green panel

        // Add a vertical layout group to the HUD
        var vlg = hudGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // HUD Header
        var header = CreateTMP(hudGO, "HUDHeader", "TUIN MISSIE", 20,
            TextAlignmentOptions.Left, Vector2.zero, new Vector2(0, 24));
        header.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        header.GetComponent<TextMeshProUGUI>().color = HexColor("#e8c447"); // premium yellow

        // Quest Text
        var questText = CreateTMP(hudGO, "QuestText", "Laden...", 16,
            TextAlignmentOptions.Left, Vector2.zero, new Vector2(0, 50));
        questText.GetComponent<TextMeshProUGUI>().color = Color.white;
        
        var questUI = hudGO.AddComponent<QuestUI>();
        questUI.AssignText(questText.GetComponent<TextMeshProUGUI>());

        // Spacer Line
        var line = new GameObject("Divider");
        line.transform.SetParent(hudGO.transform, false);
        var lineImg = line.AddComponent<Image>();
        lineImg.color = new Color(1, 1, 1, 0.2f);
        var lineLE = line.AddComponent<LayoutElement>();
        lineLE.minHeight = 2;
        lineLE.preferredHeight = 2;

        // Counter Area
        var counters = new GameObject("Counters");
        counters.transform.SetParent(hudGO.transform, false);
        var countersVLG = counters.AddComponent<VerticalLayoutGroup>();
        countersVLG.spacing = 5;
        countersVLG.childForceExpandWidth = true;
        countersVLG.childForceExpandHeight = false;
        countersVLG.childControlWidth = true;
        countersVLG.childControlHeight = true;

        // Actions Counter
        var actionsTextGO = CreateTMP(counters, "ActionsText", "Acties: 15", 18,
            TextAlignmentOptions.Left, Vector2.zero, new Vector2(0, 25));
        actionsTextGO.GetComponent<TextMeshProUGUI>().color = HexColor("#a2e3a0"); // soft green
        actionsTextGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Biodiversity Counter
        var bioTextGO = CreateTMP(counters, "BiodiversityText", "Biodiversiteit: 0", 18,
            TextAlignmentOptions.Left, Vector2.zero, new Vector2(0, 25));
        bioTextGO.GetComponent<TextMeshProUGUI>().color = HexColor("#7cd2e6"); // soft blue
        bioTextGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Bottom-right Klaar button (co-anchored with Toolbar)
        var klaarBtn = new GameObject("KlaarButton");
        klaarBtn.transform.SetParent(panel.transform, false);
        var klaarRT = klaarBtn.AddComponent<RectTransform>();
        klaarRT.anchorMin = new Vector2(0.72f, 0.02f);
        klaarRT.anchorMax = new Vector2(0.98f, 0.12f);
        klaarRT.offsetMin = Vector2.zero;
        klaarRT.offsetMax = Vector2.zero;

        var klaarImg = klaarBtn.AddComponent<Image>();
        klaarImg.color = HexColor("#5ab84a"); // solid green

        var btn = klaarBtn.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1, 1, 1, 0.85f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;

        var textGO = CreateTMP(klaarBtn, "Label", "Klaar! 🦔", 22,
            TextAlignmentOptions.Center, Vector2.zero, Vector2.zero);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 5);
        textRT.offsetMax = new Vector2(-10, -5);
        textRT.anchoredPosition = Vector2.zero;
        textRT.sizeDelta = Vector2.zero;
        textGO.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Wire
        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("actionsText").objectReferenceValue = actionsTextGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("biodiversityText").objectReferenceValue = bioTextGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("doneButton").objectReferenceValue = btn;
        // Store tool buttons
        var toolBtnsProp = uiSO.FindProperty("toolButtons");
        toolBtnsProp.arraySize = 7;
        for (int i = 0; i < 7; i++)
            toolBtnsProp.GetArrayElementAtIndex(i).objectReferenceValue = toolButtons[i];
        uiSO.ApplyModifiedProperties();
    }

    private static void BuildHedgehogPhasePanel(GameObject panel, HedgehogPhaseUI ui)
    {
        var bg = panel.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0); // transparent

        // Top message
        var msgGO = CreateTMP(panel, "HedgehogMsg", "De egel zoekt voedsel en schuilplek...", 28,
            TextAlignmentOptions.Center, new Vector2(0, 420), new Vector2(800, 60));

        // Timer
        var timerGO = CreateTMP(panel, "TimerText", "45", 36,
            TextAlignmentOptions.Center, new Vector2(350, 380), new Vector2(200, 60));

        // Hunger bar background
        var hungerBgGO = new GameObject("HungerBarBG");
        hungerBgGO.transform.SetParent(panel.transform, false);
        var hbRT = hungerBgGO.AddComponent<RectTransform>();
        hbRT.anchorMin = new Vector2(0.7f, 0.85f);
        hbRT.anchorMax = new Vector2(0.95f, 0.87f);
        hbRT.offsetMin = Vector2.zero;
        hbRT.offsetMax = Vector2.zero;
        var hbImg = hungerBgGO.AddComponent<Image>();
        hbImg.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);

        // Hunger bar fill
        var hungerFillGO = new GameObject("HungerBarFill");
        hungerFillGO.transform.SetParent(hungerBgGO.transform, false);
        var hfRT = hungerFillGO.AddComponent<RectTransform>();
        hfRT.anchorMin = Vector2.zero;
        hfRT.anchorMax = Vector2.one;
        hfRT.offsetMin = Vector2.zero;
        hfRT.offsetMax = Vector2.zero;
        var hfImg = hungerFillGO.AddComponent<Image>();
        hfImg.color = GreenColor();
        hfImg.type = Image.Type.Filled;
        hfImg.fillMethod = Image.FillMethod.Horizontal;

        // Hunger label
        var hungerLabel = CreateTMP(panel, "HungerLabel", "Honger", 18,
            TextAlignmentOptions.Right, new Vector2(300, 340), new Vector2(200, 30));

        // Safety label
        var safetyTextGO = CreateTMP(panel, "SafetyLabel", "Veiligheid", 18,
            TextAlignmentOptions.Right, new Vector2(300, 300), new Vector2(200, 30));
        safetyTextGO.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Safety icon (small colored square/circle representing safe/unsafe)
        var safetyIconGO = new GameObject("SafetyIcon");
        safetyIconGO.transform.SetParent(panel.transform, false);
        var siRT = safetyIconGO.AddComponent<RectTransform>();
        siRT.anchoredPosition = new Vector2(420, 300);
        siRT.sizeDelta = new Vector2(20, 20);
        var siImg = safetyIconGO.AddComponent<Image>();
        siImg.color = HexColor("#d94a3d"); // default to unsafe (red)

        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("timerText").objectReferenceValue = timerGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("hungerBarFill").objectReferenceValue = hfImg;
        uiSO.FindProperty("headerText").objectReferenceValue = msgGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("safetyText").objectReferenceValue = safetyTextGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("safetyIcon").objectReferenceValue = siImg;
        uiSO.ApplyModifiedProperties();
    }

    private static void BuildResultPanel(GameObject panel, ResultScreenUI ui)
    {
        var bg = panel.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);

        // Title
        var titleGO = CreateTMP(panel, "ResultTitle", "Jouw tuin is een egel-paradijs!", 42,
            TextAlignmentOptions.Center, new Vector2(0, 350), new Vector2(900, 80));
        titleGO.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Score box
        var scoreBox = new GameObject("ScoreBox");
        scoreBox.transform.SetParent(panel.transform, false);
        var sbRT = scoreBox.AddComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(0.2f, 0.3f);
        sbRT.anchorMax = new Vector2(0.8f, 0.7f);
        sbRT.offsetMin = Vector2.zero;
        sbRT.offsetMax = Vector2.zero;
        var sbImg = scoreBox.AddComponent<Image>();
        sbImg.color = new Color(1, 1, 1, 0.15f);

        var scoreText = CreateTMP(scoreBox, "ScoreText", "EGEL SCORE\n\nBiodiversiteit: 0\nVoedsel: 0\nWater: 0\nSchuilplek: 0\n───────\nTOTAAL: 0", 26,
            TextAlignmentOptions.Center, Vector2.zero, new Vector2(0, 0));
        var stRT = scoreText.GetComponent<RectTransform>();
        stRT.anchorMin = new Vector2(0.1f, 0.1f);
        stRT.anchorMax = new Vector2(0.9f, 0.9f);
        stRT.offsetMin = Vector2.zero;
        stRT.offsetMax = Vector2.zero;
        scoreText.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Tips
        var tipsGO = CreateTMP(panel, "TipsText", "", 20,
            TextAlignmentOptions.Center, new Vector2(0, -200), new Vector2(800, 120));
        tipsGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 0.8f, 1);

        // Final message
        var finalGO = CreateTMP(panel, "FinalMsg", "Elke kleine verandering helpt. Eén tuin is al een verschil.", 22,
            TextAlignmentOptions.Center, new Vector2(0, -320), new Vector2(800, 50));
        finalGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.8f);
        finalGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Italic;

        // Replay button
        var replayBtn = CreateStyledButton(panel, "ReplayButton",
            "Speel opnieuw", HexColor("#5ab84a"),
            new Vector2(-120, -420), new Vector2(280, 60));

        // Quit button
        var quitBtn = CreateStyledButton(panel, "QuitButton",
            "Sluiten", HexColor("#d94a3d"),
            new Vector2(120, -420), new Vector2(200, 60));

        var uiSO = new SerializedObject(ui);
        uiSO.FindProperty("titleText").objectReferenceValue = titleGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("tipsText").objectReferenceValue = tipsGO.GetComponent<TextMeshProUGUI>();
        uiSO.FindProperty("replayButton").objectReferenceValue = replayBtn.GetComponent<Button>();
        uiSO.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
        uiSO.ApplyModifiedProperties();
    }

    // ════════════════════════════════════════
    // UI Helper methods
    // ════════════════════════════════════════

    private static GameObject CreatePanel(GameObject parent, string name, bool active)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false; // Fix: Prevent full screen UI from blocking mouse clicks
        go.SetActive(active);
        return go;
    }

    private static GameObject CreateTMP(GameObject parent, string name, string text,
        float fontSize, TextAlignmentOptions alignment, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = HexColor("#333333");
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return go;
    }

    private static GameObject CreateStyledButton(GameObject parent, string name,
        string label, Color bgColor, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1, 1, 1, 0.85f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;

        var textGO = CreateTMP(go, "Label", label, 22,
            TextAlignmentOptions.Center, Vector2.zero, size);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 5);
        textRT.offsetMax = new Vector2(-10, -5);
        textRT.anchoredPosition = Vector2.zero;
        textRT.sizeDelta = Vector2.zero;
        var txt = textGO.GetComponent<TextMeshProUGUI>();
        txt.color = Color.white;
        txt.enableWordWrapping = false; // Fix: Prevent text from stacking vertically

        return go;
    }

    private static GameObject CreateToolButton(GameObject parent, string name, string label, string spriteName)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 75); // Taller button for modern stacked icon design

        var img = go.AddComponent<Image>();
        img.color = new Color(0.95f, 0.95f, 0.95f, 1f);

        var btn = go.AddComponent<Button>();

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 75;
        le.preferredHeight = 75;

        // Vertical stacking layout for the icon and label
        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // 1. Icon GameObject
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(go.transform, false);
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite = LoadSprite(spriteName);
        iconImg.preserveAspect = true;

        var iconLE = iconGO.AddComponent<LayoutElement>();
        iconLE.preferredHeight = 48;
        iconLE.preferredWidth = 48;

        // 2. Text Label
        var textGO = CreateTMP(go, "Label", label, 14, 
            TextAlignmentOptions.Center, Vector2.zero, Vector2.zero);
        var txt = textGO.GetComponent<TextMeshProUGUI>();
        txt.color = HexColor("#333333");
        txt.enableWordWrapping = false;
        
        var textLE = textGO.AddComponent<LayoutElement>();
        textLE.preferredHeight = 20;

        return go;
    }

    // ════════════════════════════════════════
    // Sprite loading
    // ════════════════════════════════════════

    private static Sprite LoadSprite(string name)
    {
        string path = $"{SpriteDir}/{name}.png";
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }

    private static Color GreenColor()
    {
        return HexColor("#7bc043");
    }
}
#endif
