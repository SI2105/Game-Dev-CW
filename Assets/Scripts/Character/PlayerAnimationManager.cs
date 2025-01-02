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
        animator.SetBool("itemChange", true);
    }

    public void PlaySheatheAnimation()
    {
        animator.SetBool("itemChange", false);
    }
    
}