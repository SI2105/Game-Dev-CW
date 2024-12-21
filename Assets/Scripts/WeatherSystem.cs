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

    public DayNightSystem dayNightSystem;

    private GameObject currentWeatherEffect=null;

    public bool isSpecialWeather = false;

    private Dictionary<ParticleSystem, float> originalEmissionRates = new Dictionary<ParticleSystem, float>();


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
    private int remainingWeatherTimeOfDayChanges = 0; // Tracks how many TimeOfDay changes are left for the current weather
    private int bufferTimeOfDayChanges = 0; // Tracks how many TimeOfDay changes are left for the buffer period


    void Start()
    {
        // Subscribe to the event
        TimeManager.Instance.OnTimeOfDayChange += HandleTimeOfDayChange;

        // Start with an initial buffer period using the existing StartBufferPeriod method
        StartBufferPeriod();
    }


    private void StartBufferPeriod()
    {
        // Set a random buffer duration between 5 to 8 TimeOfDay changes
        bufferTimeOfDayChanges = Random.Range(2, 4);

        // Gradually fade out the current weather effect
        if (currentWeatherEffect != null)
        {
            StartCoroutine(FadeOutWeatherEffect(currentWeatherEffect));
        }

        Debug.Log($"Buffer period started for {bufferTimeOfDayChanges} TimeOfDay changes");
    }

   public IEnumerator FadeOutWeatherEffect(GameObject weatherEffect)
    {
        // Get the particle system
        var particleSystem = weatherEffect.GetComponentInChildren<ParticleSystem>();

        // Store the original alpha value
        var main = particleSystem.main;
        float originalAlpha = main.startColor.color.a;
      

        // Set initial alpha to 30 (normalized)
        float initialAlpha = 30f / 255f; // Convert to normalized range
        float fadeDuration = 20f;

        // Set alpha to initial value
        var startColor = main.startColor;
        Color color = startColor.color;
        color.a = initialAlpha;
        startColor.color = color;
        main.startColor = startColor;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Decrease alpha over time
            float alphaDecrementPerSecond = initialAlpha / fadeDuration;
            float newAlpha = Mathf.Max(initialAlpha - (alphaDecrementPerSecond * elapsedTime), 0f);

            // Adjust alpha transparency
            color.a = newAlpha;
            startColor.color = color;
            main.startColor = startColor;

            Debug.LogError($"Alpha: {newAlpha * 255f}"); // Log in 0-255 range

            yield return null; // Wait for the next frame
        }

        // Deactivate the weather effect
        weatherEffect.SetActive(false);
        

        // Restore original alpha (if needed)
        var resetColor = main.startColor;
        Color resetColorValue = resetColor.color;
        resetColorValue.a = originalAlpha;
        resetColor.color = resetColorValue;
        main.startColor = resetColor;
    }


     private void HandleTimeOfDayChange()
    {
        if (bufferTimeOfDayChanges > 0)
        {
            bufferTimeOfDayChanges--;
            if (bufferTimeOfDayChanges <= 0)
            {
                GenerateRandomWeather();
            }
            return; // Wait until the buffer period ends
        }

        if (remainingWeatherTimeOfDayChanges > 0)
        {
            remainingWeatherTimeOfDayChanges--;

            if (remainingWeatherTimeOfDayChanges <= 0)
            {
                StartBufferPeriod();
            }
        }

        if (currentWeatherEffect != null){
            // Update skybox material if there's an active weather effect
            UpdateSkyboxMaterial();
        }
    }

     private void UpdateSkyboxMaterial()
    {
        

        var selectedMapping = weatherMappings.Find(mapping => mapping.condition == currentCondition);
       

        DayNightSystem.TimeOfDay currentTimeOfDay = TimeManager.Instance.currentTimeOfDayEnum;
        foreach (var timeOfDayMaterial in selectedMapping.skyboxMaterialList)
        {
            if (timeOfDayMaterial.timeOfDay == currentTimeOfDay)
            {
                RenderSettings.skybox = timeOfDayMaterial.material;
                Debug.LogError(currentCondition);
                Debug.LogError(currentTimeOfDay);
                return;
            }
        }
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
            remainingWeatherTimeOfDayChanges = Random.Range(4, 7); // Random duration in hours (4-6 inclusive)

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
            // Sunny weather doesn't require special handling
            isSpecialWeather = false;
            remainingWeatherTimeOfDayChanges = Random.Range(4, 7); // Random duration in hours (4-6 inclusive)

            if (currentWeatherEffect != null)
            {
                currentWeatherEffect.SetActive(false);
                currentWeatherEffect=null;
            }
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

        // Get the current TimeOfDay from the assigned DayNightSystem
        DayNightSystem.TimeOfDay currentTimeOfDay = dayNightSystem.currentTimeOfDay;

        // Apply the selected mapping (e.g., activate effects and set skybox)
        Material skyboxMaterial = null;

        // Search for the matching TimeOfDayMaterial in the skyboxMaterialList
        foreach (var timeOfDayMaterial in selectedMapping.skyboxMaterialList)
        {
            if (timeOfDayMaterial.timeOfDay == currentTimeOfDay)
            {
                skyboxMaterial = timeOfDayMaterial.material;
                break;
            }
        }
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
        }
        else
        {
            Debug.LogWarning($"No skybox material found for TimeOfDay: {currentTimeOfDay}");
        }

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
// Mapping class to map weather condition and time of day to skybox material
public class WeatherSystemMapping
{
    public WeatherSystem.WeatherCondition condition; // The weather condition (e.g., Rain, Sunny)
    public GameObject weatherEffect; // The visual effect for this condition
    public List<TimeOfDayMaterial> skyboxMaterialList; // List mapping TimeOfDay to skybox material
}

[System.Serializable]
// Helper class to serialize key-value pairs
public class TimeOfDayMaterial
{
    public DayNightSystem.TimeOfDay timeOfDay; // Key
    public Material material; // Value
}

