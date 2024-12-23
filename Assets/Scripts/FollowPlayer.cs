using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public Transform player;


    // Update is called once per frame
    void LateUpdate()
    {
        transform.position=player.position;
    }
}
