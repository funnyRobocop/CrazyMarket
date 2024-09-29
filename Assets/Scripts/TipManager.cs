using UnityEngine;
using UnityEngine.UI;

public class TipManager : MonoBehaviour
{
    [SerializeField] private GameObject tipPanel;
    [SerializeField] private Button closeTipButton;

    private LevelTimer levelTimer;
    private LevelManager levelManager;

    private void Start()
    {
        levelTimer = FindObjectOfType<LevelTimer>();
        levelManager = FindObjectOfType<LevelManager>();

        if (levelTimer == null)
        {
            Debug.LogError("LevelTimer не найден на сцене!");
        }

        if (levelManager == null)
        {
            Debug.LogError("LevelManager не найден на сцене!");
        }

        if (closeTipButton != null)
        {
            closeTipButton.onClick.AddListener(CloseTip);
        }
        else
        {
            Debug.LogError("Кнопка закрытия подсказки не назначена!");
        }

        UpdateTipPanelVisibility();
    }

    private void UpdateTipPanelVisibility()
    {
        if (SDKWrapper.savesData.currentLevel <= 10)
        {
            if (tipPanel != null)
            {
                tipPanel.SetActive(true);
                ShowTip();
            }
        }
        else
        {
            if (tipPanel != null)
            {
                tipPanel.SetActive(false);
            }
        }
    }

    private void ShowTip()
    {
        if (SDKWrapper.savesData.currentLevel <= 10)
        {
            tipPanel.SetActive(true);
            PauseGame();
        }
    }

    private void CloseTip()
    {
        tipPanel.SetActive(false);
        ResumeGame();
    }

    private void PauseGame()
    {
        if (levelTimer != null)
        {
            levelTimer.FreezeTimer();
        }

        if (levelManager != null)
        {
            levelManager.PauseGame();
        }
    }

    private void ResumeGame()
    {
        if (levelTimer != null)
        {
            levelTimer.UnfreezeTimer();
        }

        if (levelManager != null)
        {
            levelManager.ResumeGame();
        }
    }
}