using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{


    [Range(0, 1)] public float probabilityOfRainSpring = 0.3f;
    [Range(0, 1)] public float probabilityOfRainSummer = 0.1f;
    [Range(0, 1)] public float probabilityOfRainFall = 0.6f;
    [Range(0, 1)] public float probabilityOfRainWinter = 0.5f;

    [Range(0, 1)] public float probabilityOfSnowSpring = 0.1f;
    [Range(0, 1)] public float probabilityOfSnowSummer = 0.0f;
    [Range(0, 1)] public float probabilityOfSnowFall = 0.5f;
    [Range(0, 1)] public float probabilityOfSnowWinter = 0.8f;

    [Range(0, 1)] public float probabilityOfLightningSpring = 0.1f;
    [Range(0, 1)] public float probabilityOfLightningSummer = 0.2f;
    [Range(0, 1)] public float probabilityOfLightningFall = 0.4f;
    [Range(0, 1)] public float probabilityOfLightningWinter = 0.3f;

    [Range(0, 1)] public float probabilityOfSandstormSpring = 0.0f;
    [Range(0, 1)] public float probabilityOfSandstormSummer = 0.7f;
    [Range(0, 1)] public float probabilityOfSandstormFall = 0.1f;
    [Range(0, 1)] public float probabilityOfSandstormWinter = 0.0f;

    [Range(0, 1)] public float probabilityOfFogSpring = 0.2f;
    [Range(0, 1)] public float probabilityOfFogSummer = 0.1f;
    [Range(0, 1)] public float probabilityOfFogFall = 0.5f;
    [Range(0, 1)] public float probabilityOfFogWinter = 0.6f;

    public List<WeatherSystemMapping> weatherMappings;

    private GameObject currentWeatherEffect=null;

    public bool isSpecialWeather = false;

    public enum WeatherCondition
    {
        Sunny,
        Rainy,
        Snowy,
        Lightning,
        Sandstorm,
        Foggy
    }

    public WeatherCondition currentCondition = WeatherCondition.Sunny;

    void Start()
    {
        //subscribe to the event
        TimeManager.Instance.OnDayPass += GenerateRandomWeather;
    }

    private void GenerateRandomWeather()
    {
        TimeManager.SeasonOfYear currentSeasonOfYear = TimeManager.Instance.currentSeasonOfYear;

        // Map season to probabilities
        Dictionary<WeatherCondition, float> probabilities = GetSeasonProbabilities(currentSeasonOfYear);

        // Randomly select weather based on probabilities
        currentCondition = SelectWeatherCondition(probabilities);

        if (currentCondition != WeatherCondition.Sunny){

            isSpecialWeather=true;
            WeatherSystemMapping selectedMapping = null;

            switch (currentCondition)
            {
                case WeatherCondition.Sunny:
                    selectedMapping = weatherMappings.Find(mapping => mapping.condition == WeatherCondition.Sunny);
                    break;
                case WeatherCondition.Rainy:
                    selectedMapping = weatherMappings.Find(mapping => mapping.condition == WeatherCondition.Rainy);
                    break;
                case WeatherCondition.Snowy:
                    selectedMapping = weatherMappings.Find(mapping => mapping.condition == WeatherCondition.Snowy);
                    break;
                case WeatherCondition.Lightning:
                    selectedMapping = weatherMappings.Find(mapping => mapping.condition == WeatherCondition.Lightning);
                    break;
                case WeatherCondition.Sandstorm:
                    selectedMapping = weatherMappings.Find(mapping => mapping.condition == WeatherCondition.Sandstorm);
                    break;
                case WeatherCondition.Foggy:
                    selectedMapping = weatherMappings.Find(mapping => mapping.condition == WeatherCondition.Foggy);
                    break;
                default:
                    Debug.LogWarning("Weather condition not recognized!");
                    break;
            }

            if (selectedMapping != null)
            {

            
                // Start the coroutine to delay the effect start
                StartCoroutine(startEffect(selectedMapping));
            }

        }

        else{
            isSpecialWeather = false;
            currentWeatherEffect.SetActive(false);
        }

    
        Debug.Log($"Weather for {currentSeasonOfYear}: {currentCondition}");
    }


   private IEnumerator startEffect(WeatherSystemMapping selectedMapping)
    {
        // Wait for 1 second before proceeding
        yield return new WaitForSeconds(1f);

        if (currentWeatherEffect != null)
        {
            // Stop the current weather effect
            currentWeatherEffect.SetActive(false);
        }

        // Apply the selected mapping (e.g., activate effects and set skybox)
        RenderSettings.skybox = selectedMapping.skyboxMaterial;

        // If the selected weather effect is not null, stop, clear and play it
        if (selectedMapping.weatherEffect != null)
        {
            
            // Play the new weather effect
            selectedMapping.weatherEffect.SetActive(true);

            // Optionally, you can assign this new effect to currentWeatherEffect if you need to manage it later
            currentWeatherEffect = selectedMapping.weatherEffect;

            Debug.Log("Selected weather effect is played");
        }
        else
        {
            Debug.Log("Selected weather effect is null!");
        }
    }




    private Dictionary<WeatherCondition, float> GetSeasonProbabilities(TimeManager.SeasonOfYear season)
    {
        switch (season)
        {
            case TimeManager.SeasonOfYear.Spring:
                return new Dictionary<WeatherCondition, float>
                {
                    { WeatherCondition.Sunny, 0.4f },
                    { WeatherCondition.Rainy, probabilityOfRainSpring },
                    { WeatherCondition.Snowy, probabilityOfSnowSpring },
                    { WeatherCondition.Lightning, probabilityOfLightningSpring },
                    { WeatherCondition.Sandstorm, probabilityOfSandstormSpring },
                    { WeatherCondition.Foggy, probabilityOfFogSpring }
                };
            case TimeManager.SeasonOfYear.Summer:
                return new Dictionary<WeatherCondition, float>
                {
                    { WeatherCondition.Sunny, 0.5f },
                    { WeatherCondition.Rainy, probabilityOfRainSummer },
                    { WeatherCondition.Snowy, probabilityOfSnowSummer },
                    { WeatherCondition.Lightning, probabilityOfLightningSummer },
                    { WeatherCondition.Sandstorm, probabilityOfSandstormSummer },
                    { WeatherCondition.Foggy, probabilityOfFogSummer }
                };
            case TimeManager.SeasonOfYear.Fall:
                return new Dictionary<WeatherCondition, float>
                {
                    { WeatherCondition.Sunny, 0.2f },
                    { WeatherCondition.Rainy, probabilityOfRainFall },
                    { WeatherCondition.Snowy, probabilityOfSnowFall },
                    { WeatherCondition.Lightning, probabilityOfLightningFall },
                    { WeatherCondition.Sandstorm, probabilityOfSandstormFall },
                    { WeatherCondition.Foggy, probabilityOfFogFall }
                };
            case TimeManager.SeasonOfYear.Winter:
                return new Dictionary<WeatherCondition, float>
                {
                    { WeatherCondition.Sunny, 0.1f },
                    { WeatherCondition.Rainy, probabilityOfRainWinter },
                    { WeatherCondition.Snowy, probabilityOfSnowWinter },
                    { WeatherCondition.Lightning, probabilityOfLightningWinter },
                    { WeatherCondition.Sandstorm, probabilityOfSandstormWinter },
                    { WeatherCondition.Foggy, probabilityOfFogWinter }
                };
            default:
                return new Dictionary<WeatherCondition, float>();
        }
    }

    private WeatherCondition SelectWeatherCondition(Dictionary<WeatherCondition, float> probabilities)
    {
        float total = 0f;
        foreach (var prob in probabilities.Values)
        {
            total += prob;
        }

        float randomPoint = Random.value * total;

        foreach (var pair in probabilities)
        {
            if (randomPoint < pair.Value)
            {
                return pair.Key;
            }
            randomPoint -= pair.Value;
        }

        return WeatherCondition.Sunny; // Default fallback
    }
}


[System.Serializable]
// Mapping class to map hour of day to skybox material
public class WeatherSystemMapping
{
    public WeatherSystem.WeatherCondition condition;
    public GameObject weatherEffect;
    public Material skyboxMaterial;
}
