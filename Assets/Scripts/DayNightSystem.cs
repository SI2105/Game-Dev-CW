using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    public enum TimeOfDay
    {
        Sunrise,
        Day,
        Sunset,
        Night
    }

    public Light directionalLight;
    public List<SkyboxMapping> timeMapping;

    public SkyboxTransitionManager skyboxTransitionManager;

    private float blended_value = 0.0f;

    public WeatherSystem weatherSystem;

    [SerializeField]
    private TimeOfDay _currentTimeOfDay;

    private TimeOfDay previousTimeOfDay; // Keep track of the previous time of day

    public TimeOfDay currentTimeOfDay
    {
        get => _currentTimeOfDay;
        set => _currentTimeOfDay = value;
    }

    public int currentHour;

    void Start()
    {
        // Initialize the previousTimeOfDay to Night
        previousTimeOfDay = TimeOfDay.Night;
        currentTimeOfDay = TimeOfDay.Night;

        // Set the initial skybox material to the night material
        SetInitialNightSkybox();
    }

    // Update is called once per frame
    void Update()
    {
        // Get the current time of day and hour from TimeManager
        float currentTimeOfDay = TimeManager.Instance.CurrentTimeOfDay;
        currentHour = TimeManager.Instance.CurrentHour;

        // Update the TimeOfDay enum based on the hour
        UpdateTimeOfDay(currentHour);

        // Change the direction of the light based on the current time of day
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3((currentTimeOfDay / TimeManager.Instance.DayDurationInSeconds * 360) - 90, 170, 0));

        if (!weatherSystem.isSpecialWeather)
        {
            // Check if the time of day has changed
            if (previousTimeOfDay != this.currentTimeOfDay)
            {
                // Trigger a skybox material change
                ChangeSkyMaterial(currentHour);

                // Update the previousTimeOfDay to the current value
                previousTimeOfDay = this.currentTimeOfDay;
            }
        }
    }

    private void UpdateTimeOfDay(int currentHour)
    {
        // Determine the current time of day based on the hour
        if (currentHour >= 6 && currentHour < 11)
        {
            currentTimeOfDay = TimeOfDay.Sunrise;
        }
        else if (currentHour >= 11 && currentHour < 18)
        {
            currentTimeOfDay = TimeOfDay.Day;
        }
        else if (currentHour >= 18 && currentHour < 22)
        {
            currentTimeOfDay = TimeOfDay.Sunset;
        }
        else
        {
            currentTimeOfDay = TimeOfDay.Night;
        }
    }

    private void ChangeSkyMaterial(int currentHour)
    {
        // Retrieve the correct material for the current hour
        Material newSkybox = null;

        foreach (SkyboxMapping mapping in timeMapping)
        {
            // If the current hour matches the mapping hour, set the new skybox material
            if (currentHour == mapping.hour)
            {
                newSkybox = mapping.skyboxMaterial;
                break;
            }
        }

        if (newSkybox != null)
        {
            // Get the current skybox material
            Material currentSkybox = RenderSettings.skybox;

            // Use the SkyboxTransitionManager for the transition
            if (skyboxTransitionManager != null)
            {
                skyboxTransitionManager.StartTransition(currentSkybox, newSkybox, 5f); // Example duration: 5 seconds
            }
            else
            {
                Debug.LogError("SkyboxTransitionManager is not assigned in the Inspector!");
            }
        }
        else
        {
            Debug.LogWarning($"No skybox material found for hour {currentHour}");
        }
    }

    private void SetInitialNightSkybox()
    {
        Material nightSkybox = null;

        foreach (SkyboxMapping mapping in timeMapping)
        {
            if (mapping.phaseName == "Night")
            {
                nightSkybox = mapping.skyboxMaterial;
                break;
            }
        }

        if (nightSkybox != null)
        {
            RenderSettings.skybox = nightSkybox;
        }
        else
        {
            Debug.LogError("No skybox material found for the 'Night' phase!");
        }
    }
}


[System.Serializable]
public class SkyboxMapping
{
    public string phaseName;
    public int hour;
    public Material skyboxMaterial;
}
