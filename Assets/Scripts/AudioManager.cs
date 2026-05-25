using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip backsoundClip;
    private AudioSource audioSource;

    private string[] silentScenes = new string[]
    {
        "Scan_Story1",
        "Scan_Story2",
        "Scan_Story3",
        "Scan_Story4",
        "freeplay"
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ← ini yang bikin survive antar scene

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backsoundClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        PlayOrStop(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayOrStop(scene.name);
    }

    void PlayOrStop(string sceneName)
    {
        bool isSilent = System.Array.Exists(silentScenes, s => s == sceneName);
        if (isSilent)
            audioSource.Stop();
        else
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}