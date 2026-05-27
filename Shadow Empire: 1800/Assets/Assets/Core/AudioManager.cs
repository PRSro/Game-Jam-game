using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    const float TARGET_VOLUME    = 0.72f;
    const float CROSSFADE_SECS   = 2.5f;
    const float TENSION_FADE     = 1.2f;

    AudioSource primarySource;
    AudioSource secondarySource;

    Coroutine crossFadeCoroutine;
    Coroutine trackEndWatchCoroutine;
    int       playerFactionId   = -1;
    bool      tensionActive     = false;
    float     primaryClipLength = 0f;

    public class FactionMusicPool
    {
        public List<AudioClip> ambientTracks = new List<AudioClip>();
        public List<AudioClip> tensionTracks = new List<AudioClip>();
        public int ambientIndex = 0;
        public int tensionIndex = 0;
    }

    FactionMusicPool[] factionPools;
    List<AudioClip>    generalPool = new List<AudioClip>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        primarySource   = gameObject.AddComponent<AudioSource>();
        secondarySource = gameObject.AddComponent<AudioSource>();

        primarySource.loop   = false;
        secondarySource.loop = false;
        primarySource.volume   = TARGET_VOLUME;
        secondarySource.volume = 0f;

        factionPools = new FactionMusicPool[4];
        for (int i = 0; i < 4; i++) factionPools[i] = new FactionMusicPool();

        LoadTracks();
    }

    void LoadTracks()
    {
        AudioClip[] allClips = Resources.LoadAll<AudioClip>("Music");
        if (allClips == null || allClips.Length == 0)
        {
            Debug.LogWarning("[AudioManager] No tracks found in Resources/Music/.");
            return;
        }

        foreach (AudioClip clip in allClips)
        {
            string n = clip.name.ToLower();
            bool matched = false;
            for (int f = 0; f < 4; f++)
            {
                string prefix = $"faction{f}_";
                if (!n.StartsWith(prefix)) continue;
                if (n.Contains("_tension_"))
                    factionPools[f].tensionTracks.Add(clip);
                else
                    factionPools[f].ambientTracks.Add(clip);
                matched = true;
                break;
            }
            if (!matched) generalPool.Add(clip);
        }

        const int ambientPerFaction = 4;
        for (int f = 0; f < 4; f++)
        {
            for (int i = 0; i < ambientPerFaction && generalPool.Count > 0; i++)
            {
                int idx = Random.Range(0, generalPool.Count);
                factionPools[f].ambientTracks.Add(generalPool[idx]);
                generalPool.RemoveAt(idx);
            }
        }

        for (int f = 0; f < 4; f++)
        {
            Shuffle(factionPools[f].ambientTracks);
            Shuffle(factionPools[f].tensionTracks);
        }
        Shuffle(generalPool);

        Debug.Log($"[AudioManager] Loaded tracks — ambient per faction, {generalPool.Count} general pool remaining.");
    }

    int TotalFactionTracks()
    {
        int n = 0;
        foreach (var p in factionPools) n += p.ambientTracks.Count + p.tensionTracks.Count;
        return n;
    }

    public void SetPlayerFaction(int factionId)
    {
        playerFactionId = factionId;
        PlayNextAmbient(factionId);
    }

    public void OnEnemyFactionActing(int factionId)
    {
        if (factionId == playerFactionId) return;
        if (factionId < 0 || factionId >= factionPools.Length) return;
        float remaining = primaryClipLength - primarySource.time;
        if (primarySource.isPlaying && remaining > 10f) return;
        if (tensionActive) return;

        List<AudioClip> pool = factionPools[factionId].tensionTracks;
        AudioClip pick = null;

        if (pool.Count > 0)
        {
            int idx = factionPools[factionId].tensionIndex % pool.Count;
            factionPools[factionId].tensionIndex++;
            pick = pool[idx];
        }
        else if (generalPool.Count > 0)
        {
            pick = generalPool[Random.Range(0, generalPool.Count)];
        }

        if (pick != null)
        {
            tensionActive = true;
            CrossFadeTo(pick, TENSION_FADE);
        }
    }

    public void OnResolutionComplete()
    {
        if (playerFactionId < 0) return;
        tensionActive = false;
    }

    void PlayNextAmbient(int factionId)
    {
        if (factionId < 0 || factionId >= factionPools.Length) return;

        List<AudioClip> pool = factionPools[factionId].ambientTracks;
        AudioClip pick = null;

        if (pool.Count > 0)
        {
            int idx = factionPools[factionId].ambientIndex % pool.Count;
            factionPools[factionId].ambientIndex++;
            pick = pool[idx];
        }
        else if (generalPool.Count > 0)
        {
            int idx = Random.Range(0, generalPool.Count);
            pick = generalPool[idx];
        }

        if (pick != null)
            CrossFadeTo(pick, CROSSFADE_SECS);
    }

    void CrossFadeTo(AudioClip clip, float duration)
    {
        if (crossFadeCoroutine != null)     StopCoroutine(crossFadeCoroutine);
        if (trackEndWatchCoroutine != null) StopCoroutine(trackEndWatchCoroutine);

        crossFadeCoroutine = StartCoroutine(DoCrossFade(clip, duration));
    }

    IEnumerator DoCrossFade(AudioClip newClip, float duration)
    {
        float normalisedVolume = GetNormalisedVolume(newClip);

        secondarySource.clip   = newClip;
        secondarySource.volume = 0f;
        secondarySource.Play();

        float startVol  = primarySource.volume;
        float elapsed   = 0f;

        while (elapsed < duration)
        {
            elapsed            += Time.deltaTime;
            float t             = Mathf.Clamp01(elapsed / duration);
            primarySource.volume   = Mathf.Lerp(startVol,         0f,               t);
            secondarySource.volume = Mathf.Lerp(0f,               normalisedVolume, t);
            yield return null;
        }

        primarySource.Stop();

        AudioSource temp = primarySource;
        primarySource    = secondarySource;
        secondarySource  = temp;

        primarySource.volume   = normalisedVolume;
        secondarySource.volume = 0f;
        crossFadeCoroutine     = null;

        primaryClipLength = newClip.length;

        trackEndWatchCoroutine = StartCoroutine(WaitForClipLength(newClip.length));
    }

    IEnumerator WaitForClipLength(float clipLength)
    {
        float waitTime = Mathf.Max(0f, clipLength - CROSSFADE_SECS);
        yield return new WaitForSeconds(waitTime);

        trackEndWatchCoroutine = null;

        if (tensionActive)
            tensionActive = false;

        if (playerFactionId >= 0)
            PlayNextAmbient(playerFactionId);
    }

    float GetNormalisedVolume(AudioClip clip)
    {
        if (clip == null) return TARGET_VOLUME;

        float[] samples = new float[clip.samples * clip.channels];
        bool    ok      = clip.GetData(samples, 0);
        if (!ok) return TARGET_VOLUME;

        float peak = 0f;
        foreach (float s in samples)
        {
            float abs = Mathf.Abs(s);
            if (abs > peak) peak = abs;
        }

        if (peak < 0.001f) return TARGET_VOLUME;

        float scale = TARGET_VOLUME / peak;
        return Mathf.Clamp(scale * TARGET_VOLUME, 0.30f, TARGET_VOLUME);
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j    = Random.Range(0, i + 1);
            T   tmp  = list[i];
            list[i]  = list[j];
            list[j]  = tmp;
        }
    }
}
