using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [SerializeField] private List<ObjectiveData> objectiveList = new List<ObjectiveData>();
    Dictionary<string, ObjectiveData> Objective_Dict = new Dictionary<string, ObjectiveData>();
    private Transform ObjectivePanel;
    private GameObject ObjectiveUI;
    [SerializeField] private GameObject ObjectivePrefab;
    private bool IsObjectivePanelActive;
    private void Awake()
    {
        //Ensures Singleton Pattern
        if (Instance == null) {
            Instance = this;
            IsObjectivePanelActive = false;
            ObjectivePanel = GameObject.Find("ObjectivePanel").transform;
            ObjectiveUI = GameObject.Find("ObjectiveUI");
            ObjectiveUI.SetActive(IsObjectivePanelActive);
            DontDestroyOnLoad(gameObject);
            IntializeObjective_Dict();
            UpdateDisplayedObjectives();
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void IntializeObjective_Dict() {
        //Populates the Objective_Dict with the objectives from the objectiveList
        foreach (ObjectiveData objective in objectiveList)
        {
            if (!Objective_Dict.ContainsKey(objective.objectiveName))
                Objective_Dict[objective.objectiveName] = objective; //Making the Key for the dictionary the ObjectiveName facilitates easy access
            UpdateDisplayedObjectives();
        }
    }
    public void AddEvent(string eventName, string eventDescription)
    {
        //Adds a new event to the Objective_Dict
        if (!Objective_Dict.ContainsKey(eventName))
        {
            Objective_Dict[eventName] = new ObjectiveData(eventName, eventDescription);
        }
    }

    public void SetEventComplete(string objectiveName) {
        //Sets the event to complete, will be used when the event associated with the objective is completed
        if (Objective_Dict.ContainsKey(objectiveName))
        {
            Debug.Log(objectiveName + " is complete");
            Objective_Dict[objectiveName].objectiveComplete = true;
            UpdateDisplayedObjectives();

        }
    }

    public bool IsObjectiveComplete(string objectiveName) {
        //Checks if the objective is complete
        return Objective_Dict.ContainsKey(objectiveName) && Objective_Dict[objectiveName].objectiveComplete;
    }

    private void UpdateDisplayedObjectives(){
        //UI facing method, updates the interface with the latest Objective status. Should be called before the Objectives UI is shown to players
        foreach (Transform child in ObjectivePanel)
        {
           Destroy(child.gameObject);   //Destroys existing state of the Objective UI, ready for update
           
        }

        foreach (ObjectiveData objective in Objective_Dict.Values)
        {
            //For each objective, Prefab used to create UI Component
           GameObject item = Instantiate(ObjectivePrefab, ObjectivePanel);
           item.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = objective.objectiveName;
           item.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = objective.objectiveDescription;
            if (!objective.objectiveComplete) {
                item.transform.GetChild(3).gameObject.SetActive(false);
            }
        }
    }

    public void ToggleObjectivePanel()
    {
        //Simple Toggle for Objective Panel, called by InputActions method
        
        IsObjectivePanelActive = !IsObjectivePanelActive;
        ObjectiveUI.SetActive(IsObjectivePanelActive);
    }
}

