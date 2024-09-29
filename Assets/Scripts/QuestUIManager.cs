using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class QuestUIManager : MonoBehaviour
{
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private Transform questContainer;
    [SerializeField] private Button closeButton;
    [SerializeField] private Text coinsText;
    [SerializeField] private Text updateTimeText;
    [SerializeField] private AudioClip claimQuestSound;
    private List<QuestUIItem> questItems = new List<QuestUIItem>();
    private Coroutine updateTimeCoroutine;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseQuestPanel);
        }
        else
        {
            Debug.LogError("Close button is not assigned in QuestUIManager");
        }
    }

    private void OnEnable()
    {
        RefreshQuestDisplay();
        StartUpdateTimeCoroutine();
    }

    private void OnDisable()
    {
        StopUpdateTimeCoroutine();
    }

    public void RefreshQuestDisplay()
    {
        UpdateQuestDisplay();
        UpdateCoinsDisplay();
        UpdateTimeDisplay();
    }

    private void UpdateQuestDisplay()
    {
        Debug.Log("UpdateQuestDisplay called");
        ClearQuestItems();
        List<QuestData> activeQuests = QuestSystem.Instance.GetActiveQuests();
        Debug.Log($"Number of active quests: {activeQuests.Count}");
        
        foreach (QuestData quest in activeQuests)
        {
            GameObject questItemObject = Instantiate(questItemPrefab, questContainer);
            questItemObject.SetActive(true); // Явно активируем объект
            QuestUIItem questItem = questItemObject.GetComponent<QuestUIItem>();
            
            if (questItem != null)
            {
                questItem.SetupQuest(quest);
                questItem.OnRewardClaimed += UpdateCoinsDisplay;
                questItems.Add(questItem);
                Debug.Log($"Quest item created and activated for quest ID: {quest.id}, Description: {quest.description}");
            }
            else
            {
                Debug.LogError($"QuestUIItem component not found on instantiated prefab for quest ID: {quest.id}");
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(questContainer as RectTransform);
        Debug.Log($"Total quest items after update: {questItems.Count}");
    }

    private void ClearQuestItems()
    {
        Debug.Log($"Clearing {questItems.Count} quest items");
        foreach (QuestUIItem item in questItems)
        {
            if (item != null)
            {
                item.OnRewardClaimed -= UpdateCoinsDisplay;
                Destroy(item.gameObject);
                Debug.Log($"Destroyed quest item: {item.name}");
            }
        }
        questItems.Clear();
        
        // Дополнительная проверка, чтобы убедиться, что все дочерние объекты удалены
        foreach (Transform child in questContainer)
        {
            Destroy(child.gameObject);
            Debug.Log($"Destroyed remaining child: {child.name}");
        }
    }

    private void UpdateCoinsDisplay()
    {
        if (coinsText != null && ResourceManager.Instance != null)
        {
            coinsText.text = "" + ResourceManager.Instance.Coins;
            SoundManager.Instance.PlaySound(claimQuestSound);
        }
    }

    private void UpdateTimeDisplay()
    {
        if (updateTimeText != null && QuestSystem.Instance != null)
        {
            TimeSpan timeUntilRefresh = QuestSystem.Instance.GetTimeUntilNextRefresh();
            updateTimeText.text = $"Обновление через: {timeUntilRefresh.Hours:D2}:{timeUntilRefresh.Minutes:D2}:{timeUntilRefresh.Seconds:D2}";
        }
    }

    private void StartUpdateTimeCoroutine()
    {
        if (updateTimeCoroutine != null)
        {
            StopCoroutine(updateTimeCoroutine);
        }
        updateTimeCoroutine = StartCoroutine(UpdateTimeCoroutine());
    }

    private void StopUpdateTimeCoroutine()
    {
        if (updateTimeCoroutine != null)
        {
            StopCoroutine(updateTimeCoroutine);
            updateTimeCoroutine = null;
        }
    }

    private IEnumerator UpdateTimeCoroutine()
    {
        while (true)
        {
            UpdateTimeDisplay();
            yield return new WaitForSeconds(1f);
        }
    }

    private void CloseQuestPanel()
    {
        gameObject.SetActive(false);
        
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