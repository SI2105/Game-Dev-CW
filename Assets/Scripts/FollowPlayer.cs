using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;

    // Update is called once per frame
    void LateUpdate()
    {
        // Get the current y position of the object
        float currentY = transform.position.y;

        // Set the position with the same x and z as the player, but keep the current y
        transform.position = new Vector3(player.position.x, currentY, player.position.z);
    }
}
