using UnityEngine;

public class WeaponCollisionHandler : MonoBehaviour
{
    private float temporaryDamage;

    public void SetTemporaryDamage(float damage)
    {
        temporaryDamage = damage;
    }
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on WeaponCollisionHandler. Add one to play sound.");
        }
        print("here sdsd");
    }
    public AudioClip hit; // Sword Slash Hit 2
    [Range(0f, 1f)] public float hitVolume = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        print("trigger");
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Hit {other.name} with damage: {temporaryDamage}");
            if (audioSource != null)
            {
                audioSource.PlayOneShot(hit, hitVolume);
            }

            // Apply damage logic
            //if (other.TryGetComponent<Health>(out var health))
            //{
             //   health.TakeDamage(temporaryDamage);
           // }
        }
    }
}
