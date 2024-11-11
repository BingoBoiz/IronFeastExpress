using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AchievementManager : NetworkBehaviour
{
    public static AchievementManager Instance {  get; private set; }
    public event EventHandler<OnNewAchievementUnlockEventArgs> OnNewRecipeDishUnlock;

    [SerializeField] private FinishDishListSO lockedDishListSO;
    private List<FinishDishSO> lockedDishList;

    public class OnNewAchievementUnlockEventArgs : EventArgs
    {
        public string achievementShownText;
    }

    private void Awake()
    {
        Instance = this;

    }
    private void Start()
    {
        lockedDishList = new List<FinishDishSO>(lockedDishListSO.finishDishSOList);

        RecipeBookManager.OnUnlockNewDishAchievement += RecipeBookManager_OnUnlockNewDishAchievement;
    }

    private void RecipeBookManager_OnUnlockNewDishAchievement(RecipeBookManager.OnUnlockNewDishAchievementEventArgs obj)
    {
        // Show up achievement
        Debug.Log("You just unlock: " + obj.newUnlockFinishDishSO.finishDishName);
        OnNewRecipeDishUnlock?.Invoke(this, new OnNewAchievementUnlockEventArgs
        {
            achievementShownText = "You just unlock: " + obj.newUnlockFinishDishSO.finishDishName
        });
    }

    public List<FinishDishSO> GetLockedDishList()
    {
        return lockedDishList;
    }

    public void RemoveLockedDishList(FinishDishSO removeDishSO)
    {
        lockedDishList.Remove(removeDishSO);
    }
}
