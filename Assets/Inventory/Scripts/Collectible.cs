using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Collectible class", menuName = "Item/Collectible")]
public class Collectible : ItemClass
{
    public override ItemClass GetItem() { return this; }
    public override WeaponClass GetWeapon() { return null; }
    public override ConsumableClass GetConsumable() { return null; }
    public override Collectible GetCollectible() { return this; }
    public override MiscClass GetMisc() { return null;  }

}
