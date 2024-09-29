using System.Collections.Generic;

[System.Serializable]
    public class SavesYG
    {
        // "Технические сохранения" для работы плагина (Не удалять)
        public int idSave;
        public bool isFirstSession = true;
        public string language = "ru";
        public bool promptDone;

        // Сохранения для нашего проекта
        public int currentLevel = 1;
        public int currentLives = 10;
        public int maxLives = 10;
        public int stars = 0;
        public int coins = 0;
        public string currentBackgroundId = "0";
        public string currentAvatarId = "0";
        public long lastLiveRegenTimeTicks;

        public float soundVolume = 0.3f;
        public float musicVolume = 0.3f;

        // Сохранения для PiggyBank
        public int piggyBankCoins = 0;

        // Сохранения для DailyGift
        public string lastGiftTimestamp;

        // Сохранения для Boosters
        public int[] boosterQuantities = new int[10]; // Предполагаем, что у нас не более 10 типов бустеров

        // Сохранения для Quest System
        public int[] questProgress = new int[20]; // Предполагаем, что у нас не более 20 квестов
        public bool[] claimedRewards = new bool[20];
        public int[] activeQuestIds = new int[3]; // Предполагаем, что одновременно активны 3 квеста
        public string lastQuestRefreshTime;

        // Сохранения для Rewards
        public bool[] unlockedRewards = new bool[50]; // Предполагаем, что у нас не более 50 наград

        // Сохранения для разблокированных аватаров и фонов
        public bool[] unlockedAvatars = new bool[20]; // Предполагаем, что у нас не более 20 аватаров
        public bool[] unlockedBackgrounds = new bool[20]; // Предполагаем, что у нас не более 20 фонов

        // Конструктор
        public SavesYG()
        {
            // Инициализация значений по умолчанию
            unlockedAvatars[0] = true;
            unlockedBackgrounds[0] = true;
        }
    }