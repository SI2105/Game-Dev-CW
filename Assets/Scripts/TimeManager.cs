using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TimeManager class to keep track of time of day, day of week, and such to be used by Other classes for features dependent on time
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; set; }

    public enum SeasonOfYear
    {
        Spring,
        Summer,
        Fall,
        Winter
    }

    public SeasonOfYear currentSeasonOfYear = SeasonOfYear.Spring;

    public event Action OnTimeOfDayChange;

    public int numberOfDaysPerSeasonOfYear = 30;

    public int currentSeasonOfYearDay = 1;

    private DayNightSystem.TimeOfDay previousTimeOfDay; // To track when the TimeOfDay changes

    public DayNightSystem.TimeOfDay currentTimeOfDayEnum;

    public int currentHour; // To track the current hour

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

    // Getter and Setter for currentTimeOfDay
    public float CurrentTimeOfDay
    {
        get { return currentTimeOfDay; }
        set { currentTimeOfDay = Mathf.Clamp(value, 0.0f, dayDurationInSeconds); } // Ensure time is within the day's duration
    }

    // Getter for currentHour
    public int CurrentHour
    {
        get { return currentHour; }
    }

    // Method to transition to the next day
    public void TransitionNextDay()
    {
        Day += 1;
        currentSeasonOfYearDay += 1;

        // Transition to the next season of year
        if (currentSeasonOfYearDay > numberOfDaysPerSeasonOfYear)
        {
            int seasonIndex = (int)currentSeasonOfYear;
            int nextSeasonIndex = (seasonIndex + 1) % 4;
            currentSeasonOfYear = (SeasonOfYear)nextSeasonIndex;
            currentSeasonOfYearDay = 0;
        }

        CurrentTimeOfDay = 0.0f;
        currentHour = 0; // Reset the current hour
        previousTimeOfDay = DayNightSystem.TimeOfDay.Night; // Reset to the default TimeOfDay

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

        currentHour = Mathf.FloorToInt((CurrentTimeOfDay / dayDurationInSeconds) * 24.0f);
    
        currentTimeOfDayEnum = CalculateTimeOfDay(currentHour);

        // Check if the TimeOfDay has changed
        if (currentTimeOfDayEnum != previousTimeOfDay)
        {
            previousTimeOfDay = currentTimeOfDayEnum; // Update the previous TimeOfDay
            OnTimeOfDayChange?.Invoke(); // Invoke the event
        }
    }

    // Method to calculate the current TimeOfDay based on the hour
    private DayNightSystem.TimeOfDay CalculateTimeOfDay(int currentHour)
    {
        if (currentHour >= 7 && currentHour <= 11)
        {
            return DayNightSystem.TimeOfDay.Sunrise;
        }
        else if (currentHour > 11 && currentHour <= 18)
        {
            return DayNightSystem.TimeOfDay.Day;
        }
        else if (currentHour > 18 && currentHour <= 22)
        {
            return DayNightSystem.TimeOfDay.Sunset;
        }
        else
        {
            return DayNightSystem.TimeOfDay.Night;
        }
    }

    private void Update()
    {
        UpdateTime(Time.deltaTime); // Ensure time progresses
    }
}
