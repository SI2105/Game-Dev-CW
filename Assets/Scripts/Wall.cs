using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
     public GameObject fireEffect1;
     public GameObject fireEffect2;
     public GameObject light1;
     public GameObject light2;
     public GameObject flag1;
     public GameObject flag2;
     public GameObject torch1;
     public GameObject torch2;
     public GameObject pot1;
     public GameObject pot2;
     public GameObject wallPrefab;


    private void Start()
    {
       
        TimeManager.Instance.OnTimeOfDayChange += toggleFireEffect;
        
    }

    // Toggle visibility of the wall and associated objects
     public void toggleWall(bool isFlagActive){
         
       
        // If the flag is active, deactivate it and activate everything else
        if (flag1.activeSelf)
        {
            flag1.SetActive(false);
            flag2.SetActive(false);
            torch1.SetActive(true);
            torch2.SetActive(true);
            pot1.SetActive(true);
            pot2.SetActive(true);
        }
        else
        {
            // If the flag is not active, activate it and deactivate everything else
            flag1.SetActive(true);
            flag2.SetActive(true);
            torch1.SetActive(false);
            torch2.SetActive(false);
            pot1.SetActive(false);
            pot2.SetActive(false);
        }
     }

     // Event handler for OnTimeOfDayChange
    private void toggleFireEffect()
    {
        if(TimeManager.Instance.currentTimeOfDayEnum == DayNightSystem.TimeOfDay.Night){
            fireEffect1.SetActive(true);
            fireEffect2.SetActive(true);
            light1.SetActive(true);
            light2.SetActive(true);
        }
        else{
            fireEffect1.SetActive(false);
            fireEffect2.SetActive(false);
            light1.SetActive(false);
            light2.SetActive(false);
        }
    }




}
