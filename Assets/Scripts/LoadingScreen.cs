using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel; // The loading screen panel
    [SerializeField] private TextMeshProUGUI countdownText; // Text to display countdown time
    [SerializeField] private Slider progressBar; // Progress bar to indicate time left
    private float totalTime = 40f; // Total time for the loading screen

    private GameObject playerUI; // Reference to the PlayerUI GameObject

    void Start()
    {
        // Find the Player GameObject by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Find the PlayerUI GameObject inside the Player
            playerUI = player.transform.Find("PlayerUI")?.gameObject;

            if (playerUI != null)
            {
                // Deactivate the PlayerUI GameObject
                playerUI.SetActive(false);
            }
            else
            {
                Debug.LogWarning("PlayerUI GameObject not found inside Player!");
            }
        }
        else
        {
            Debug.LogWarning("Player GameObject not found! Ensure it is tagged as 'Player'.");
        }

        // Initialize the loading screen
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (progressBar != null)
            progressBar.maxValue = totalTime; // Set the maximum value of the progress bar

        // Start the countdown
        StartCoroutine(LoadingCountdown());
    }

    private IEnumerator LoadingCountdown()
    {
        float timeLeft = totalTime;

        while (timeLeft > 0)
        {
            // Update countdown text
            if (countdownText != null)
                countdownText.text = $"Time Left: {Mathf.CeilToInt(timeLeft)}s";

            // Update progress bar
            if (progressBar != null)
                progressBar.value = totalTime - timeLeft;

            // Wait for the next frame
            yield return null;

            // Decrease the remaining time
            timeLeft -= Time.deltaTime;
        }

        // Ensure progress bar and text are updated at the end
        if (countdownText != null)
            countdownText.text = "Time Left: 0s";

        if (progressBar != null)
            progressBar.value = totalTime;

        // Hide the loading screen after the countdown
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        // Reactivate the PlayerUI GameObject
        if (playerUI != null)
        {
            playerUI.SetActive(true);
        }

        // Trigger post-loading logic
        OnLoadingComplete();
    }

    private void OnLoadingComplete()
    {
        // Add any post-loading logic here
        Debug.Log("Loading complete!");
    }
}
