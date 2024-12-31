using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Collectible class", menuName = "Item/Class")]
public class CollectibleClass : ItemClass
{
    public override ItemClass GetItem() { return this; }
    public override WeaponClass GetWeapon() { return null; }
    public override ConsumableClass GetConsumable() { return null; }
    public override MiscClass GetMisc() { return this; }
}
