using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchievementShownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI achievementShownText;

    private void Start()
    {
        Hide();
        AchievementManager.Instance.OnNewRecipeDishUnlock += AchievementManager_OnNewRecipeDishUnlock;
    }

    private void AchievementManager_OnNewRecipeDishUnlock(object sender, AchievementManager.OnNewAchievementUnlockEventArgs e)
    {
        Shown();
        achievementShownText.text = e.achievementShownText;
        StartCoroutine(HideAchievement(3f));
    }

    IEnumerator HideAchievement(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }

    private void Shown()
    {
        this.gameObject.SetActive(true);
    }
    private void Hide()
    {
        this.gameObject.SetActive(false);
    }

}
