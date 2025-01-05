using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class EnemyUIHudManager : MonoBehaviour
    {
        public Slider EnemyHealthBar;
        public TextMeshProUGUI EnemyName;
        private EnemyAIController enemyAI;
        public GameObject enemyBar;

        private void Awake()
        {
            enemyAI = GetComponentInParent<EnemyAIController>();
            // Subscribe to stamina/health events
            enemyAI.OnHealthChanged += UpdateEnemyUI;
            UpdateEnemyUI(100, 100);
            UpdateNameUI(enemyAI.enemyName);
        }
        
        private void UpdateNameUI(string enemyName)
        {
            if (EnemyName)
            {
                EnemyName.text = enemyName;
            }
        }

        // ------------------- Enemy / Health UI ------------------- //
        private void UpdateEnemyUI(float currentEnemyHealth, float maxHealth)
        {
            if (EnemyHealthBar)
            {
                EnemyHealthBar.maxValue = maxHealth;
                EnemyHealthBar.value = currentEnemyHealth;
            }
        }

       
}