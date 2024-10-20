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

    void Update(){
        if (Input.GetMouseButtonDown(0) && canAttack){
            PerformAttack();
        }
    }

    private void PerformAttack(){
        Vector2 mousePosition = mouse.position.ReadValue();
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange)){
            EnemyController enemy = hit.collider.GetComponent<EnemyController>();

            if (enemy != null){
                enemy.TakeDamage(attackDamage);
            }
        }

        StartCoroutine(AttackCoolDown());
    }

    private IEnumerator AttackCoolDown(){
        canAttack = false;
        yield return new WaitForSeconds(attackCoolDown);
        canAttack = true;
    }
}