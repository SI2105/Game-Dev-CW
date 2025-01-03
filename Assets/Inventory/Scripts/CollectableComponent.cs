using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleComponent : MonoBehaviour
{
    // Start is called before the first frame update
    public Collectible collectibleItem;

    public Collectible GetCollectible()
    {
        return collectibleItem;
    }
}
