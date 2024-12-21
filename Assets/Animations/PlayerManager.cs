using UnityEngine;

namespace SG{
    public class PlayerManager : MonoBehaviour{
        public TwoDimensionalAnimationController playerController;
        public PlayerAttributesManager attributesManagers;

        void Awake(){
            playerController = new TwoDimensionalAnimationController();
            attributesManagers = new PlayerAttributesManager();
        }
    }
}