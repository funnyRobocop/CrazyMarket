using UnityEngine;
using UnityEngine.UI;
using YG;

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
        YandexGame.GetDataEvent += LoadSettings;

        // Если данные уже загружены, сразу применяем настройки
        if (YandexGame.SDKEnabled)
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
        float soundVolume = YandexGame.savesData.soundVolume;
        float musicVolume = YandexGame.savesData.musicVolume;

        // Если это первый запуск (значения не были сохранены), используем значение по умолчанию
        if (soundVolume == 0 && musicVolume == 0 && !YandexGame.savesData.isFirstSession)
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
        if (YandexGame.savesData.isFirstSession)
        {
            YandexGame.savesData.isFirstSession = false;
            YandexGame.SaveProgress();
        }
    }

    private void UpdateSoundVolume(float volume)
    {
        if (SoundSource != null)
        {
            SoundSource.volume = volume;
        }
        YandexGame.savesData.soundVolume = volume;
        YandexGame.SaveProgress();
    }

    private void UpdateMusicVolume(float volume)
    {
        if (MusicSource != null)
        {
            MusicSource.volume = volume;
        }
        YandexGame.savesData.musicVolume = volume;
        YandexGame.SaveProgress();
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        YandexGame.GetDataEvent -= LoadSettings;
        if (SoundSlider != null) SoundSlider.onValueChanged.RemoveListener(UpdateSoundVolume);
        if (MusicSlider != null) MusicSlider.onValueChanged.RemoveListener(UpdateMusicVolume);
    }
}