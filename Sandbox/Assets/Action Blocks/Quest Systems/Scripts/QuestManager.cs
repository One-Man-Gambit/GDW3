using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager questManager;
    private void Awake()
    {
        if(questManager != null && questManager != this)
        {
            Destroy(this);
        }
        else
        {
            questManager = this;
        }
    }

    [Header("UI Components")]
    public GameObject questPanel;
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;
    public TextMeshProUGUI questObjective;

    [Header("Quest Components")]
    [SerializeField]
    public Quest activeQuest;
    private List<Quest> quests; //not currently in use

    private void Start() 
    {   
        // Turn off quest panel when we dont have a quest
        if (activeQuest == null) {
            questPanel.SetActive(false);
        }
    }
    public void ActivateQuest(Quest _quest)
    {
        if(activeQuest == null)
        {
            activeQuest = _quest;
            questTitle.text = _quest.GetQuestName();
            questDescription.text = _quest.GetQuestDescription();
            questObjective.text = _quest.GetQuestObjective();
            for(int i = 0; i < activeQuest.questItems.Count; i++)
            {
                Instantiate(activeQuest.questItems[i], activeQuest.questItemsPositions[i].position, Quaternion.identity);
            }

            // Turn on quest panel when we have a quest
            questPanel.SetActive(true);
        }        
    }

    public void QuestComplete()
    {
        activeQuest.SetQuestCondition(true);
        activeQuest = null;
        questTitle.text = "Quest Title: -";
        questDescription.text = "Quest Description: -";
        questObjective.text = "Quest Objective: -";

        GameManager.GetInstance().playerRef.AddMoney(50);

        // Turn off Quest Panel when no quest is left
        questPanel.SetActive(false);
    }

    public void QuestItemCollected(QuestItem item)
    {
        if(activeQuest.questItems.Count > 0)
        {
            activeQuest.questItems.RemoveAt(activeQuest.questItems.Count-1);
        }
        if(activeQuest.questItems.Count == 0)
        {
            activeQuest.allItemsCollected = true;
        }        
    }

}
