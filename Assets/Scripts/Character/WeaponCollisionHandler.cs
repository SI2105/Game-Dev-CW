using UnityEngine;

public class WeaponCollisionHandler : MonoBehaviour
{
    private float temporaryDamage;

    public void SetTemporaryDamage(float damage)
    {
        temporaryDamage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        print("here");
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Hit {other.name} with damage: {temporaryDamage}");

            // Apply damage logic
            //if (other.TryGetComponent<Health>(out var health))
            //{
             //   health.TakeDamage(temporaryDamage);
           // }
        }
    }
}
