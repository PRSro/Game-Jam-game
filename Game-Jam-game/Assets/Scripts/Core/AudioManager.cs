using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private AudioSource audioSource;
    private List<AudioClip> musicClips = new List<AudioClip>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false; // We handle looping manually to switch tracks
        audioSource.volume = 0.5f;

        LoadMusic();
    }

    void Start()
    {
        PlayRandomMusic();
    }

    void LoadMusic()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Music/Music");
        if (clips != null && clips.Length > 0)
        {
            musicClips.AddRange(clips);
            Debug.Log($"[AudioManager] Loaded {musicClips.Count} music tracks.");
        }
        else
        {
            Debug.LogWarning("[AudioManager] No music found in Resources/Music/Music.");
        }
    }

    public void PlayRandomMusic()
    {
        if (musicClips.Count == 0) return;
        
        int randomIndex = Random.Range(0, musicClips.Count);
        audioSource.clip = musicClips[randomIndex];
        audioSource.Play();
        
        // Start a coroutine to play next track when this one finishes
        StopAllCoroutines();
        StartCoroutine(WaitForTrackEnd());
    }
    
    IEnumerator WaitForTrackEnd()
    {
        while (audioSource.isPlaying)
        {
            yield return new WaitForSeconds(1f);
        }
        PlayRandomMusic();
    }
}
