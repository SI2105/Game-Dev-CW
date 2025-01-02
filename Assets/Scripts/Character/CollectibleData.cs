using UnityEngine;

[CreateAssetMenu(fileName = "NewCollectible", menuName = "Collectible/Item")]
public class CollectibleData : MonoBehaviour
{
    public string displayName;
    public Sprite icon;
    public int value;
    public Collectible collectible;
}
