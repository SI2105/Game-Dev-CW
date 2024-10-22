using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
public class PlayerAttack : MonoBehaviour{
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCoolDown = 1f;
    private bool canAttack = true;

    [SerializeField] private Camera playerCamera;
    private Mouse mouse;

    private void Start(){
        mouse = Mouse.current;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            PerformAttack(); 
        }
    }

    /// <summary>
    /// Executes the attack by detecting and damaging an enemy within range.
    /// </summary>
    private void PerformAttack()
    {
        Vector2 mousePosition = mouse.position.ReadValue(); // Get current mouse position
        Ray ray = playerCamera.ScreenPointToRay(mousePosition); // Create a ray from the camera through the mouse position
        RaycastHit hit;

        // Perform the raycast to detect enemies within attack range
        if (Physics.Raycast(ray, out hit, attackRange))
        {
            EnemyController enemy = hit.collider.GetComponent<EnemyController>(); // Attempt to get the EnemyController component

            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage); // Inflict damage on the enemy
            }
        }

        StartCoroutine(AttackCoolDown()); // Start the attack cooldown coroutine
    }

    /// <summary>
    /// Manages the cooldown period between attacks.
    /// </summary>
    private IEnumerator AttackCoolDown()
    {
        canAttack = false; // Disable attacking
        yield return new WaitForSeconds(attackCoolDown); // Wait for the cooldown duration
        canAttack = true; // Re-enable attacking
    }
}