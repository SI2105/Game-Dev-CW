using System;

[Serializable]
public class ObjectiveData
{
    //Designed as a data structure to hold objective data
    public string objectiveName;
    public string objectiveDescription;
    public bool objectiveComplete; //Used to see if the Objective has been completed or not

    public ObjectiveData(string objectiveName, string objectiveDescription)
    {
        this.objectiveName = objectiveName;
        this.objectiveDescription = objectiveDescription;
        this.objectiveComplete = false;
    }
}
