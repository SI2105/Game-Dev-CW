using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayUnsheathAnimation()
    {
        animator.SetTrigger("Unsheath");
    }

    public void PlaySheatheAnimation()
    {
        animator.SetTrigger("Sheathe");
    }

    
}