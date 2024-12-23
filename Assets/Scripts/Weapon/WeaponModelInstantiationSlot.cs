using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponModelInstantiationSlot : MonoBehaviour
{
    public GameObject currentWeaponModel;
    public enum WeaponModelSlot{
        RightHand,
        LeftHand,
    }

    public WeaponModelSlot weaponSlot;

    public void UnloadWeapon(){
        if (currentWeaponModel != null){
            Destroy(currentWeaponModel);
        }
    }

    public void LoadWeapon(GameObject weaponModel){
        currentWeaponModel = weaponModel;
        weaponModel.transform.parent = transform;
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localRotation = Quaternion.identity;
        weaponModel.transform.localScale = Vector3.one;
    }

}
