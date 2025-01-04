using UnityEngine;
using System.Collections;
using System;

namespace SG
{
    public class WeaponCollisionHandler : MonoBehaviour
    {
        // Temporary damage value set by the PlayerComboManager
        private float temporaryDamage;

        // Reference to the PlayerState to check attack status
        public PlayerState _playerState;

        // Audio components for hit sounds
        private AudioSource audioSource;

        [Tooltip("Assign the hit sound clips in the Inspector.")]
        public AudioClip[] hitSounds; // Array to hold multiple hit sounds
        [Range(0f, 1f)]
        public float hitVolume = 0.5f;

        // Index to keep track of the current sound
        private int currentHitSoundIndex = 0;

        // Collider component (ensure it's set as a Trigger in the Inspector)
        private Collider weaponCollider;

        private void Awake()
        {
            // Initialize AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("No AudioSource found on WeaponCollisionHandler. Adding one automatically.");
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Initialize Collider
            weaponCollider = GetChildComponent<Collider>();
            if (weaponCollider == null)
            {
                Debug.LogError("No Collider component found on WeaponCollisionHandler.");
            }
            else if (!weaponCollider.isTrigger)
            {
                Debug.LogWarning("Weapon collider should be set as a Trigger. Setting it as Trigger now.");
                weaponCollider.isTrigger = true;
            }
        }

        private void Update()
        {
            Debug.Log("Player state: " + _playerState);
        }

        public void SetTemporaryDamage(float damage)
        {
            temporaryDamage = damage;
        }

        public void TriggerAttack()
        {
            if (weaponCollider == null)
            {
                Debug.LogWarning("WeaponCollider is not assigned.");
                return;
            }

            // Enable the collider to detect hits
            weaponCollider.enabled = true;
            Debug.Log("Weapon collider enabled for attack.");
        }

        private int internalCurrentComboStep;

        public void SetCurrentComboStep(int comboStep)
        {
            internalCurrentComboStep = comboStep;
        }

        public event Action OnHit;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                if (_playerState == null)
                {
                    Debug.LogError("PlayerState is not assigned.");
                    return;
                }
                EnemyAIController enemy = other.GetComponent<EnemyAIController>();

                print($"Hit {other.name} with damage: {temporaryDamage}");
                if (_playerState.IsInState(PlayerAttackState.Attacking))
                {
                    OnHit?.Invoke();
                    enemy.takeDamage(temporaryDamage);
                    PlayHitSound();
                }
            }
        }

        /// <summary>
        /// Plays the next hit sound in the sequence.
        /// </summary>
        private void PlayHitSound()
        {
            if (audioSource != null && hitSounds != null && hitSounds.Length > 0)
            {
                // Play the current sound
                audioSource.PlayOneShot(hitSounds[currentHitSoundIndex], hitVolume);

                // Increment and loop the index
                currentHitSoundIndex = (currentHitSoundIndex + 1) % hitSounds.Length;

                Debug.Log($"Hit sound {currentHitSoundIndex} played.");
            }
            else
            {
                Debug.LogWarning("Hit sounds array is empty or AudioSource is not assigned.");
            }
        }

        private T GetChildComponent<T>() where T : Component
        {
            foreach (Transform child in transform)
            {
                var component = child.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }
    }
}
