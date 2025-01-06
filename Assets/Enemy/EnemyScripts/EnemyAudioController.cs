using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAudioController : MonoBehaviour
{
    // Reference to the AudioSource component
    public AudioSource audioSource;
    public AudioClip roarSound; // The sound clip for this weather condition
    public AudioClip enemyBackgroundClip; // The

    public float fadeDuration = 2.0f; // Duration of the fade-out in seconds

    public void playRoar()
    {
        audioSource.clip = roarSound;
        audioSource.Play();
    }


    public void playBackgroundMusic(){
        audioSource.clip = enemyBackgroundClip;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }

    public void stopSound()
    {
        StartCoroutine(FadeOutSound());
    }

    private IEnumerator FadeOutSound()
    {
        float startVolume = audioSource.volume;

        // Gradually reduce the volume to 0 over the duration of fadeDuration
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
        audioSource.volume = startVolume; // Reset volume for future plays
    }
}

//sound effects needed

//walking
//surging
//normal attack and combo attack
//hit reaction
//dodge - jump
//death


