using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerMainMenuIdleAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string[] idleAnimationTriggers; // List of trigger names for idle animations

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        StartCoroutine(PlayRandomIdleAnimation());
    }

    private IEnumerator PlayRandomIdleAnimation()
    {
        while (true)
        {
            // Randomly choose an idle animation from the list
            string randomIdleTrigger = idleAnimationTriggers[Random.Range(0, idleAnimationTriggers.Length)];
            
            // Set the trigger to play the chosen idle animation
            animator.SetTrigger(randomIdleTrigger);

            // Wait for the current animation's duration
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
        }
    }

}
