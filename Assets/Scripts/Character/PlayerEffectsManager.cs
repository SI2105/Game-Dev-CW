using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace SG{
    public class PlayerEffectsManager : MonoBehaviour
    {
        // PROCESS INSTANT EFFECTS (TAKE DAMAGE, HEAL)
        // PROCESS TIMED EFFECTS (POISON, BUILD UPS)
        // PROCESS STATIC EFFECTS (ADDING/REMOVING BUFFS)
        PlayerAttributesManager player;
    
        protected virtual void Awake(){
            player = GetComponent<PlayerAttributesManager>();
        }

        public void ProcessInstantEffect(InstantPlayerEffect effect){
            // TAKE IN AN EFFECT
            // PROCESS IT
            effect.ProcessEffect(player);
        }
    }
}