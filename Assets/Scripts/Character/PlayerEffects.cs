using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace SG{
    public class PlayerEffects : PlayerEffectsManager{
        [Header("Debug Delete Later")]
        [SerializeField] InstantPlayerEffect effectToTest;
        [SerializeField] bool processEffect = false;

        private void Update(){

            if (processEffect){
                processEffect = false;

                if (effectToTest == null){
                    Debug.LogError("effectToTest is null! Assign an effect in the Inspector.");
                    return;
                }

                Debug.Log("Instantiating and applying effect: " + effectToTest.name);
                InstantPlayerEffect effect = Instantiate(effectToTest);
                ProcessInstantEffect(effect);
            }
        }

    }
}