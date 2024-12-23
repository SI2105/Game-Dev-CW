using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SG{
    public class UI_StatBar : MonoBehaviour
    {
        private Slider slider;
        // VARIABLE TO SCALE BAR SIZE DEPENDING ON STAT = HIGHER STAT = LONGER BAR ACCROSS SCREEN

        protected virtual void Awake(){
            slider = GetComponent<Slider>();
        }

        public virtual void SetStat(float value){
            slider.value = value;
        }

        public virtual void SetMaxStat(float maxValue){
            slider.maxValue = maxValue;
            slider.value = maxValue;
        }

    }
}