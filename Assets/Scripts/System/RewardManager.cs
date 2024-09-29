using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using YG;

public class RewardsManager : MonoBehaviour
{

    public static RewardsManager Instance { get; private set; }
    public enum RewardType
    {
        Coins,
        Background,
        CharacterIcon
    }

    [System.Serializable]
    public class Reward
    {
        public string id;
        public string rewardName;
        public RewardType rewardType;
        public int starsRequired;
        public Sprite rewardSprite;
        public int coinAmount;
        public int unlockIndex;
    }

    public List<Reward> rewards = new List<Reward>();
    public GameObject rewardPrefab;
    public ScrollRect scrollRect;
    //public Text totalStarsText;

    public Button openRewardPanelButton;
    public Button closeRewardPanelButton;
    public GameObject rewardPanel;
    public AudioClip claimReward;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        InitializeRewards();
        
        SetupButtonListeners();
    }

    private void InitializeRewards()
    {
        if (scrollRect == null || scrollRect.content == null)
        {
            Debug.LogError("ScrollRect or its content is null!");
            return;
        }

        foreach (Transform child in scrollRect.content)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < rewards.Count; i++)
        {
            rewards[i].id = $"reward_{i}";
            if (rewardPrefab == null)
            {
                Debug.LogError("Reward prefab is null!");
                continue;
            }

            GameObject rewardObject = Instantiate(rewardPrefab, scrollRect.content);
            RewardUI rewardUI = rewardObject.GetComponent<RewardUI>();
            if (rewardUI != null)
            {
                rewardUI.Initialize(rewards[i], this);
            }
            else
            {
                Debug.LogError("RewardUI component not found on instantiated prefab!");
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content as RectTransform);
    }

    private void SetupButtonListeners()
    {
        if (openRewardPanelButton != null) openRewardPanelButton.onClick.AddListener(OpenRewardPanel);
        if (closeRewardPanelButton != null) closeRewardPanelButton.onClick.AddListener(CloseRewardPanel);
        
    }

    public void OpenRewardPanel()
    {
        if (rewardPanel != null) rewardPanel.SetActive(true);
    }

    public void CloseRewardPanel()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    

   public bool ClaimReward(Reward reward)
    {
        if (YandexGame.savesData.stars < reward.starsRequired)
        {
            Debug.LogWarning("Not enough stars to claim this reward!");
            return false;
        }

        if (YandexGame.savesData.unlockedRewards[int.Parse(reward.id.Split('_')[1])])
        {
            Debug.LogWarning("This reward has already been claimed!");
            return false;
        }

        switch (reward.rewardType)
        {
            case RewardType.Coins:
                YandexGame.savesData.coins += reward.coinAmount;
                Debug.Log($"Получено {reward.coinAmount} монет");
                break;
            case RewardType.Background:
                YandexGame.savesData.unlockedBackgrounds[reward.unlockIndex] = true;
                Debug.Log($"Разблокирован фон с индексом {reward.unlockIndex}");
                break;
            case RewardType.CharacterIcon:
                YandexGame.savesData.unlockedAvatars[reward.unlockIndex] = true;
                Debug.Log($"Разблокирована иконка персонажа с индексом {reward.unlockIndex}");
                break;
        }

        int rewardIndex = int.Parse(reward.id.Split('_')[1]);
        YandexGame.savesData.unlockedRewards[rewardIndex] = true;
        SoundManager.Instance.PlaySound(claimReward);
        YandexGame.SaveProgress();

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.RefreshUI();
        }
        else
        {
            Debug.LogWarning("MenuManager.Instance is null, unable to refresh UI");
        }

        return true;
    }

    public bool HasUnclaimedRewards()
    {
        for (int i = 0; i < rewards.Count; i++)
        {
            if (YandexGame.savesData.stars >= rewards[i].starsRequired && !YandexGame.savesData.unlockedRewards[i])
            {
                return true;
            }
        }
        return false;
    }



   
}
    

    

    
