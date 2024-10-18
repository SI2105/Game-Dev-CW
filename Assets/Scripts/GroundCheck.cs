using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    // Start is called before the first frame update


    // Update is called once per frame
    public Transform groundCheckPosition;

    void Update()
    {
        transform.position = groundCheckPosition.position;
    }
}
