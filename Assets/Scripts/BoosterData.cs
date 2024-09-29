using UnityEngine;
using System.Collections.Generic;

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
        SDKWrapper.GetDataEvent += OnDataLoaded;
    }

    private void OnDestroy()
    {
        SDKWrapper.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        InitializeBoosterData();
    }

    private void InitializeBoosterData()
    {
        // Инициализация данных о бустерах из SDKWrapper.savesData
        // Если данных нет, устанавливаем значения по умолчанию
        for (int i = 0; i < boosters.Count; i++)
        {
            if (i >= SDKWrapper.savesData.boosterQuantities.Length)
            {
                Debug.LogError($"Booster index {i} is out of range in SDKWrapper.savesData.boosterQuantities");
                continue;
            }
            if (SDKWrapper.savesData.boosterQuantities[i] == 0)
            {
                SDKWrapper.savesData.boosterQuantities[i] = 0;
            }
        }
        SDKWrapper.SaveProgress();
    }

    public void SaveBoosterData()
    {
        SDKWrapper.SaveProgress();
    }

    public BoosterInfo GetBoosterData(BoosterType type)
    {
        return boosters.Find(b => b.type == type);
    }

    public int GetBoosterQuantity(BoosterType type)
    {
        int index = (int)type;
        if (index < 0 || index >= SDKWrapper.savesData.boosterQuantities.Length)
        {
            Debug.LogError($"Invalid booster type index: {index}");
            return 0;
        }
        return SDKWrapper.savesData.boosterQuantities[index];
    }

    public void AddBooster(BoosterType type, int amount)
    {
        int index = (int)type;
        if (index < 0 || index >= SDKWrapper.savesData.boosterQuantities.Length)
        {
            Debug.LogError($"Invalid booster type index: {index}");
            return;
        }
        SDKWrapper.savesData.boosterQuantities[index] += amount;
        SaveBoosterData();
    }

    public bool UseBooster(BoosterType type)
    {
        int index = (int)type;
        if (index < 0 || index >= SDKWrapper.savesData.boosterQuantities.Length)
        {
            Debug.LogError($"Invalid booster type index: {index}");
            return false;
        }
        if (SDKWrapper.savesData.boosterQuantities[index] > 0)
        {
            SDKWrapper.savesData.boosterQuantities[index]--;
            SaveBoosterData();
            Debug.Log($"Использован бустер {type}. Осталось: {SDKWrapper.savesData.boosterQuantities[index]}");
            return true;
        }
        Debug.Log($"Не удалось использовать бустер {type}. Количество: {SDKWrapper.savesData.boosterQuantities[index]}");
        return false;
    }
}