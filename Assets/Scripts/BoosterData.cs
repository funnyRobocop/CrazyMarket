using UnityEngine;
using System.Collections.Generic;
using YG;

public class BoosterData : MonoBehaviour
{
    public static BoosterData Instance { get; private set; }

    public enum BoosterType
    {
        Freeze = 0,
        // Добавьте здесь другие типы бустеров по мере необходимости
    }

    [System.Serializable]
    public class BoosterInfo
    {
        public BoosterType type;
        public string name;
        public int price;
    }

    public List<BoosterInfo> boosters;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBoosterData();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        YandexGame.GetDataEvent += OnDataLoaded;
    }

    private void OnDestroy()
    {
        YandexGame.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        InitializeBoosterData();
    }

    private void InitializeBoosterData()
    {
        // Инициализация данных о бустерах из YandexGame.savesData
        // Если данных нет, устанавливаем значения по умолчанию
        for (int i = 0; i < boosters.Count; i++)
        {
            if (i >= YandexGame.savesData.boosterQuantities.Length)
            {
                Debug.LogError($"Booster index {i} is out of range in YandexGame.savesData.boosterQuantities");
                continue;
            }
            if (YandexGame.savesData.boosterQuantities[i] == 0)
            {
                YandexGame.savesData.boosterQuantities[i] = 0;
            }
        }
        YandexGame.SaveProgress();
    }

    public void SaveBoosterData()
    {
        YandexGame.SaveProgress();
    }

    public BoosterInfo GetBoosterData(BoosterType type)
    {
        return boosters.Find(b => b.type == type);
    }

    public int GetBoosterQuantity(BoosterType type)
    {
        int index = (int)type;
        if (index < 0 || index >= YandexGame.savesData.boosterQuantities.Length)
        {
            Debug.LogError($"Invalid booster type index: {index}");
            return 0;
        }
        return YandexGame.savesData.boosterQuantities[index];
    }

    public void AddBooster(BoosterType type, int amount)
    {
        int index = (int)type;
        if (index < 0 || index >= YandexGame.savesData.boosterQuantities.Length)
        {
            Debug.LogError($"Invalid booster type index: {index}");
            return;
        }
        YandexGame.savesData.boosterQuantities[index] += amount;
        SaveBoosterData();
    }

    public bool UseBooster(BoosterType type)
    {
        int index = (int)type;
        if (index < 0 || index >= YandexGame.savesData.boosterQuantities.Length)
        {
            Debug.LogError($"Invalid booster type index: {index}");
            return false;
        }
        if (YandexGame.savesData.boosterQuantities[index] > 0)
        {
            YandexGame.savesData.boosterQuantities[index]--;
            SaveBoosterData();
            Debug.Log($"Использован бустер {type}. Осталось: {YandexGame.savesData.boosterQuantities[index]}");
            return true;
        }
        Debug.Log($"Не удалось использовать бустер {type}. Количество: {YandexGame.savesData.boosterQuantities[index]}");
        return false;
    }
}