using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationLayerController : MonoBehaviour
{
    private Animator animator;
    private int noWeaponStanceIndex;
    private int weaponOverrideLayerIndex;
    private Coroutine currentTransition;
    [SerializeField] private float transitionDuration = 0.75f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        noWeaponStanceIndex = animator.GetLayerIndex("NoWeaponStance");
        weaponOverrideLayerIndex = animator.GetLayerIndex("WeaponOverrideLayer");
    }

    public void ActivateWeaponOverride()
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        // Reset the animation state for the override layer
        animator.Play("WeaponOverrideLayer", weaponOverrideLayerIndex, 0f);

        currentTransition = StartCoroutine(TransitionLayerWeight(noWeaponStanceIndex, 0f, weaponOverrideLayerIndex, 1f));
    }

    public void DeactivateWeaponOverride()
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
        currentTransition = StartCoroutine(TransitionLayerWeight(noWeaponStanceIndex, 1f, weaponOverrideLayerIndex, 0f));
    }

    private IEnumerator TransitionLayerWeight(int firstLayerIndex, float firstTargetWeight, 
                                            int secondLayerIndex, float secondTargetWeight)
    {
        float elapsedTime = 0f;
        float firstStartWeight = animator.GetLayerWeight(firstLayerIndex);
        float secondStartWeight = animator.GetLayerWeight(secondLayerIndex);

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            animator.SetLayerWeight(firstLayerIndex, 
                Mathf.Lerp(firstStartWeight, firstTargetWeight, smoothT));
            animator.SetLayerWeight(secondLayerIndex, 
                Mathf.Lerp(secondStartWeight, secondTargetWeight, smoothT));

            yield return null;
        }

        // Ensure we end up exactly at target weights
        animator.SetLayerWeight(firstLayerIndex, firstTargetWeight);
        animator.SetLayerWeight(secondLayerIndex, secondTargetWeight);

        currentTransition = null;
    }

    public void ResetTriggers(){
        animator.ResetTrigger("Unsheath");
        animator.ResetTrigger("Sheathe");
    }

    public void ResetLayerWeights()
    {
        for (int i = 1; i < animator.layerCount; i++)
        {
            animator.SetLayerWeight(i, 0);
        }
    }
}