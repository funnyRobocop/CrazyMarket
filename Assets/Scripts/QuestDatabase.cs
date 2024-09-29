using UnityEngine;
using System.Collections.Generic;

public enum QuestType
{
    GameEntry,
    FruitMatch
}

[System.Serializable]
public class QuestData
{
    public int id;
    public QuestType type;
    public string description;
    public int reward;
    public Sprite icon;
    public int requiredProgress;
    public string GetRewardText()
    {
        return $"Награда: {reward}";
    }
}

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    public List<QuestData> quests = new List<QuestData>();

    public QuestData GetQuestById(int id)
    {
        return quests.Find(q => q.id == id);
    }
}