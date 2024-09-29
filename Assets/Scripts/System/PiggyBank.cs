using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PiggyBank : MonoBehaviour
{
    public static PiggyBank Instance { get; private set; }

    [SerializeField] private Text coinsAmountText;
    [SerializeField] private Button collectButton;
    [SerializeField] private int coinsToCollect = 10;
    [SerializeField] private int coinsPerLevel = 2;

    private bool isInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("PiggyBank");
            Instance = go.AddComponent<PiggyBank>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        YandexGame.GetDataEvent += OnDataLoaded;
        if (YandexGame.SDKEnabled)
        {
            OnDataLoaded();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        YandexGame.GetDataEvent -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        isInitialized = true;
        UpdateUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PiggyBankScene") // Замените на имя вашей сцены с копилкой
        {
            SetupUI();
        }
    }

    private void SetupUI()
    {
        coinsAmountText = GameObject.Find("CoinsAmountText")?.GetComponent<Text>();
        collectButton = GameObject.Find("CollectButton")?.GetComponent<Button>();
        if (collectButton != null)
        {
            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(CollectCoins);
        }
        UpdateUI();
    }

    public void AddCoinsForLevelCompletion()
    {
        if (!isInitialized) return;
        YandexGame.savesData.piggyBankCoins += coinsPerLevel;
        YandexGame.SaveProgress();
        UpdateUI();
    }

    private void CollectCoins()
    {
        if (!isInitialized) return;
        if (YandexGame.savesData.piggyBankCoins >= coinsToCollect)
        {
            ResourceManager.Instance.AddCoins(YandexGame.savesData.piggyBankCoins);
            YandexGame.savesData.piggyBankCoins = 0;
            YandexGame.SaveProgress();
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (!isInitialized) return;
        if (coinsAmountText != null)
            coinsAmountText.text = YandexGame.savesData.piggyBankCoins.ToString();
        if (collectButton != null)
            collectButton.interactable = YandexGame.savesData.piggyBankCoins >= coinsToCollect;
    }

    public void ResetPiggyBank()
    {
        if (!isInitialized) return;
        YandexGame.savesData.piggyBankCoins = 0;
        YandexGame.SaveProgress();
        UpdateUI();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MenuScene");
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.RefreshUI();
        }
        else
        {
            Debug.LogWarning("MenuManager.Instance is null, unable to refresh UI");
        }
    }
}