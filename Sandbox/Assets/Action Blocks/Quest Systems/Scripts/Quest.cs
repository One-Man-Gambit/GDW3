using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour
{
    [Header("Quest Attributes")]
    public string questName;
    public string questDescription;
    public string questObjective;
    [SerializeField]
    private bool isComplete;
    public List<QuestItem> questItems;


    public string GetQuestName()
    {
        return questName;
    }

    public string GetQuestDescription()
    {
        return questDescription;
    }

    public string GetQuestObjective()
    {
        return questObjective;
    }

    public void SetQuestCondition(bool questCondition)
    {
        isComplete = questCondition;
    }

}