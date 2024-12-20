using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    public Light directionalLight;
    public List<SkyboxMapping> timeMapping;

    private float blended_value = 0.0f;

    public WeatherSystem weatherSystem;

    // Update is called once per frame
    void Update()
    {
        // Get the current time of day and hour from TimeManager
        float currentTimeOfDay = TimeManager.Instance.CurrentTimeOfDay;
        int currentHour = TimeManager.Instance.CurrentHour;

        // Change the direction of the light based on the current time of day
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3((currentTimeOfDay / TimeManager.Instance.DayDurationInSeconds * 360) - 90, 170, 0));


        if(weatherSystem.isSpecialWeather ==false){
            // Change the material of the sky based on the current hour
            ChangeSkyMaterial(currentHour);
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
