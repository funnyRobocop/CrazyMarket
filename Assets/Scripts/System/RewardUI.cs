using UnityEngine;
using UnityEngine.UI;
using YG;

public class RewardUI : MonoBehaviour
{
    public Image rewardImage;
    public Text rewardText;
    public Button claimButton;
    public Image lockImage;
    public Image checkmarkImage;

    private RewardsManager.Reward reward;
    private RewardsManager rewardsManager;

    public void Initialize(RewardsManager.Reward reward, RewardsManager manager)
    {
        this.reward = reward;
        this.rewardsManager = manager;

        if (rewardImage != null && reward.rewardSprite != null)
        {
            rewardImage.sprite = reward.rewardSprite;
        }
        else
        {
            Debug.LogError($"RewardImage or RewardSprite is null for reward: {reward.rewardName}");
        }

        UpdateRewardText();
        claimButton.onClick.AddListener(OnClaimButtonClick);
        UpdateUI();
    }

    private void UpdateRewardText()
    {
        string rewardDescription = reward.rewardType switch
        {
            RewardsManager.RewardType.Coins => $"{reward.coinAmount} монет",
            RewardsManager.RewardType.Background => "Новый фон",
            RewardsManager.RewardType.CharacterIcon => "Иконка персонажа",
            _ => reward.rewardName
        };
        rewardText.text = $"{rewardDescription}\n{reward.starsRequired} звезд";
    }

    public void UpdateUI()
    {
        int totalStars = YandexGame.savesData.stars;
        bool isUnlocked = totalStars >= reward.starsRequired;
        bool isClaimed = YandexGame.savesData.unlockedRewards[int.Parse(reward.id.Split('_')[1])];

        rewardImage.gameObject.SetActive(true);
        lockImage.gameObject.SetActive(!isUnlocked);
        checkmarkImage.gameObject.SetActive(isClaimed);
        claimButton.interactable = isUnlocked && !isClaimed;

        if (isClaimed)
        {
            rewardText.text = "Получено";
        }
        else
        {
            UpdateRewardText();
        }
    }

    private void OnClaimButtonClick()
    {
        if (rewardsManager.ClaimReward(reward))
        {
            UpdateUI();
        }
    }
}