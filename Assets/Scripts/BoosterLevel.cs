using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BoosterLevel : MonoBehaviour
{
    [System.Serializable]
    public class BoosterButton
    {
        public BoosterData.BoosterType type;
        public Button button;
        public Text quantityText;
    }

    public List<BoosterButton> boosterButtons;
    private LevelTimer levelTimer;

    [Header("Freeze Effect")]
    public GameObject freezeEffectPanel;
    public ParticleSystem snowflakesParticleSystem;
    public AudioClip freezeSound;

    private void Start()
    {
        SetupBoosterButtons();
        InitializeLevelTimer();
        InitializeFreezeEffect();
    }

    private void SetupBoosterButtons()
    {
        foreach (var boosterButton in boosterButtons)
        {
            if (boosterButton.button != null)
            {
                boosterButton.button.onClick.RemoveAllListeners();
                boosterButton.button.onClick.AddListener(() => UseBooster(boosterButton.type));
                Debug.Log($"Настроена кнопка для бустера: {boosterButton.type}");
            }
            else
            {
                Debug.LogError($"Кнопка для бустера {boosterButton.type} не назначена в инспекторе!");
            }
            UpdateBoosterUI(boosterButton);
        }
    }

    private void InitializeLevelTimer()
    {
        levelTimer = FindObjectOfType<LevelTimer>();
        if (levelTimer == null)
        {
            Debug.LogError("LevelTimer не найден на сцене!");
        }
    }

    private void InitializeFreezeEffect()
    {
        if (freezeEffectPanel != null)
        {
            freezeEffectPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Панель эффекта заморозки не назначена!");
        }

        if (snowflakesParticleSystem != null)
        {
            snowflakesParticleSystem.gameObject.SetActive(false);
            snowflakesParticleSystem.Stop();
            snowflakesParticleSystem.Clear();
        }
        else
        {
            Debug.LogWarning("Система частиц снежинок не назначена!");
        }
    }

    private void UseBooster(BoosterData.BoosterType type)
    {
        Debug.Log($"Попытка использовать бустер: {type}");
        if (BoosterData.Instance.UseBooster(type))
        {
            Debug.Log($"Бустер {type} успешно использован");
            ApplyBoosterEffect(type);
            UpdateBoosterUI(boosterButtons.Find(b => b.type == type));
        }
        else
        {
            Debug.Log($"Не удалось использовать бустер {type}. Возможно, их нет в наличии.");
        }
    }

    private void ApplyBoosterEffect(BoosterData.BoosterType type)
    {
        Debug.Log($"Применение эффекта бустера: {type}");
        switch (type)
        {
            case BoosterData.BoosterType.Freeze:
                StartCoroutine(FreezeTimeCoroutine(15f));
                SoundManager.Instance.PlaySound(freezeSound);
                break;
            // Добавьте другие типы бустеров здесь
            default:
                Debug.LogWarning($"Неизвестный тип бустера: {type}");
                break;
        }
    }

    private IEnumerator FreezeTimeCoroutine(float duration)
    {
        Debug.Log($"Начало эффекта заморозки на {duration} секунд");
        if (levelTimer != null)
        {
            levelTimer.FreezeTimer();
            
            // Включаем визуальные эффекты заморозки
            if (freezeEffectPanel != null)
            {
                freezeEffectPanel.SetActive(true);
                Debug.Log("Панель эффекта заморозки активирована");
            }
            if (snowflakesParticleSystem != null)
            {
                snowflakesParticleSystem.gameObject.SetActive(true);
                snowflakesParticleSystem.Clear();
                snowflakesParticleSystem.Play();
                Debug.Log("Система частиц снежинок активирована и запущена");
            }
            else
            {
                Debug.LogError("Система частиц снежинок не найдена!");
            }

            yield return new WaitForSeconds(duration);

            levelTimer.UnfreezeTimer();

            // Выключаем визуальные эффекты заморозки
            if (freezeEffectPanel != null)
            {
                freezeEffectPanel.SetActive(false);
                Debug.Log("Панель эффекта заморозки деактивирована");
            }
            if (snowflakesParticleSystem != null)
            {
                snowflakesParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                snowflakesParticleSystem.gameObject.SetActive(false);
                Debug.Log("Система частиц снежинок остановлена и деактивирована");
            }

            Debug.Log("Эффект заморозки закончился");
        }
        else
        {
            Debug.LogError("LevelTimer не найден, невозможно применить эффект заморозки!");
        }
    }

    private void UpdateBoosterUI(BoosterButton boosterButton)
    {
        int quantity = BoosterData.Instance.GetBoosterQuantity(boosterButton.type);
        boosterButton.quantityText.text = quantity.ToString();
        boosterButton.button.interactable = quantity > 0;
    }

    public void RefreshBoostersUI()
    {
        foreach (var boosterButton in boosterButtons)
        {
            UpdateBoosterUI(boosterButton);
        }
    }
}