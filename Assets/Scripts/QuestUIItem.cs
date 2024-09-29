using UnityEngine;
using UnityEngine.UI;
using System;

public class QuestUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text progressText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Image completionCheckmark; 
    [SerializeField] private Text rewardText;

    public event Action OnRewardClaimed;

    private int questId;

    public void SetupQuest(QuestData quest)
    {
        questId = quest.id;
        iconImage.sprite = quest.icon;
        descriptionText.text = quest.description;
        rewardText.text = quest.GetRewardText();

        UpdateProgress(quest);

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(ClaimReward);
        UpdateQuestState(quest);
    }

    private void UpdateProgress(QuestData quest)
    {
        int currentProgress = QuestSystem.Instance.GetQuestProgress(quest.id);
        progressText.text = $"{currentProgress}/{quest.requiredProgress}";
    }

    private void UpdateQuestState(QuestData quest)
    {
        bool isCompleted = QuestSystem.Instance.IsQuestCompleted(quest.id);
        bool isRewardClaimed = QuestSystem.Instance.IsRewardClaimed(quest.id);

        claimButton.gameObject.SetActive(isCompleted && !isRewardClaimed);
        completionCheckmark.gameObject.SetActive(isRewardClaimed);
    }

    private void ClaimReward()
    {
        QuestSystem.Instance.ClaimReward(questId);
        QuestData quest = QuestSystem.Instance.GetQuestById(questId);
        if (quest != null)
        {
            UpdateProgress(quest);
            UpdateQuestState(quest);
            OnRewardClaimed?.Invoke();
        }
    }
}