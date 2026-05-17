using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CutsceneManager : MonoBehaviour
{
    static readonly Color agedGold = new Color(0.784f, 0.584f, 0.165f);
    static readonly Color parchmentMid = new Color(0.831f, 0.663f, 0.416f);
    static readonly Color parchmentDark = new Color(0.545f, 0.412f, 0.078f);
    static readonly Color deepBrown = new Color(0.102f, 0.059f, 0.039f);
    static readonly Color parchmentLight = new Color(0.961f, 0.902f, 0.784f);
    static readonly Color inkBrown = new Color(0.239f, 0.169f, 0.122f);

    private readonly string[] cutscenePaths = new string[]
    {
        "Illuminati",
        "KnightsTemplar",
        "Freemasons",
        "Carbonari"
    };

    private readonly string[] cutsceneUrls = new string[4];

    private readonly string[] cutsceneTitles = new string[]
    {
        "The Birth of the Illuminati",
        "The Templar Legacy",
        "Freemasonry and the Founding Fathers",
        "The Carbonari Conspiracy"
    };

    Canvas canvas;
    GameObject videoOutputGO;
    RawImage rawImageOutput;
    VideoPlayer videoPlayer;
    RenderTexture renderTexture;

    GameObject cutsceneUI;
    Text cutsceneCounterText;
    Text cutsceneTitleText;

    GameObject continueButton;
    GameObject skipAllButton;
    GameObject fadeOverlay;
    CanvasGroup fadeCanvasGroup;

    bool isWaitingForContinue = false;
    bool videoErrorReceived = false;
    int currentFactionIndex = -1;
    System.Action onComplete;

    public void PlayFactionCutscene(int factionIndex, System.Action onCompleteCallback)
    {
        currentFactionIndex = factionIndex;
        onComplete = onCompleteCallback;
        StartCoroutine(PlayCutsceneRoutine(factionIndex));
    }

    IEnumerator PlayCutsceneRoutine(int factionIndex)
    {
        if (factionIndex < 0 || factionIndex >= cutscenePaths.Length)
        {
            Debug.LogWarning("[CutsceneManager] Invalid faction index " + factionIndex + ", skipping to game");
            onComplete?.Invoke();
            yield break;
        }

        // FIX ERROR-2: build the correct StreamingAssets path (filesystem path for File.Exists)
        string fsPath = System.IO.Path.Combine(
            Application.streamingAssetsPath, "Cutscenes", cutscenePaths[factionIndex] + ".mp4");

        if (!System.IO.File.Exists(fsPath))
        {
            Debug.LogError(
                "[CutsceneManager] VIDEO FILE MISSING — expected at: " + fsPath +
                "\nCopy your .mp4 files into Assets/StreamingAssets/Cutscenes/ and re-run.");
            onComplete?.Invoke();
            yield break;
        }

        // VideoPlayer.url requires forward slashes and file:/// prefix on Windows
        string videoUrl = fsPath.Replace("\\", "/");
        if (!videoUrl.StartsWith("file://"))
            videoUrl = "file:///" + videoUrl;
        cutsceneUrls[factionIndex] = videoUrl;

        yield return null;

        try
        {
            BuildCutsceneCanvas();
            BuildVideoPlayer();
            BuildUI(factionIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[CutsceneManager] Build failed: " + e.Message);
            Cleanup();
            onComplete?.Invoke();
            yield break;
        }

        if (videoPlayer == null || canvas == null)
        {
            Debug.LogError("[CutsceneManager] Video components null after build");
            Cleanup();
            onComplete?.Invoke();
            yield break;
        }

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = cutsceneUrls[factionIndex];
        videoErrorReceived = false;

        videoPlayer.Prepare();

        float prepareTimeout = 5f;
        float prepareElapsed = 0f;
        while (!videoPlayer.isPrepared && !videoErrorReceived && prepareElapsed < prepareTimeout)
        {
            prepareElapsed += Time.deltaTime;
            yield return null;
        }

        if (videoErrorReceived || !videoPlayer.isPrepared || videoPlayer.frameCount == 0)
        {
            Debug.LogWarning("[CutsceneManager] Video failed to prepare or has zero frames, skipping cutscene");
            Cleanup();
            onComplete?.Invoke();
            yield break;
        }

        yield return StartCoroutine(Fade(0f, 1f, 0.4f));

        videoPlayer.Play();

        yield return StartCoroutine(Fade(1f, 0f, 0.4f));

        continueButton.SetActive(false);

        float playTimeout = 30f;
        float playElapsed = 0f;
        while (videoPlayer.isPlaying && !videoErrorReceived && playElapsed < playTimeout)
        {
            if (videoPlayer.time >= videoPlayer.length - 0.1)
                break;
            playElapsed += Time.deltaTime;
            yield return null;
        }

        if (videoPlayer.time >= videoPlayer.length - 0.1 || !videoPlayer.isPlaying || playElapsed >= playTimeout)
        {
            if (playElapsed >= playTimeout)
                Debug.LogWarning("[CutsceneManager] Video playback timed out, showing continue");
            continueButton.SetActive(true);
            isWaitingForContinue = true;
            yield return new WaitUntil(() => !isWaitingForContinue);
        }

        Cleanup();
        onComplete?.Invoke();
    }

    void BuildCutsceneCanvas()
    {
        GameObject go = new GameObject("CutsceneCanvas", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();

        fadeOverlay = new GameObject("FadeOverlay", typeof(RectTransform));
        fadeOverlay.transform.SetParent(canvas.transform, false);
        RectTransform fadeR = fadeOverlay.GetComponent<RectTransform>();
        if (fadeR != null)
        {
            fadeR.anchorMin = Vector2.zero;
            fadeR.anchorMax = Vector2.one;
            fadeR.sizeDelta = Vector2.zero;
        }
        fadeCanvasGroup = fadeOverlay.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        Image fadeImg = fadeOverlay.AddComponent<Image>();
        fadeImg.color = Color.black;
        fadeImg.raycastTarget = true;

        videoOutputGO = new GameObject("VideoOutput", typeof(RectTransform));
        videoOutputGO.transform.SetParent(canvas.transform, false);
        RectTransform vr = videoOutputGO.GetComponent<RectTransform>();
        if (vr != null)
        {
            vr.anchorMin = Vector2.zero;
            vr.anchorMax = Vector2.one;
            vr.sizeDelta = Vector2.zero;
        }
        Image vi = videoOutputGO.AddComponent<Image>();
        vi.color = Color.white;
    }

    void BuildVideoPlayer()
    {
        if (videoOutputGO == null)
        {
            Debug.LogError("[CutsceneManager] videoOutputGO is null, cannot build VideoPlayer");
            return;
        }

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            Debug.LogError("[CutsceneManager] Failed to create VideoPlayer component");
            return;
        }
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        renderTexture = new RenderTexture(1920, 1080, 24);
        if (renderTexture == null)
        {
            Debug.LogError("[CutsceneManager] Failed to create RenderTexture");
            return;
        }
        renderTexture.Create();

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;

        DestroyImmediate(videoOutputGO.GetComponent<Image>());
        rawImageOutput = videoOutputGO.AddComponent<RawImage>();
        if (rawImageOutput == null)
        {
            Debug.LogError("[CutsceneManager] Failed to add RawImage component");
            return;
        }
        rawImageOutput.texture = renderTexture;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += (vp, msg) => {
            videoErrorReceived = true;
            Debug.LogError($"[CutsceneManager] VideoPlayer error: {msg}. H.264 profile might be unsupported.");
            if (continueButton != null) continueButton.SetActive(true);
        };
    }

    void BuildUI(int factionIndex)
    {
        if (canvas == null) return;

        cutsceneUI = new GameObject("CutsceneUI", typeof(RectTransform));
        cutsceneUI.transform.SetParent(canvas.transform, false);
        RectTransform uiR = cutsceneUI.GetComponent<RectTransform>();
        if (uiR != null)
        {
            uiR.anchorMin = Vector2.zero;
            uiR.anchorMax = Vector2.one;
            uiR.sizeDelta = Vector2.zero;
        }

        GameObject topBar = new GameObject("TopBar", typeof(RectTransform));
        topBar.transform.SetParent(cutsceneUI.transform, false);
        RectTransform tbR = topBar.GetComponent<RectTransform>();
        tbR.anchorMin = new Vector2(0, 0.97f);
        tbR.anchorMax = new Vector2(1, 1);
        tbR.sizeDelta = Vector2.zero;
        Image tbImg = topBar.AddComponent<Image>();
        tbImg.color = agedGold;
        tbImg.raycastTarget = false;

        GameObject bottomBar = new GameObject("BottomBar", typeof(RectTransform));
        bottomBar.transform.SetParent(cutsceneUI.transform, false);
        RectTransform bbR = bottomBar.GetComponent<RectTransform>();
        bbR.anchorMin = new Vector2(0, 0);
        bbR.anchorMax = new Vector2(1, 0.075f);
        bbR.sizeDelta = Vector2.zero;
        Image bbImg = bottomBar.AddComponent<Image>();
        bbImg.color = new Color(0.051f, 0.031f, 0.024f, 0.85f);
        bbImg.raycastTarget = false;

        GameObject border = new GameObject("TopBorder", typeof(RectTransform));
        border.transform.SetParent(bottomBar.transform, false);
        RectTransform bdr = border.GetComponent<RectTransform>();
        bdr.anchorMin = new Vector2(0, 1);
        bdr.anchorMax = new Vector2(1, 1);
        bdr.sizeDelta = new Vector2(0, 1);
        bdr.anchoredPosition = Vector2.zero;
        Image bImg = border.AddComponent<Image>();
        bImg.color = agedGold;
        bImg.raycastTarget = false;

        cutsceneCounterText = MakeUIText(bottomBar.transform, "Counter", 14, FontStyle.Normal,
            TextAnchor.MiddleLeft, parchmentDark, new Vector2(0.02f, 0), new Vector2(0.25f, 1)).GetComponent<Text>();

        cutsceneTitleText = MakeUIText(bottomBar.transform, "Title", 18, FontStyle.Bold,
            TextAnchor.MiddleCenter, parchmentMid, new Vector2(0.25f, 0), new Vector2(0.75f, 1)).GetComponent<Text>();

        cutsceneCounterText.text = "CUTSCENE";
        cutsceneTitleText.text = factionIndex >= 0 && factionIndex < cutsceneTitles.Length
            ? cutsceneTitles[factionIndex] : "";

        continueButton = new GameObject("ContinueBtn", typeof(RectTransform));
        continueButton.transform.SetParent(cutsceneUI.transform, false);
        RectTransform cbr = continueButton.GetComponent<RectTransform>();
        cbr.anchorMin = new Vector2(0.5f, 0.5f);
        cbr.anchorMax = new Vector2(0.5f, 0.5f);
        cbr.sizeDelta = new Vector2(200, 50);
        cbr.anchoredPosition = new Vector2(0, -150);
        Image cBorder = MakeChild(continueButton.transform, "Border", Vector2.zero, Vector2.one, new Vector2(1, 1)).AddComponent<Image>();
        cBorder.color = agedGold;
        Image cBg = MakeChild(continueButton.transform, "Bg", Vector2.zero, Vector2.one, new Vector2(-2, -2)).AddComponent<Image>();
        cBg.color = inkBrown;
        Text cLabel = MakeUIText(continueButton.transform, "Label", 20, FontStyle.Bold,
            TextAnchor.MiddleCenter, parchmentLight, Vector2.zero, Vector2.one).GetComponent<Text>();
        cLabel.text = "CONTINUE \u2192";

        Button cBtn = continueButton.AddComponent<Button>();
        cBtn.targetGraphic = cBg;
        Navigation cNav = new Navigation();
        cNav.mode = Navigation.Mode.None;
        cBtn.navigation = cNav;
        cBtn.onClick.AddListener(OnContinueClicked);
        continueButton.SetActive(false);

        EventTrigger cEt = continueButton.AddComponent<EventTrigger>();
        AddTrigger(cEt, EventTriggerType.PointerEnter, () => { cBg.color = new Color(0.176f, 0.122f, 0.078f); cBorder.color = parchmentMid; });
        AddTrigger(cEt, EventTriggerType.PointerExit, () => { cBg.color = inkBrown; cBorder.color = agedGold; });

        skipAllButton = new GameObject("SkipAllBtn", typeof(RectTransform));
        skipAllButton.transform.SetParent(cutsceneUI.transform, false);
        RectTransform sbr = skipAllButton.GetComponent<RectTransform>();
        sbr.anchorMin = new Vector2(1f, 1f);
        sbr.anchorMax = new Vector2(1f, 1f);
        sbr.sizeDelta = new Vector2(120, 35);
        sbr.anchoredPosition = new Vector2(-20, -45);
        Image sBg = skipAllButton.AddComponent<Image>();
        sBg.color = deepBrown;
        Text sLabel = MakeUIText(skipAllButton.transform, "Label", 12, FontStyle.Normal,
            TextAnchor.MiddleCenter, parchmentDark, Vector2.zero, Vector2.one).GetComponent<Text>();
        sLabel.text = "SKIP";

        Button sBtn = skipAllButton.AddComponent<Button>();
        sBtn.targetGraphic = sBg;
        Navigation sNav = new Navigation();
        sNav.mode = Navigation.Mode.None;
        sBtn.navigation = sNav;

        EventTrigger sEt = skipAllButton.AddComponent<EventTrigger>();
        AddTrigger(sEt, EventTriggerType.PointerEnter, () => sBg.color = new Color(0.176f, 0.122f, 0.078f));
        AddTrigger(sEt, EventTriggerType.PointerExit, () => sBg.color = deepBrown);

        sBtn.onClick.AddListener(OnSkipAll);
    }

    void OnContinueClicked()
    {
        isWaitingForContinue = false;
        continueButton.SetActive(false);
    }

    void OnSkipAll()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();
        isWaitingForContinue = false;
        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f, 0.3f));
        StartCoroutine(DelayedCleanup(0.3f));
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (continueButton != null)
            continueButton.SetActive(true);
    }

    IEnumerator DelayedCleanup(float delay)
    {
        yield return new WaitForSeconds(delay);
        Cleanup();
        onComplete?.Invoke();
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeCanvasGroup == null) yield break;
        fadeCanvasGroup.blocksRaycasts = true;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = to;
        if (to == 0f)
            fadeCanvasGroup.blocksRaycasts = false;
    }

    void Cleanup()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }
        if (canvas != null)
        {
            Destroy(canvas.gameObject);
            canvas = null;
        }
        if (videoPlayer != null)
        {
            Destroy(videoPlayer);
            videoPlayer = null;
        }
        videoOutputGO = null;
        rawImageOutput = null;
        cutsceneUI = null;
    }

    void AddTrigger(EventTrigger et, EventTriggerType type, System.Action action)
    {
        if (et == null) return;
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((data) => action());
        et.triggers.Add(entry);
    }

    GameObject MakeUIText(Transform parent, string name, int size, FontStyle style, TextAnchor align, Color color, Vector2 aMin, Vector2 aMax)
    {
        if (parent == null) return null;
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = GetFont();
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = color;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        return go;
    }

    GameObject MakeChild(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 sizeDelta)
    {
        if (parent == null) return null;
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.sizeDelta = sizeDelta;
        r.anchoredPosition = Vector2.zero;
        return go;
    }

    static Font GetFont() => UIFont.Get();

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Re-encode Cutscenes for VideoPlayer")]
    public static void ReencodeCutscenes()
    {
        string dir = System.IO.Path.Combine(Application.streamingAssetsPath, "Cutscenes");
        if (!System.IO.Directory.Exists(dir))
        {
            UnityEngine.Debug.LogError("Directory not found: " + dir);
            return;
        }
        string[] files = System.IO.Directory.GetFiles(dir, "*.mp4");
        foreach (string file in files)
        {
            string outPath = file.Replace(".mp4", "_encoded.mp4");
            string args = $"-y -i \"{file}\" -c:v libx264 -profile:v baseline -pix_fmt yuv420p -movflags +faststart -c:a aac -b:a 160k \"{outPath}\"";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "ffmpeg";
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();
            if (p.ExitCode == 0)
            {
                System.IO.File.Delete(file);
                System.IO.File.Move(outPath, file);
            }
            else
            {
                UnityEngine.Debug.LogError("FFmpeg failed for: " + file);
            }
        }
        UnityEngine.Debug.Log("Re-encoded cutscenes to Unity-compatible H.264 MP4.");
        UnityEditor.AssetDatabase.Refresh();
    }
#endif
}
