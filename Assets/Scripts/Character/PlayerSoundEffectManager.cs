using UnityEngine;

namespace SG{

    public class PlayerSoundManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        public AudioSource walkingAudioSource; // Audio source for walking sounds
        public AudioSource runningAudioSource; // Audio source for running sounds
        public AudioSource attackAudioSource;
        public AudioSource effectAudioSource;
        
        [Header("Breathing Sounds")]
        public AudioClip breathing_0_100; // 0-100 Breathing
        [Range(0f, 1f)] public float breathing_0_100Volume = 0.7f;
        public AudioClip breathing_50_100; // 50-100 Breathing
        [Range(0f, 1f)] public float breathing_50_100Volume = 0.7f;

        [Header("Death Sounds")]
        public AudioClip deathClip; // Death
        [Range(0f, 1f)] public float deathClipVolume = 1.0f;
        public AudioClip deathWithArmorClip; // Death With Armor
        [Range(0f, 1f)] public float deathWithArmorClipVolume = 1.0f;

        [Header("Footstep Sounds")]
        public AudioClip walkingLeftFootstepClip; // Left Footstep for walking
        [Range(0f, 1f)] public float walkingLeftFootstepVolume = 0.8f;
        public AudioClip walkingRightFootstepClip; // Right Footstep for walking
        [Range(0f, 1f)] public float walkingRightFootstepVolume = 0.8f;

        public AudioClip runningLeftFootstepClip; // Left Footstep for running
        [Range(0f, 1f)] public float runningLeftFootstepVolume = 0.9f;
        public AudioClip runningRightFootstepClip; // Right Footstep for running
        [Range(0f, 1f)] public float runningRightFootstepVolume = 0.9f;

        [Header("Healing Sounds")]
        public AudioClip healingSoundClip; // Healing Sound
        [Range(0f, 1f)] public float healingSoundVolume = 0.9f;

        [Header("Combat Sounds")]
        public AudioClip shieldHitClip; // Shield Hit
        [Range(0f, 1f)] public float shieldHitVolume = 0.9f;
        public AudioClip swordAndAxeLightAttack1WhooshClip; // Sword and Axe Light Attack 1 Whoosh
        [Range(0f, 1f)] public float lightAttack1WhooshVolume = 1.0f;
        public AudioClip swordAndAxeLightAttack2WhooshClip; // Sword and Axe Light Attack 2 Whoosh
        [Range(0f, 1f)] public float lightAttack2WhooshVolume = 1.0f;
        public AudioClip swordAndAxeSpinAttackWhooshClip; // Sword and Axe Spin Attack Whoosh
        [Range(0f, 1f)] public float spinAttackWhooshVolume = 1.0f;
        public AudioClip swordSlashHit1Clip; // Sword Slash Hit 1
        [Range(0f, 1f)] public float swordSlashHit1Volume = 0.9f;
        public AudioClip swordSlashHit2Clip; // Sword Slash Hit 2
        [Range(0f, 1f)] public float swordSlashHit2Volume = 0.9f;
        public AudioClip grabItem; // Sword Slash Hit 2
        [Range(0f, 1f)] public float grabItemVolume = 0.9f;
        public AudioClip grunt1; // Sword Slash Hit 2
        [Range(0f, 1f)] public float grunt1Volume = 0.9f;

        public AudioClip grunt2; // Sword Slash Hit 2
        [Range(0f, 1f)] public float grunt2Volume = 0.9f;
        public AudioClip grunt3; // Sword Slash Hit 2
        [Range(0f, 1f)] public float grunt3Volume = 0.9f;

        public AudioClip dodge; // Sword Slash Hit 2
        [Range(0f, 1f)] public float dodgeVolume = 0.9f;

        public AudioClip powerup1; // Sword Slash Hit 2
        [Range(0f, 1f)] public float powerup1Volume = 0.9f;

        public AudioClip powerup2; // Sword Slash Hit 2
        [Range(0f, 1f)] public float powerup2Volume = 0.9f;

        public AudioClip injuredClip;
        private void Awake()
        {
            if (walkingAudioSource == null || runningAudioSource == null)
            {
                Debug.LogError("Walking or Running AudioSource is missing on PlayerSoundManager!");
            }
        }

