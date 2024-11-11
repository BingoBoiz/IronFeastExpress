using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NewspaperManager : NetworkBehaviour
{
    public static NewspaperManager Instance {  get; private set; } 

    [SerializeField] List<ArticleSO> articleSOArray;

    private List<ArticleSO> dailyArticleRandomList;

    private int pageCurrentIndex = 1;
    private int pageIndexMax = 3;

    private void Awake()
    {
        Instance = this;
        dailyArticleRandomList = new List<ArticleSO>();
    }

    public void UpdateArticle()
    {
        UpdateNewArticleServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateNewArticleServerRpc()
    {
        //Debug.Log("Choosing " + pageIndexMax + " random articles from the database");
        dailyArticleRandomList.Clear();
        //Create new list to make sure all the article will be diffrent
        List<ArticleSO> tempArticleSOList = new List<ArticleSO>(articleSOArray);


        for (int i = 0; i < pageIndexMax; i++)
        {
            if (tempArticleSOList.Count == 0)
            {
                Debug.LogWarning("Reset newspaper database!!!!");
                tempArticleSOList = new List<ArticleSO>(articleSOArray);
            }

            //Debug.Log("Create ArticleSO number " + i+1);
            int randomIndex = UnityEngine.Random.Range(0, tempArticleSOList.Count);
            ArticleSO randomArticleSO = tempArticleSOList[randomIndex];

            int articleAddIndex = GetArticleSOIndex(randomArticleSO);
            AddingNewRandomArticleClientRpc(articleAddIndex);

            tempArticleSOList.RemoveAt(randomIndex);
        }
    }

    [ClientRpc]
    private void AddingNewRandomArticleClientRpc(int articleIndex)
    {
        /*if (!IsHost)
        {
            Debug.Log("[ClientRpc].AddArticle: " + GetArticleSOByIndex(articleIndex));
        }*/
        ArticleSO randomArticleSO = GetArticleSOByIndex(articleIndex);
        dailyArticleRandomList.Add(randomArticleSO);
    }

    private int GetArticleSOIndex(ArticleSO articleSO)
    {
        return articleSOArray.IndexOf(articleSO);
    }

    private ArticleSO GetArticleSOByIndex(int articleIndex)
    {
        return articleSOArray[articleIndex];
    }

    public string GetArticleHeadline()
    {
        return dailyArticleRandomList[pageCurrentIndex-1].headline;
    }
    
    public string GetArticleContent()
    {
        return dailyArticleRandomList[pageCurrentIndex-1].content;
    }

    public string GetPageCurrentIndex()
    {
        return pageCurrentIndex.ToString();
    }

    public string GetPageIndexMax()
    {
        return pageIndexMax.ToString();
    }

    public void ChangeToPreviousArticle()
    {
        if (pageCurrentIndex > 1)
        {
            pageCurrentIndex--;
        }
    }

    public void ChangeToNextArticle()
    {
        if (pageCurrentIndex < pageIndexMax)
        {
            pageCurrentIndex++;
        }
        
    }
}

