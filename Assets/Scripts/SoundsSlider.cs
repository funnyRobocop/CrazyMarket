using UnityEngine;
using UnityEngine.UI;

public class SoundsSlider : MonoBehaviour
{
    public AudioSource SoundSource;
    public AudioSource MusicSource;
    public Slider SoundSlider;
    public Slider MusicSlider;

    private const float DefaultVolume = 0.6f;

    private void Start()
    {
        // Подписываемся на событие загрузки данных
        SDKWrapper.GetDataEvent += LoadSettings;

        // Если данные уже загружены, сразу применяем настройки
        if (SDKWrapper.SDKEnabled)
        {
            LoadSettings();
        }

        // Подписываемся на события изменения значения слайдеров
        SoundSlider.onValueChanged.AddListener(UpdateSoundVolume);
        MusicSlider.onValueChanged.AddListener(UpdateMusicVolume);
    }

    private void LoadSettings()
    {
        // Загружаем сохраненные настройки
        float soundVolume = SDKWrapper.savesData.soundVolume;
        float musicVolume = SDKWrapper.savesData.musicVolume;

        // Если это первый запуск (значения не были сохранены), используем значение по умолчанию
        if (soundVolume == 0 && musicVolume == 0 && !SDKWrapper.savesData.isFirstSession)
        {
            soundVolume = DefaultVolume;
            musicVolume = DefaultVolume;
        }

        // Устанавливаем значения слайдеров
        SoundSlider.value = soundVolume;
        MusicSlider.value = musicVolume;

        // Применяем громкость
        UpdateSoundVolume(soundVolume);
        UpdateMusicVolume(musicVolume);

        // Отмечаем, что это уже не первый запуск
        if (SDKWrapper.savesData.isFirstSession)
        {
            SDKWrapper.savesData.isFirstSession = false;
            SDKWrapper.SaveProgress();
        }
    }

    private void UpdateSoundVolume(float volume)
    {
        if (SoundSource != null)
        {
            SoundSource.volume = volume;
        }
        SDKWrapper.savesData.soundVolume = volume;
        SDKWrapper.SaveProgress();
    }

    private void UpdateMusicVolume(float volume)
    {
        if (MusicSource != null)
        {
            MusicSource.volume = volume;
        }
        SDKWrapper.savesData.musicVolume = volume;
        SDKWrapper.SaveProgress();
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        SDKWrapper.GetDataEvent -= LoadSettings;
        if (SoundSlider != null) SoundSlider.onValueChanged.RemoveListener(UpdateSoundVolume);
        if (MusicSlider != null) MusicSlider.onValueChanged.RemoveListener(UpdateMusicVolume);
    }
}