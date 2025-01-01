using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemClass : ScriptableObject
{
    [Header("Item")]
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
    public bool Stackable;
    public float damage;

    public abstract ItemClass GetItem();
    public abstract WeaponClass GetWeapon();
    public abstract ConsumableClass GetConsumable();
    public abstract MiscClass GetMisc();
    public abstract Collectible GetCollectible();
}

