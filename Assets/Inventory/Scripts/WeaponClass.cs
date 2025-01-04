using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(fileName = "new Weapon Class", menuName = "Item/Weapon" )]
public class WeaponClass : ItemClass
{
    [Header("Weapon")]
    public WeaponType weaponType;
    public enum WeaponType
    {
        Sword,
        Axe
    }
    
    public override ItemClass GetItem() { return this; }
    public override WeaponClass GetWeapon() { return this;  }
    public override ConsumableClass GetConsumable() { return null;  }
    public override MiscClass GetMisc() { return null;  }
    public override Collectible GetCollectible() { return null;  }
}
