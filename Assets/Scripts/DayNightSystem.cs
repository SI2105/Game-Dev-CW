using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightSystem : MonoBehaviour
{

    public Light directionalLight;

    public float dayDurationInSeconds= 24.0f;

    public int currentHour;

    private float currentTimeOfDay=0.0f;

    public List<SkyboxMapping> timeMapping;

    private float blended_value=0.0f;

    // Update is called once per frame
    void Update()
    {

        // calculating what the current time of day is
        currentTimeOfDay += Time.deltaTime / dayDurationInSeconds;
        currentTimeOfDay %=1;

        // calculating what the current hour of the day is
        currentHour = Mathf.FloorToInt(currentTimeOfDay * 24);

        // change the direction of the light based on the current time of day

        directionalLight.transform.rotation = Quaternion.Euler(new Vector3((currentTimeOfDay * 360) - 90, 170, 0 ));

        // change the material of the sky based on the current time of day (night or day)
        changeSkyMaterial();

    }


    private void changeSkyMaterial(){
        
        //retrieve the correct material for current hour of day

        Material currentSkybox=null;

        foreach(SkyboxMapping mapping in timeMapping){

            //if the current hour of day matches the mapping hour, set the current skybox to mapping skybox
            if(currentHour==mapping.hour){
                currentSkybox=mapping.skyboxMaterial;


                if (currentSkybox.shader!=null){
                    if(currentSkybox.shader.name== "Custom/SkyboxTransition"){
                        blended_value+= Time.deltaTime;
                        blended_value= Mathf.Clamp01(blended_value);

                        currentSkybox.SetFloat("_TransitionFactor", blended_value);
                    }

                    else{
                        blended_value=0.0f;
                    }
                }


                break;
            }
        }


        if(currentSkybox!=null){
            //set the skybox material to the correct material
            RenderSettings.skybox=currentSkybox;
        }

    }
}


[System.Serializable]
//Mapping class to map hour of day to skybox material
public class SkyboxMapping{
    public string phaseName;
    public int hour;
    public Material skyboxMaterial;
}
