using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Level_", menuName = "New Level Info", order = 51)]
public class LevelInfo_SO : ScriptableObject
{
    public bool hasBlender;
    public List<FruitType> availableFruits = new List<FruitType>();
    public List<FruitType> availableJuices = new List<FruitType>();
    public int maxTotalItems = 27;
    public int minTotalItems = 9; // Минимальное количество предметов для первого уровня
    public int itemIncrease = 3; // Увеличение количества предметов с каждым уровнем

    public List<InfoItemsList> GenerateItemList()
    {
        List<InfoItemsList> itemList = new List<InfoItemsList>();
        int currentLevel = Mathf.Max(1, YG.YandexGame.savesData.currentLevel);

        int totalItemsForLevel = Mathf.Min(minTotalItems + (currentLevel - 1) * itemIncrease, maxTotalItems);

        if (hasBlender)
        {
            GenerateBlenderLevel(itemList, totalItemsForLevel);
        }
        else
        {
            GenerateBalancedRandomLevel(itemList, totalItemsForLevel);
        }

        return itemList;
    }

    private void GenerateBlenderLevel(List<InfoItemsList> itemList, int totalItemsForLevel)
    {
        List<FruitType> availablePairs = GetAvailableFruitJuicePairs();
        int totalItems = 0;

        while (totalItems < totalItemsForLevel && availablePairs.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePairs.Count);
            FruitType fruit = availablePairs[randomIndex];
            FruitType juice = GetCorrespondingJuice(fruit);

            int fruitCount = Mathf.Min(2, totalItemsForLevel - totalItems);
            int juiceCount = Mathf.Min(4, totalItemsForLevel - totalItems - fruitCount);

            if (fruitCount > 0)
            {
                itemList.Add(new InfoItemsList { itemType = fruit, countItem = fruitCount });
                totalItems += fruitCount;
            }

            if (juiceCount > 0)
            {
                itemList.Add(new InfoItemsList { itemType = juice, countItem = juiceCount });
                totalItems += juiceCount;
            }

            availablePairs.RemoveAt(randomIndex);
        }
    }

    private List<FruitType> GetAvailableFruitJuicePairs()
    {
        List<FruitType> pairs = new List<FruitType>();
        foreach (FruitType fruit in availableFruits)
        {
            if (availableJuices.Contains(GetCorrespondingJuice(fruit)))
            {
                pairs.Add(fruit);
            }
        }
        return pairs;
    }

    private FruitType GetCorrespondingJuice(FruitType fruit)
    {
        switch (fruit)
        {
            case FruitType.Apple: return FruitType.Juice_Apple;
            case FruitType.Mango: return FruitType.Juice_Mango;
            case FruitType.Peach: return FruitType.Juice_Peach;
            case FruitType.Strawberry: return FruitType.Juice_Strawberry;
            case FruitType.Kiwi: return FruitType.Juice_Kiwi;
            case FruitType.Papaya: return FruitType.Juice_Papaya;
            case FruitType.Pomegranate: return FruitType.Juice_Pomegranate;
            case FruitType.Carrot: return FruitType.Juice_Carrot;
            case FruitType.Tomato: return FruitType.Juice_Tomato;
            default: return FruitType.Apple;
        }
    }

    private void GenerateBalancedRandomLevel(List<InfoItemsList> itemList, int totalItemsForLevel)
    {
        List<FruitType> allFruits = new List<FruitType>(availableFruits);
        allFruits.AddRange(availableJuices);
        int totalItems = 0;

        while (totalItems < totalItemsForLevel && allFruits.Count > 0)
        {
            int randomIndex = Random.Range(0, allFruits.Count);
            FruitType selectedFruit = allFruits[randomIndex];
            int count = Mathf.Min(3, totalItemsForLevel - totalItems);
            
            itemList.Add(new InfoItemsList { itemType = selectedFruit, countItem = count });
            totalItems += count;
            allFruits.RemoveAt(randomIndex);
        }
    }
}

[Serializable]
public struct InfoItemsList
{
    public FruitType itemType;
    public int countItem;
}