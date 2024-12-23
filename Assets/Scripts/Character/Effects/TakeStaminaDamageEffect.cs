using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG{
    [CreateAssetMenu(menuName = "Player Effects/Instant Effects/Take Stamina Damage")]
    public class TakeStaminaDamageEffect : InstantPlayerEffect
    {
        public float staminaDamage;

        public override void ProcessEffect(PlayerAttributesManager player){
            CalculateStaminaDamage(player);
            Debug.Log("Processing effect: " + player);
        }

        private void CalculateStaminaDamage(PlayerAttributesManager player){
            Debug.Log("Before " + player.CurrentStamina);
            player.UseStamina(staminaDamage);
            Debug.Log("After " + player.CurrentStamina);
        }
    }
}
