using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] public GameObject[] items;

    private float spawnRadius =2f;
    private bool isOpened = false;


    public string getState(){
        if (isOpened){
            return "Chest opened and items spawned";
        }
        return "";
    }

    public void OpenChest(){
        if (isOpened){
            return;
        }

        isOpened = true;

        foreach(GameObject item in items){
            Vector3 randomPos = GetRandomSpawnPosition();
            Instantiate(item, randomPos, Quaternion.identity);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randPosition = new Vector3(randCircle.x, 0f, randCircle.y); 
        return transform.position + randPosition;
    }
}
