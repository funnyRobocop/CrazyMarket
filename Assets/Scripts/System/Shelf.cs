using System;
using UnityEngine;
using System.Collections;

public class Shelf : MonoBehaviour
{
    public Slot[] slots_front;
    public Slot[] slots_back;
    [SerializeField] private GameObject particleSystemPrefab;

    private void Start()
    {
        int counter = 0;
        for (int i = 0; i < slots_back.Length; i++)
        {
        if (slots_back[i].transform.childCount == 0)    
        {
        counter++;   
        }
        }
    if (counter >= 3){
    for (int i = 0; i < slots_back.Length; i++)   
     {
        slots_back[i].gameObject.SetActive(false);    
        }
        }
    }

    void Update()
    {
        CheckForMatchingItems();
        
        CheckShelf();
    }

    void CheckForMatchingItems()
    {
        if (slots_front == null || slots_front.Length < 3)
        {
            Debug.LogWarning("Not enough slots for matching.");
            return;
        }

        FruitType? itemType = null;
        Vector3 matchCenter = Vector3.zero;
        int matchCount = 0;

        for (int i = 0; i < slots_front.Length; i++)
        {
            if (slots_front[i] == null)
            {
                Debug.LogError($"Slot at index {i} is null!");
                continue;
            }

            if (!slots_front[i].IsOccupied) 
            {
                itemType = null;
                matchCount = 0;
                matchCenter = Vector3.zero;
                continue;
            }

            FruitType? currentFruitType = slots_front[i].GetFruitType();
            if (currentFruitType == null)
            {
                Debug.LogWarning($"Occupied slot at index {i} has no fruit type!");
                continue;
            }

            if (itemType == null)
            {
                itemType = currentFruitType;
                matchCount = 1;
                matchCenter = slots_front[i].transform.position;
            }
            else if (itemType == currentFruitType)
            {
                matchCount++;
                matchCenter += slots_front[i].transform.position;

                if (matchCount == 3)
                {
                    ClearMatchedItems(i - 2, i);
                    PlayParticleEffect(matchCenter / 3);
                    return;
                }
            }
            else
            {
                itemType = currentFruitType;
                matchCount = 1;
                matchCenter = slots_front[i].transform.position;
            }
        }
    }

    void ClearMatchedItems(int startIndex, int endIndex)
    {
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i < 0 || i >= slots_front.Length)
            {
                Debug.LogError($"Invalid slot index: {i}");
                continue;
            }

            Slot slot = slots_front[i];
            if (slot == null)
            {
                Debug.LogError($"Slot at index {i} is null!");
                continue;
            }

            if (slot.IsOccupied)
            {
                DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
                else
                {
                    Debug.LogWarning($"No DraggableItem found in occupied slot at index {i}");
                }
                slot.ClearSlot();
            }
        }
        
        //Включаем задний слой

        for (int i = 0; i < slots_back.Length; i++)
        {
            if (slots_back[i].transform.childCount > 1)
            {
                slots_back[i].GetComponent<BoxCollider2D>().enabled = true;
                
                slots_back[i].transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                
                slots_back[i].transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;
                slots_back[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder ++;

                slots_back[i].transform.GetChild(1).GetComponent<DraggableItem>().enabled = true;
                slots_back[i].transform.GetChild(1).SetParent(slots_front[i].transform);
                slots_back[i].gameObject.SetActive(false);
            }
        }

        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.UpdateQuestProgress(QuestType.FruitMatch);
        }
        else
        {
            Debug.LogWarning("QuestSystem.Instance is null!");
        }
        
        StartCoroutine(CheckWinAfterDelay());
    }

    void PlayParticleEffect(Vector3 position)
    {
        if (particleSystemPrefab != null)
        {
            GameObject particleInstance = Instantiate(particleSystemPrefab, position, Quaternion.identity);
            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            SoundManager.Instance.PlaySound(SoundManager.Instance.tripleSound);
            
            if (particleSystem != null)
            {
                particleSystem.Play();
                float totalDuration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
                Destroy(particleInstance, totalDuration);
            }
            else
            {
                Debug.LogError("ParticleSystem component not found on the instantiated prefab.");
                Destroy(particleInstance);
            }
        }
        else
        {
            Debug.LogError("Particle System Prefab is not assigned in the inspector.");
        }
    }

    public void CheckShelf()
    {
        short counter = 0;
        
        for (int i = 0; i < slots_front.Length; i++)
        {
            if (slots_front[i].transform.childCount <= 1)
            {
                counter++;
            }
        }

        if (counter >= 3)
        {
            for (int i = 0; i < slots_back.Length; i++)
            {
                if (slots_back[i].transform.childCount > 1)
                {
                    slots_back[i].transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                    slots_back[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder++;
                
                    slots_back[i].transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;

                    slots_back[i].transform.GetChild(1).GetComponent<DraggableItem>().enabled = true;
                    
                    slots_back[i].transform.GetChild(1).SetParent(slots_front[i].transform);
                }
                
                slots_back[i].gameObject.SetActive(false);
            }
        }
    }

    IEnumerator CheckWinAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CheckWinCondition();
        }
        else
        {
            Debug.LogError("LevelManager instance is null. Unable to check win condition.");
        }
    }
}