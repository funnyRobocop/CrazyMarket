using UnityEngine;

public enum FruitType
{
    Apple,
    Carrot,
    Eggplant,
    Kiwi,
    Mango,
    Mangosteen,
    Papaya,
    Peach,
    Pear,
    Pepper,
    Pomegranate,
    Potato,
    Strawberry,
    Tomato,
    Juice_Apple,
    Juice_Tomato,
    Juice_Mango,
    Juice_Carrot,
    Juice_Strawberry,
    Juice_Kiwi,
    Juice_Peach,
    Juice_Papaya,
    Juice_Pomegranate,
    Cookie1,
    Cookie2,
    Cookie3,
    Cookie4,
    Cookie5,
    CookieChel,
    Redka,
    PinkPie,
    CandyBlue,
    CandyHeart,
    CandyOrange,
    CandyPink,
    CandyRed,
    CandySpiral


    
    // Добавьте здесь другие типы фруктов, если они есть в вашей игре
}

public class DraggableItem : MonoBehaviour
{
    public FruitType fruitType;
    private Vector3 originalLocalPosition;
    private Slot currentSlot;
    private Slot originalSlot;


    
    void Start()
    {
        // Сохраняем локальную позицию относительно родителя (слота)
        originalLocalPosition = transform.localPosition;
        // Определяем начальный слот
        originalSlot = GetComponentInParent<Slot>();
        if (originalSlot != null)
        {
           originalSlot.OccupySlot(this);
        }
    }

    

    void OnMouseDown()
    {
        GetComponent<SpriteRenderer>().sortingOrder += 2;
    }

    void OnMouseDrag()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -1f;
        // Отсоединяем объект от родителя на время перетаскивания
        //transform.SetParent(null);
        transform.position = mousePosition;
    }

     void OnMouseUp()
    {
        if (currentSlot != null && currentSlot.TryOccupySlot(this))
        {
            if (originalSlot != null && originalSlot != currentSlot)
            {
                originalSlot.ClearSlot();
            }
            originalSlot = currentSlot;
            originalLocalPosition = transform.localPosition;
        }
        else
        {
            ReturnToOriginalPosition();
        }
        GetComponent<SpriteRenderer>().sortingOrder -= 2;


        currentSlot = null;
    }

    private void ReturnToOriginalPosition()
    {
        transform.SetParent(originalSlot.transform);
        transform.localPosition = originalLocalPosition;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Slot"))
        {
            Slot slot = other.GetComponent<Slot>();
            if (slot != null && !slot.IsOccupied)
            {
                currentSlot = slot;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Slot"))
        {
            Slot slot = other.GetComponent<Slot>();
            if (slot == currentSlot)
            {
                currentSlot = null;
            }
        }
    }
}