using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG{
    public class InstantPlayerEffect : ScriptableObject
    {
        [Header("Effect ID")]
        public int instantEffectID;

        public virtual void ProcessEffect(PlayerAttributesManager player){
        }
    }
}
