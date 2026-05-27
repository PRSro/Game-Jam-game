using UnityEngine;
using UnityEngine.EventSystems;

public static class Initializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);

        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            Object.DontDestroyOnLoad(gm);
        }

        if (AIController.Instance == null)
        {
            GameObject ai = new GameObject("AIController");
            ai.AddComponent<AIController>();
            Object.DontDestroyOnLoad(ai);
        }
        if (Object.FindObjectOfType<CutsceneManager>() == null)
        {
            GameObject cm = new GameObject("CutsceneManager");
            cm.AddComponent<CutsceneManager>();
        }
        if (MainMenuManager.Instance == null)
        {
            GameObject mm = new GameObject("MainMenuManager");
            mm.AddComponent<MainMenuManager>();
        }

        if (GameUIManager.Instance == null)
        {
            GameObject gui = new GameObject("GameUIManager");
            gui.AddComponent<GameUIManager>();
            Object.DontDestroyOnLoad(gui);
        }
        if (Object.FindObjectOfType<MapLayerController>() == null)
        {
            GameObject mlc = new GameObject("MapLayerController");
            mlc.AddComponent<MapLayerController>();
            Object.DontDestroyOnLoad(mlc);
        }
        if (Object.FindObjectOfType<TerritoryGraphRenderer>() == null)
        {
            GameObject tgr = new GameObject("TerritoryGraphRenderer");
            tgr.AddComponent<TerritoryGraphRenderer>();
            Object.DontDestroyOnLoad(tgr);
        }
        if (Object.FindObjectOfType<AttackPhaseUI>() == null)
        {
            GameObject apu = new GameObject("AttackPhaseUI");
            apu.AddComponent<AttackPhaseUI>();
            Object.DontDestroyOnLoad(apu);
        }
        if (Object.FindObjectOfType<MapSystemController>() == null)
        {
            GameObject msc = new GameObject("MapSystemController");
            msc.AddComponent<MapSystemController>();
            Object.DontDestroyOnLoad(msc);
        }

        if (Object.FindObjectOfType<AudioManager>() == null)
        {
            GameObject am = new GameObject("AudioManager");
            am.AddComponent<AudioManager>();
        }

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem));
            try
            {
                es.AddComponent<StandaloneInputModule>();
            }
            catch
            {
                Debug.Log("[Initializer] StandaloneInputModule unavailable; relying on default input module.");
            }
            Object.DontDestroyOnLoad(es);
        }
    }
}