        /// <summary>
        /// Plays a sound clip on a specific audio source.
        /// </summary>
        private void PlaySound(AudioSource source, AudioClip clip, float volume)
        {
            if (source != null && clip != null)
            {
                source.PlayOneShot(clip, volume);
            }
        }

        // Methods for playing walking footstep sounds
        private bool lastFootWasLeft = false; // Tracks the last footstep sound

        public void PlayLeftFootAudioClipWalk()
        {
            if (!lastFootWasLeft) // Only play if the last sound wasn't the left foot
            {
                walkingAudioSource.PlayOneShot(walkingLeftFootstepClip, walkingLeftFootstepVolume);
                lastFootWasLeft = true;
            }
        }

        public void PlayRightFootAudioClipWalk()
        {
            if (lastFootWasLeft) // Only play if the last sound wasn't the right foot
            {
                walkingAudioSource.PlayOneShot(walkingRightFootstepClip, walkingRightFootstepVolume);
                lastFootWasLeft = false;
            }
        }

        // Methods for playing running footstep sounds
        public void PlayLeftFootAudioClipRun()
        {
            if (!lastFootWasLeft)
            {
                runningAudioSource.PlayOneShot(runningLeftFootstepClip, runningLeftFootstepVolume);
                lastFootWasLeft = true;
            }
        }

        public void PlayRightFootAudioClipRun()
        {
            if (lastFootWasLeft)
            {
                runningAudioSource.PlayOneShot(runningRightFootstepClip, runningRightFootstepVolume);
                lastFootWasLeft = false;
            }
        }


        // Other methods remain unchanged
        public void PlayHealingSoundClip()
        {
            PlaySound(effectAudioSource, healingSoundClip, healingSoundVolume); // Defaulting to walking audio source
        }

        public void PlayShieldHitClip()
        {
            PlaySound(walkingAudioSource, shieldHitClip, shieldHitVolume);
        }

        private bool lastAttackWas1 = false;
        public void PlayLightAttack1WhooshClip()
        {
            PlaySound(attackAudioSource, swordAndAxeLightAttack1WhooshClip, lightAttack1WhooshVolume);
            lastAttackWas1 = true; // Set the state
            Debug.Log("Played Light Attack 1 sound.");

        }

        public void PlayLightAttack2WhooshClip()
        {

            PlaySound(attackAudioSource, swordAndAxeLightAttack2WhooshClip, lightAttack2WhooshVolume);
            lastAttackWas1 = false; // Set the state
            Debug.Log("Played Light Attack 2 sound.");
        }

        public void PlaySpinAttackWhooshClip()
        {
            PlaySound(attackAudioSource, swordAndAxeSpinAttackWhooshClip, spinAttackWhooshVolume);
        }

        public void PlaySwordSlashHit1Clip()
        {
            PlaySound(attackAudioSource, swordSlashHit1Clip, swordSlashHit1Volume);
        }

        public void PlaySwordSlashHit2Clip()
        {
            PlaySound(attackAudioSource, swordSlashHit2Clip, swordSlashHit2Volume);
        }
        public void PlayGrabItemClip()
        {
            PlaySound(attackAudioSource, grabItem, grabItemVolume);
        }
        public void PlayGrunt1Clip()
        {
            PlaySound(attackAudioSource, grunt1, grunt1Volume);
        }
        public void PlayGrunt2Clip()
        {
            PlaySound(attackAudioSource, grunt2, grunt2Volume);
        }
        public void PlayGrunt3Clip()
        {
            PlaySound(attackAudioSource, grunt3, grunt3Volume);
        }

        public void PlayDodgeClip()
        {
            PlaySound(attackAudioSource, dodge, dodgeVolume);
        }

        public void PlayPowerUp1Clip()
        {
            PlaySound(attackAudioSource, powerup1, powerup1Volume);
        }
        public void PlayPowerUp2Clip()
        {
            PlaySound(attackAudioSource, powerup2, powerup2Volume);
        }

        public void InjuredClip(){
            PlaySound(attackAudioSource, injuredClip, 0.5f);
        }
    }
}
