using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningFlash : MonoBehaviour
{
    private float minTime = 0.5f;
    private float threshold = 0.5f;
    private float lastTime = 0.0f;
    public Light directionalLight;

    // Update is called once per frame
    void Update()
    {
        // Use Time.time for cumulative time
        if (Time.time - lastTime > minTime)
        {
            lastTime = Time.time; // Update lastTime to the current time

            if (Random.value > threshold)
            {
                directionalLight.enabled = true;
            }
            else
            {
                directionalLight.enabled = false;
            }
        }
    }
}
