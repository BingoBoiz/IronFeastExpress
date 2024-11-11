using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ArticleSO : ScriptableObject
{
    public Sprite picture;
    public string headline;
    [TextArea(minLines: 3, maxLines: 10)]
    public string content;

    [System.Serializable]
    public class ArticleOutcomeData
    {
        public ArticleOutcomeSO articleOutcome;
        [Range(0, 100)]
        public int outcomeChance;
    }

    public List<ArticleOutcomeData> articleOutcomeDataList = new List<ArticleOutcomeData>();

}
