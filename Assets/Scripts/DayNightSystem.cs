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

    private float blended_value = 0.0f;

    public WeatherSystem weatherSystem;

     // Backing field for currentTimeOfDay
    [SerializeField]
    private TimeOfDay _currentTimeOfDay;

    // Current TimeOfDay with property access
    public TimeOfDay currentTimeOfDay
    {
        get => _currentTimeOfDay;
        set => _currentTimeOfDay = value;
    }


    public int currentHour;
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


        if(weatherSystem.isSpecialWeather ==false){
            // Change the material of the sky based on the current hour
            ChangeSkyMaterial(currentHour);
        }
    }

    private void UpdateTimeOfDay(int currentHour)
    {
        // Determine the current time of day based on the hour
        if (currentHour >=7 && currentHour <= 11)
        {
            currentTimeOfDay = TimeOfDay.Sunrise;
        }
        else if (currentHour >= 11 && currentHour <= 18)
        {
            currentTimeOfDay = TimeOfDay.Day;
        }
        else if (currentHour >= 18 && currentHour <= 22)
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
        Material currentSkybox = null;

        foreach (SkyboxMapping mapping in timeMapping)
        {
            // If the current hour matches the mapping hour, set the current skybox to the mapping skybox
            if (currentHour == mapping.hour)
            {
                currentSkybox = mapping.skyboxMaterial;

                if (currentSkybox.shader != null)
                {
                    if (currentSkybox.shader.name == "Custom/SkyboxTransition")
                    {
                        blended_value += Time.deltaTime;
                        blended_value = Mathf.Clamp01(blended_value);

                        currentSkybox.SetFloat("_TransitionFactor", blended_value);
                    }
                    else
                    {
                        blended_value = 0.0f;
                    }
                }

                break;
            }
        }

        if (currentSkybox != null)
        {
            // Set the skybox material to the correct material
            RenderSettings.skybox = currentSkybox;
        }
    }
}


[System.Serializable]
// Mapping class to map hour of day to skybox material
public class SkyboxMapping
{
    public string phaseName;
    public int hour;
    public Material skyboxMaterial;
}
