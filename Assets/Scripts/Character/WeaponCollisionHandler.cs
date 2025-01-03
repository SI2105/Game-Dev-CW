using UnityEngine;
using System.Collections;

namespace SG {

    public class WeaponCollisionHandler : MonoBehaviour
    {
        // Temporary damage value set by the PlayerComboManager
        private float temporaryDamage;

        // Reference to the PlayerState to check attack status
        public PlayerState _playerState;

        // Audio components for hit sounds
        private AudioSource audioSource;
        [Tooltip("Assign the hit sound clip in the Inspector.")]
        public AudioClip hitSound; // Assign via Inspector
        [Range(0f, 1f)]
        public float hitVolume = 0.5f;

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

            // Initialize PlayerState (assumes PlayerState is on the parent GameObject)

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

            // Initially disable the collider to prevent unintended hits
            // weaponCollider.enabled = false;
        }

        void Update(){
            Debug.Log("player state" + _playerState);
        }
        /// <summary>
        /// Sets the temporary damage value for the current attack.
        /// </summary>
        /// <param name="damage">Damage to be applied to enemies.</param>
        public void SetTemporaryDamage(float damage)
        {
            temporaryDamage = damage;
        }

        /// <summary>
        /// Activates the weapon's collider to detect hits and schedules its deactivation.
        /// </summary>
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

            // Schedule collider deactivation after a short delay to prevent multiple hits
            // StartCoroutine(DisableColliderAfterDelay(0.1f)); // Adjust delay as needed
        }

        private int internalCurrentComboStep;

        public void SetCurrentComboStep(int comboStep)
        {
            internalCurrentComboStep = comboStep;
        }

        /// <summary>
        /// Coroutine to disable the collider after a specified delay.
        /// </summary>
        /// <param name="delay">Time in seconds before disabling the collider.</param>
        /// <returns></returns>
        private IEnumerator DisableColliderAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            weaponCollider.enabled = false;
            Debug.Log("Weapon collider disabled after attack.");
        }

        /// <summary>
        /// Handles collision events with enemies.
        /// </summary>
        /// <param name="other">The Collider that enters the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            // Check if the collided object is tagged as "Enemy"
            if (other.CompareTag("Enemy"))
            {
                if (_playerState == null)
                {
                    Debug.LogError("PlayerState is not assigned.");
                    return;
                }

                print($"Hit {other.name} with damage: {temporaryDamage}");
                if (_playerState.IsInState(PlayerAttackState.Attacking)){
                    if (internalCurrentComboStep == 1)
                    {
                        print($"Hit {other.name} with damage: {temporaryDamage} with stat1: {_playerState.Attack1_progress}");
                        _playerState.Attack1_progress = false;
                        PlayHitSound();
                    }
                    else if (internalCurrentComboStep == 2)
                    {
                        print($"Hit {other.name} with damage: {temporaryDamage} with state2: {_playerState.Attack2_progress}");
                        _playerState.Attack2_progress = false;
                        PlayHitSound();
                    }
                    else if (internalCurrentComboStep == 3)
                    {
                        print($"Hit {other.name} with damage: {temporaryDamage} with state3: {_playerState.Attack2_progress}");
                        _playerState.Attack3_progress = false;
                        PlayHitSound();
                    }
                    internalCurrentComboStep = 0;

                }
                
                                
                // Apply damage to the enemy
                // if (other.TryGetComponent<Health>(out Health enemyHealth))
                // {
                //     enemyHealth.TakeDamage(temporaryDamage);
                // }
                // else
                // {
                //     Debug.LogWarning($"Enemy {other.name} does not have a Health component.");
                // }

                // Play hit sound if assigned
                
            }
        }

        /// <summary>
        /// Plays the assigned hit sound.
        /// </summary>
        private void PlayHitSound()
        {
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound, hitVolume);
                Debug.Log("Hit sound played.");
            }
            else
            {
                Debug.LogWarning("Hit sound or AudioSource not assigned.");
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
