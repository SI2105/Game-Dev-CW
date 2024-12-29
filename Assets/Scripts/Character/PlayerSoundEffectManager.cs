using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerSoundEffectManager : MonoBehaviour{
    public AudioClip[] footsteps;
    public AudioClip[] swordSwings;
    public AudioClip[] swordHits;
    public AudioClip[] shieldBlocks;

    private AudioSource audioSource;

    private void Awake(){
        audioSource = GetComponent<AudioSource>();
    }
}