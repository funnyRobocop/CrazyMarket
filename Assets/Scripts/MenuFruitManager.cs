using UnityEngine;

public class MenuFruitManager : MonoBehaviour
{
    [SerializeField] private GameObject[] fruitImages;

    private void Start()
    {
        ActivateRandomFruit();
    }

    private void ActivateRandomFruit()
    {
        // Деактивируем все фрукты
        foreach (GameObject fruit in fruitImages)
        {
            fruit.SetActive(false);
        }

        // Активируем случайный фрукт
        if (fruitImages.Length > 0)
        {
            int randomIndex = Random.Range(0, fruitImages.Length);
            fruitImages[randomIndex].SetActive(true);
        }
        else
        {
            Debug.LogWarning("Массив fruitImages пуст. Добавьте изображения фруктов в инспекторе.");
        }
    }
}