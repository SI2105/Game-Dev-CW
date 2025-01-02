using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SlimUI.ModernMenu;

public class GameAudioManager : MonoBehaviour
{
    public AudioSource musicAudioSource;

    private void Start()
    {
        if (SettingsManager.Instance != null)
        {
            // Apply the saved volume to the AudioSource in the game scene
            SettingsManager.Instance.ApplyMusicVolume(musicAudioSource);
        }
        else
        {
            Debug.LogWarning("SettingsManager is missing or not initialized.");
        }
    }
}