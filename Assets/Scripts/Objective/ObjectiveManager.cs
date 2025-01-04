using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [SerializeField] private List<ObjectiveData> objectiveList = new List<ObjectiveData>();
    Dictionary<string, ObjectiveData> Objective_Dict = new Dictionary<string, ObjectiveData>();
    private Transform ObjectivePanel;
    private GameObject ObjectiveUI;
    [SerializeField] private GameObject ObjectivePrefab;
    private bool IsObjectivePanelActive;
    private GameObject ObjectivePopup;
    private GameObject MiniObjectivePanel;
    [SerializeField] private GameObject MiniObjectivePrefab;
    private List<GameObject> miniObjectiveItems = new List<GameObject>();
    private void Awake()
    {
        //Ensures Singleton Pattern
        if (Instance == null) {
            Instance = this;
            ObjectivePopup = GameObject.Find("ObjectivePopup");
            IsObjectivePanelActive = false;
            ObjectivePanel = GameObject.Find("ObjectivePanel").transform;
            ObjectiveUI = GameObject.Find("ObjectiveUI");
            MiniObjectivePanel = GameObject.Find("MiniObjectivePanel");
            ObjectiveUI.SetActive(IsObjectivePanelActive);
            ObjectivePopup.SetActive(false);
            ObjectivePopup.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "";
            DontDestroyOnLoad(gameObject);
            IntializeObjective_Dict();
            UpdateDisplayedObjectives();
            RefreshMiniObjectivePanel();
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
    private int chestOpened = 0;
    private const int chestsToOpen = 3;
    public void ChestOpened() {
        chestOpened++;
        
        if(chestOpened >= chestsToOpen)
        {
            SetEventComplete("Open 3 Chests");
        }
    }
    public void SetEventComplete(string objectiveName) {
        //Sets the event to complete, will be used when the event associated with the objective is completed
        if (Objective_Dict.ContainsKey(objectiveName) && !Objective_Dict[objectiveName].objectiveComplete)
        {
            Debug.Log(objectiveName + " is complete");
            Objective_Dict[objectiveName].objectiveComplete = true;
            UpdateDisplayedObjectives();
            ShowObjectivePopup(objectiveName, "Objective Complete!");
            HandleMiniObjectiveCompletion(objectiveName);


        }
    }

    private bool HandleMiniObjectiveCompletion(string ObjectiveName) {
        foreach (GameObject MiniObjectiveitem in miniObjectiveItems) {
            if (MiniObjectiveitem.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text == ObjectiveName)
            {
                StartCoroutine(FadeIn(MiniObjectiveitem.transform.GetChild(1).gameObject));
                return true;
            }
          
        }

        return false;
    }

    private IEnumerator FadeIn(GameObject target)
    {
        Image image = target.GetComponent<Image>();
        if (image == null)
        {
            yield break;
        }

        float duration = 1.0f; // Duration of the fade-in effect
        float elapsedTime = 0f;
        Color color = image.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / duration);
            image.color = color;
            yield return null;
        }

        color.a = 1f;
        image.color = color;

        yield return new WaitForSeconds(2.0f);

        // Refresh the MiniObjectivePanel
        RefreshMiniObjectivePanel();
    }
    public void RefreshMiniObjectivePanel()
    {
        foreach (Transform child in MiniObjectivePanel.transform)
        {
            miniObjectiveItems.Clear();
            Destroy(child.gameObject);
        }

        // Get the first two uncompleted objectives
        int count = 0;
        foreach (ObjectiveData objective in Objective_Dict.Values)
        {
            if (!objective.objectiveComplete)
            {
                GameObject miniObjectiveItem = Instantiate(MiniObjectivePrefab, MiniObjectivePanel.transform);
                miniObjectiveItems.Add(miniObjectiveItem);
                miniObjectiveItem.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = objective.objectiveName;
                Color color = miniObjectiveItem.transform.GetChild(1).GetComponent<Image>().color;
                color.a = 0;
                miniObjectiveItem.transform.GetChild(1).GetComponent<Image>().color = color;

                count++;
                if (count >= 2)
                {
                    break;
                }
            }
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
            if (!objective.objectiveComplete)
            {
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

    private void ShowObjectivePopup(string objectiveName, string message)
    {
        // Show the popup with the objective details
        ObjectivePopup.SetActive(true);
        ObjectivePopup.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"{objectiveName}: {message}";
        Invoke("HideObjectivePopup", 5);
    }

    private void HideObjectivePopup()
    {
        // Hide the popup
        ObjectivePopup.SetActive(false);
    }
}

