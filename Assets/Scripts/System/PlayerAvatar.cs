using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerAvatar : MonoBehaviour
{
    private Image avatarImage;
    private bool isInitialized = false;

    private void Awake()
    {
        avatarImage = AvatarManager.Instance.currentAvatarImage;
        if (avatarImage == null)
        {
            Debug.LogError("PlayerAvatar: Компонент Image не найден на этом GameObject.");
        }
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            StartCoroutine(WaitForAvatarManagerAndUpdate());
        }
        else
        {
            UpdateAvatar();
        }
    }

    private IEnumerator WaitForAvatarManagerAndUpdate()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (AvatarManager.Instance == null && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (AvatarManager.Instance != null)
        {
            UpdateAvatar();
            isInitialized = true;
        }
        else
        {
            Debug.LogError("PlayerAvatar: AvatarManager не был инициализирован в течение 5 секунд.");
        }
    }

    public void UpdateAvatar()
    {
        if (AvatarManager.Instance != null && avatarImage != null)
        {
            Sprite newSprite = AvatarManager.Instance.GetCurrentAvatarSprite();
            if (newSprite != null)
            {
                avatarImage.sprite = newSprite;
            }
            else
            {
                Debug.LogWarning("PlayerAvatar: Получен нулевой спрайт от AvatarManager");
            }
        }
        else
        {
            if (AvatarManager.Instance == null)
                Debug.LogWarning("PlayerAvatar: AvatarManager.Instance равен null");
            if (avatarImage == null)
                Debug.LogWarning("PlayerAvatar: avatarImage равен null");
        }
    }

    public static void UpdateAllAvatars()
    {
        PlayerAvatar[] allAvatars = FindObjectsByType<PlayerAvatar>(FindObjectsSortMode.None);
        foreach (PlayerAvatar avatar in allAvatars)
        {
            avatar.UpdateAvatar();
        }
    }
}