using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PiggyBankUI : MonoBehaviour
{
    [SerializeField] private Text coinsAmountText;
    [SerializeField] private Button collectButton;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private GameObject MoneyParticleSystem;
    [SerializeField] private Animator hummerAnimator;
    
    // Новые поля для звука
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip piggyBreakSound;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (PiggyBankManager.Instance != null)
        {
            PiggyBankManager.Instance.OnCoinsChanged += UpdateUI;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (PiggyBankManager.Instance != null)
        {
            PiggyBankManager.Instance.OnCoinsChanged -= UpdateUI;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneManager.GetActiveScene().name)
        {
            UpdateUI();
        }
    }

    private void Start()
    {
        SetupButtons();
        UpdateUI();
        if (PiggyBankManager.Instance != null)
        {
            hummerAnimator.SetBool("canCrash", PiggyBankManager.Instance.CanCollectCoins());
        }

        // Инициализация AudioSource, если не назначен
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Установка громкости в соответствии с настройками игрока
        audioSource.volume = SDKWrapper.savesData.soundVolume;
    }

    private void SetupButtons()
    {
        if (collectButton != null)
            collectButton.onClick.AddListener(OnCollectButtonClicked);
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMenu);
    }

    private void UpdateUI()
    {
        if (PiggyBankManager.Instance == null)
        {
            Debug.LogError("PiggyBankManager.Instance is null!");
            return;
        }

        if (coinsAmountText != null)
            coinsAmountText.text = PiggyBankManager.Instance.GetCurrentCoins().ToString();
        if (collectButton != null)
            collectButton.interactable = PiggyBankManager.Instance.CanCollectCoins();
    }

    private void OnCollectButtonClicked()
    {
        if (PiggyBankManager.Instance == null)
        {
            Debug.LogError("PiggyBankManager.Instance is null!");
            return;
        }
       
        PiggyBankManager.Instance.CollectCoins();
        hummerAnimator.SetBool("canCrash", PiggyBankManager.Instance.CanCollectCoins());
        MoneyParticleSystem.SetActive(true);

        // Воспроизведение звука разбивания копилки
        if (audioSource != null && piggyBreakSound != null)
        {
            audioSource.PlayOneShot(piggyBreakSound);
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MenuScene");
        MoneyParticleSystem.SetActive(false);
    }
}