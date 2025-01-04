using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    public class WeaponColliderHandler : MonoBehaviour
    {
        public float damage;

        private void OnCollisionEnter(Collision collision)
        {
            // Check if the collided object is the Player
            if (collision.gameObject.CompareTag("Player"))
            {
                // Get the EnemyAIController from the parent
                EnemyAIController enemyAI = GetComponentInParent<EnemyAIController>();
                if (enemyAI != null && (enemyAI.isAttacking || enemyAI.isComboAttacking))
                {
                    // If the enemy is attacking, apply damage to the player
                    PlayerAttributesManager playerAttributes = collision.gameObject.GetComponent<PlayerAttributesManager>();
                    if (playerAttributes != null)
                    {
                        playerAttributes.TakeDamage(damage);
                    }
                }
                else
                {
                    Debug.Log("Enemy is not attacking, no damage applied.");
                }
            }
        }
    }
}
