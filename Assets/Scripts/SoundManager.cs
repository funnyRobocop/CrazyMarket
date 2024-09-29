using UnityEngine;
using UnityEngine.UI;
using YG;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioSource gameSounds;
    [SerializeField] private AudioClip buttonClickSound;
     public AudioClip tripleSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (gameSounds == null)
        {
            gameSounds = gameObject.AddComponent<AudioSource>();
        }

        // Загружаем сохраненную громкость
        YandexGame.GetDataEvent += LoadVolume;
        if (YandexGame.SDKEnabled)
        {
            LoadVolume();
        }
    }

    private void Start()
    {
        
    }

    private void LoadVolume()
    {
        if (gameSounds != null)
        {
            gameSounds.volume = YandexGame.savesData.soundVolume;
        }
    }

    public void PlayButtonClickSound()
    {
        PlaySound(buttonClickSound);
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && gameSounds != null)
        {
            gameSounds.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Attempted to play a null audio clip or AudioSource is missing.");
        }
    }

    

    private void OnDestroy()
    {
        YandexGame.GetDataEvent -= LoadVolume;
    }
}