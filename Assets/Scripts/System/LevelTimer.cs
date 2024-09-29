using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using YG;

public class LevelTimer : MonoBehaviour
{
    private float totalTime = 240f;
    [SerializeField] private Text timerText;
    [SerializeField] private Image[] characterFaces;
    [SerializeField] private GameObject facePanel;

    private float remainingTime;
    private bool isTimerRunning = false;
    private bool isTimerFrozen = false;

    private void Start()
    {
        for (int i = 0; i < YandexGame.savesData.currentLevel; i++)
        {
            if (totalTime >= 60f)
            {
                totalTime -= 1f;
            }
        }
        remainingTime = totalTime;
        UpdateTimerDisplay();
        StartTimer();
    }

    public float GetTotalTime()
    {
        return totalTime;
    }

    private void StartTimer()
    {
        isTimerRunning = true;
        StartCoroutine(CountdownTimer());
    }

     private IEnumerator CountdownTimer()
    {
        while (remainingTime > 0 && isTimerRunning)
        {
            yield return new WaitForSecondsRealtime(1f);
            if (!isTimerFrozen)
            {
                remainingTime--;
                UpdateTimerDisplay();
                UpdateCharacterFace();
            }
        }

        if (remainingTime <= 0)
        {
            GameOver();
        }
    }

    public void FreezeTimer()
    {
        isTimerFrozen = true;
    }

    public void UnfreezeTimer()
    {
        isTimerFrozen = false;
    }


    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void UpdateCharacterFace()
    {
        float progress = 1 - (remainingTime / totalTime);

        if (progress >= 2f / 3f)
        {
            SetActiveFace(2);
        }
        else if (progress >= 1f / 3f)
        {
            SetActiveFace(1);
        }
        else
        {
            SetActiveFace(0);
        }
    }

    private void SetActiveFace(int index)
    {
        for (int i = 0; i < characterFaces.Length; i++)
        {
            characterFaces[i].gameObject.SetActive(i == index);
        }
    }

    private void GameOver()
    {
        isTimerRunning = false;
        HideFacePanel();
        LevelManager.Instance.ShowLosePanel();
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void HideFacePanel()
    {
        if (facePanel != null)
        {
            facePanel.SetActive(false);
        }
    }
}