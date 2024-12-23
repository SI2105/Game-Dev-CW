using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SG{
    public class WorldPlayerEffectsManager : MonoBehaviour{
        public static WorldPlayerEffectsManager instance;

        [SerializeField] List<InstantPlayerEffect> instantEffects;
        private void Awake(){
            if(instance ==null){
                instance = this;
            }else{
                Destroy(gameObject);
            }

            GenerateEffectIDs();
        }

        private void GenerateEffectIDs(){
            for (int i = 0; i < instantEffects.Count; i++){
                instantEffects[i].instantEffectID = i;
            }
        }
    }
}