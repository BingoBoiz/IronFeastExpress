using UnityEngine;

public class PlayerLobbyIdleAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string[] idleAnimationNames; // List of random idle animations
    [SerializeField] private string specialAnimationName; // The special animation name

    private void OnEnable()
    {
        PlaySpecialAnimation();
    }

    private void PlaySpecialAnimation()
    {
        animator.Play(specialAnimationName);
        StartCoroutine(WaitForAnimationToEnd(specialAnimationName, PlayRandomIdleAnimation));
    }

    private void PlayRandomIdleAnimation()
    {
        int randomIndex = Random.Range(0, idleAnimationNames.Length);
        string selectedAnimation = idleAnimationNames[randomIndex];
        animator.Play(selectedAnimation);

        StartCoroutine(WaitForAnimationToEnd(selectedAnimation, PlayRandomIdleAnimation));
    }

    private System.Collections.IEnumerator WaitForAnimationToEnd(string animationName, System.Action callback)
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(animationName));
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        callback?.Invoke();
    }

    
}
