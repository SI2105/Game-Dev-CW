using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TimeManager class to keep track of time of day, day of week, and such to be used by Other classes for features dependent on time
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public int day = 1;
    public float dayDurationInSeconds = 24.0f;
    public int currentHour;
    public float currentTimeOfDay = 0.0f;

    // Getter and Setter for day
    public int Day
    {
        get { return day; }
        set { day = Mathf.Max(1, value); } // Ensure day is at least 1
    }

    // Getter and Setter for dayDurationInSeconds
    public float DayDurationInSeconds
    {
        get { return dayDurationInSeconds; }
        set { dayDurationInSeconds = Mathf.Max(1.0f, value); } // Ensure duration is at least 1 second
    }

    // Getter and Setter for currentHour
    public int CurrentHour
    {
        get { return currentHour; }
        set { currentHour = Mathf.Clamp(value, 0, 23); } // Ensure hour is between 0 and 23
    }

    // Getter and Setter for currentTimeOfDay
    public float CurrentTimeOfDay
    {
        get { return currentTimeOfDay; }
        set { currentTimeOfDay = Mathf.Clamp(value, 0.0f, dayDurationInSeconds); } // Ensure time is within the day's duration
    }

    // Method to transition to the next day
    public void TransitionNextDay()
    {
        Day += 1;
        CurrentTimeOfDay = 0.0f;
        CurrentHour = 0;
        Debug.Log("Transitioned to Day " + Day);
    }

    // Example method to update time (to be called in Update or another method)
    public void UpdateTime(float deltaTime)
    {
        CurrentTimeOfDay += deltaTime;

        if (CurrentTimeOfDay >= dayDurationInSeconds)
        {
            TransitionNextDay();
        }

        CurrentHour = Mathf.FloorToInt((CurrentTimeOfDay / dayDurationInSeconds) * 24.0f);
    }

     private void Update()
        {
            UpdateTime(Time.deltaTime); // Ensure time progresses
        }
}
