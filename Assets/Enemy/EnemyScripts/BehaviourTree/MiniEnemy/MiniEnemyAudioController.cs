using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniEnemyAudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip attackSound;

    public void playWalk()
    {
        audioSource.PlayOneShot(walkSound, 0.2f);
    }

    public void playAttack(){
        audioSource.PlayOneShot(attackSound, 0.2f);
    }
}
