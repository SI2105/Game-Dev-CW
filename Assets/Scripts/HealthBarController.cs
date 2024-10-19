using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    public Transform camera;

    
    void LateUpdate()
    {
        transform.LookAt(camera);
    }
}
