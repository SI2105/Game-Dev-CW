using UnityEngine;

public class SkyboxTransitionManager : MonoBehaviour
{
    public Shader transitionShader; // Assign the "Custom/SkyboxTransition" shader in the Inspector

    private Material transitionMaterial;

    public void StartTransition(Material currentSkybox, Material newSkybox, float duration)
    {
        if (transitionShader == null)
        {
            Debug.LogError("Transition Shader is not assigned!");
            return;
        }

        // Create or reuse the transition material
        if (transitionMaterial == null)
        {
            transitionMaterial = new Material(transitionShader);
        }

        // Assign the cube maps from the current and new skybox materials
        var currentTexture = currentSkybox?.GetTexture("_Tex");
        var newTexture = newSkybox?.GetTexture("_Tex");

    
        transitionMaterial.SetTexture("_AtmosphereTex", currentTexture);
        transitionMaterial.SetTexture("_SpaceTex", newTexture);

        // Set the initial transition factor to 0
        transitionMaterial.SetFloat("_TransitionFactor", 0.0f);

        // Apply the transition material to the skybox
        RenderSettings.skybox = transitionMaterial;

        // Start the transition coroutine
        StartCoroutine(TransitionCoroutine(duration, newSkybox));
    }

    private System.Collections.IEnumerator TransitionCoroutine(float duration, Material newSkybox)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float transitionFactor = Mathf.Clamp01(elapsedTime / duration);

            // Update the transition factor in the shader
            transitionMaterial.SetFloat("_TransitionFactor", transitionFactor);

            yield return null;
        }

        // Ensure the transition factor is fully set at the end
        transitionMaterial.SetFloat("_TransitionFactor", 1.0f);

        // Set the final skybox to the new material
        RenderSettings.skybox = newSkybox;

        Debug.Log("Skybox transition complete. New skybox applied.");
    }
}
