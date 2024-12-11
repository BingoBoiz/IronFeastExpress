using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static ArticleSO;

public class NewspaperManager : NetworkBehaviour
{
    public static NewspaperManager Instance {  get; private set; } 

    public event EventHandler OnUpdateNewspaper;

    [SerializeField] ArticleSO[] articleSOArray;
    [SerializeField] ArticleOutcomeSO[] gameTutorialArticleSO;

    private List<ArticleSO> dailyArticleRandomList;
    private List<ArticleOutcomeSO> todayArticleOutComeList;
    private List<ArticleOutcomeSO> tomorrowArticleOutComeList; // For tommorrow to show

    private int articleCurrentIndex = 1;
    private int articleAmountMax = 3;

    private void Awake()
    {
        Instance = this;
        dailyArticleRandomList = new List<ArticleSO>();
        tomorrowArticleOutComeList = new List<ArticleOutcomeSO>();
    }

    private void Start()
    {
        todayArticleOutComeList = new List<ArticleOutcomeSO>(gameTutorialArticleSO);
        GameManager.Instance.OnStateChange += GameManager_OnStateChange;
    }

    private void GameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsTrainStop())
        {
            UpdateNewArticleServerRpc();
        }
        if (GameManager.Instance.IsShowingResult())
        {
            todayArticleOutComeList = new List<ArticleOutcomeSO>(tomorrowArticleOutComeList);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetTodayArticleOutComeServerRpc()
    {
        GetTodayArticleOutComeClientRpc();
    }

    [ClientRpc]
    private void GetTodayArticleOutComeClientRpc()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateNewArticleServerRpc()
    {
        //Debug.Log("Choosing " + pageIndexMax + " random articles from the database");
        dailyArticleRandomList.Clear();
        //Create new list to make sure all the article will be diffrent
        List<ArticleSO> tempArticleSOList = new List<ArticleSO>(articleSOArray);

        for (int i = 0; i < articleAmountMax; i++)
        {
            if (tempArticleSOList.Count == 0)
            {
                Debug.LogWarning("Reset newspaper database!!!!");
                tempArticleSOList = new List<ArticleSO>(articleSOArray);
            }

            //Debug.Log("Create ArticleSO number " + i+1);
            int randomArticleIndex = UnityEngine.Random.Range(0, tempArticleSOList.Count);
            ArticleSO randomArticleSO = tempArticleSOList[randomArticleIndex];
            int articleAddIndex = GetArticleSOIndex(randomArticleSO);
            AddingNewRandomArticleClientRpc(articleAddIndex);
            tempArticleSOList.RemoveAt(randomArticleIndex);

            int randomOutComeChance = UnityEngine.Random.Range(0, 100);
            GetTomorrowArticleOutComeClientRpc(articleAddIndex, randomOutComeChance);
        }
        CallingForUpdateUIClientRpc();
    }

    [ClientRpc]
    private void AddingNewRandomArticleClientRpc(int articleIndex)
    {
        ArticleSO randomArticleSO = GetArticleSOByIndex(articleIndex);
        dailyArticleRandomList.Add(randomArticleSO);
    }

    [ClientRpc]
    private void GetTomorrowArticleOutComeClientRpc(int articleIndex, int randomOutComeChance)
    {
        ArticleSO randomArticleSO = GetArticleSOByIndex(articleIndex);
        foreach (ArticleOutcomeData articleOutcomeData in randomArticleSO.articleOutcomeDataList)
        {
            if (randomOutComeChance - articleOutcomeData.outcomeChance < 0)
            {
                tomorrowArticleOutComeList.Add(articleOutcomeData.articleOutcome);
                Debug.Log(articleOutcomeData.articleOutcome.headline);
                return;
            }
            else
            {
                randomOutComeChance -= articleOutcomeData.outcomeChance;
            }
        }
    }

    [ClientRpc]
    private void CallingForUpdateUIClientRpc()
    {
        Debug.Log("CallingForUpdateUIClientRpc");
        OnUpdateNewspaper?.Invoke(this, EventArgs.Empty);
    }

    private int GetArticleSOIndex(ArticleSO articleSO)
    {
        //return articleSOArray.IndexOf(articleSO);
        return System.Array.IndexOf(articleSOArray, articleSO);
    }

    private ArticleSO GetArticleSOByIndex(int articleIndex)
    {
        return articleSOArray[articleIndex];
    }

    public List<ArticleSO> GetDailyArticleRandomList()
    {
        return dailyArticleRandomList;
    }
    
    public List<ArticleOutcomeSO> GetTodayArticleOutComeList()
    {
        return todayArticleOutComeList;
    }
}

