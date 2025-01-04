using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG{
    public class WeaponColliderHandler : MonoBehaviour
    {
        public float damage;

        private bool hasCollided = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (hasCollided) return; // Prevent multiple triggers
            hasCollided = true;
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerAttributesManager playerAttributes = collision.gameObject.GetComponent<PlayerAttributesManager>();
                if (playerAttributes != null)
                {
                    playerAttributes.TakeDamage(damage);
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                hasCollided = false; // Reset the flag when the collision ends
            }
        }

    }
}
