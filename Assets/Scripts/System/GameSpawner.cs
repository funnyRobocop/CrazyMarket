using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameSpawner : MonoBehaviour
{
    [SerializeField] private LevelInfo_SO levelInfo;
    [SerializeField] private Transform[] slots_layerFront;
    [SerializeField] private Transform[] slots_layerBack;

    [SerializeField] private GameObject[] fruitPrefabs;
    private Dictionary<FruitType, GameObject> fruitPrefabDict;

    private void Awake()
    {
        InitializeFruitPrefabs();
    }

    private void Start()
    {
        List<InfoItemsList> itemList = levelInfo.GenerateItemList();
        GenerateRow(slots_layerFront, itemList, false);
        GenerateRow(slots_layerBack, itemList, true);
    }

    private void InitializeFruitPrefabs()
    {
        fruitPrefabDict = new Dictionary<FruitType, GameObject>();
        foreach (GameObject prefab in fruitPrefabs)
        {
            if (prefab != null)
            {
                DraggableItem draggableItem = prefab.GetComponent<DraggableItem>();
                if (draggableItem != null)
                {
                    fruitPrefabDict[draggableItem.fruitType] = prefab;
                }
                else
                {
                    Debug.LogWarning($"Prefab {prefab.name} does not have a DraggableItem component!");
                }
            }
            else
            {
                Debug.LogWarning("Null prefab found in fruitPrefabs array!");
            }
        }
    }

    private void GenerateRow(Transform[] layer, List<InfoItemsList> itemList, bool isShadow)
    {
        List<Transform> availableSlots = new List<Transform>(layer);
        Dictionary<Transform, int> shelfFruitCount = new Dictionary<Transform, int>();

        foreach (Transform slot in layer)
        {
            Transform shelf = slot.parent;
            if (!shelfFruitCount.ContainsKey(shelf))
            {
                shelfFruitCount[shelf] = 0;
            }
        }

        foreach (InfoItemsList item in itemList)
        {
            for (int j = 0; j < item.countItem; j++)
            {
                if (availableSlots.Count == 0)
                {
                    Debug.LogWarning("Not enough slots available for all items!");
                    return;
                }

                Transform selectedSlot = GetAvailableSlot(availableSlots, shelfFruitCount);

                if (selectedSlot != null && fruitPrefabDict.TryGetValue(item.itemType, out GameObject fruitPrefab))
                {
                    GameObject newItem = Instantiate(fruitPrefab, new Vector3(selectedSlot.position.x, selectedSlot.position.y, -1f), Quaternion.identity);
                    SetupNewItem(newItem, selectedSlot, isShadow);

                    // Явно устанавливаем состояние занятости слота
                    Slot slotComponent = selectedSlot.GetComponent<Slot>();
                    if (slotComponent != null)
                    {
                        slotComponent.OccupySlot(newItem.GetComponent<DraggableItem>());
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to instantiate fruit of type {item.itemType}. Prefab not found or no slot available.");
                }
            }
        }
    }
    private Transform GetAvailableSlot(List<Transform> availableSlots, Dictionary<Transform, int> shelfFruitCount)
    {
        for (int attempt = 0; attempt < availableSlots.Count; attempt++)
        {
            int randomIndex = Random.Range(0, availableSlots.Count);
            Transform selectedSlot = availableSlots[randomIndex];
            Transform selectedShelf = selectedSlot.parent;

            if (shelfFruitCount[selectedShelf] < 3)
            {
                availableSlots.RemoveAt(randomIndex);
                shelfFruitCount[selectedShelf]++;
                return selectedSlot;
            }
        }

        return null;
    }

    private void SetupNewItem(GameObject newItem, Transform selectedSlot, bool isShadow)
    {
        if (isShadow)
        {
            SpriteRenderer spriteRenderer = newItem.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.2f, 0.2f, 0.2f, 0.4f);
                spriteRenderer.sortingOrder--;
            }

            DraggableItem draggableItem = newItem.GetComponent<DraggableItem>();
            if (draggableItem != null)
            {
                draggableItem.enabled = false;
            }

            BoxCollider2D boxCollider = newItem.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }
            
        }

        else
            {
                 SpriteRenderer spriteRenderer = newItem.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder ++;
            }
            }
        
        newItem.transform.SetParent(selectedSlot);
        newItem.transform.position = selectedSlot.GetChild(0).transform.position;
    }
}