using UnityEngine;
using System;

public class Slot : MonoBehaviour
{
    [SerializeField] private Transform itemSpawnPoint;
    public bool IsOccupied => transform.childCount >= 2;
    
    public event Action OnSlotOccupied;
    public event Action OnSlotCleared;

    public bool TryOccupySlot(DraggableItem item)
    {
        if (!IsOccupied)
        {
            OccupySlot(item);
            return true;
        }
        return false;
    }

    public void OccupySlot(DraggableItem item)
    {
        if (!IsOccupied)
        {
            item.transform.SetParent(transform);
            item.transform.position = itemSpawnPoint.position;
            OnSlotOccupied?.Invoke();
        }
        else
        {
            Debug.LogWarning("Attempting to occupy a slot that already has 2 children!");
        }
    }

    public void ClearSlot()
    {
        OnSlotCleared?.Invoke();
    }

    public FruitType? GetFruitType()
    {
        if (transform.childCount > 1)
        {
            DraggableItem item = transform.GetChild(1).GetComponent<DraggableItem>();
            if (item != null)
            {
                return item.fruitType;
            }
            else
            {
                Debug.LogWarning($"Child object in slot {name} does not have a DraggableItem component.");
            }
        }
        return null;
    }
}