using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewspaperManagerUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;

    [SerializeField] private Image image0;  
    [SerializeField] private TextMeshProUGUI headline0;
    [SerializeField] private TextMeshProUGUI content0;
    [SerializeField] private TextMeshProUGUI effect0;

    [SerializeField] private Image image1;
    [SerializeField] private TextMeshProUGUI headline1;
    [SerializeField] private TextMeshProUGUI content1;
    [SerializeField] private TextMeshProUGUI effect1;

    [SerializeField] private Image image2;
    [SerializeField] private TextMeshProUGUI headline2;
    [SerializeField] private TextMeshProUGUI content2;
    [SerializeField] private TextMeshProUGUI effect2;

    [SerializeField] private Image image3;
    [SerializeField] private TextMeshProUGUI headline3;
    [SerializeField] private TextMeshProUGUI content3;

    [SerializeField] private Image image4;
    [SerializeField] private TextMeshProUGUI headline4;
    [SerializeField] private TextMeshProUGUI content4;

    [SerializeField] private Image image5;
    [SerializeField] private TextMeshProUGUI headline5;
    [SerializeField] private TextMeshProUGUI content5;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
            StoreUI.Instance.Hide();
        });
    }

    private void Start()
    {
        Hide();
        TrainManager.Instance.OnDoneSetUp += TrainManager_OnDoneSetUp;
        NewspaperManager.Instance.OnUpdateNewspaper += NewspaperManager_OnUpdateNewspaper;
    }

    private void TrainManager_OnDoneSetUp(object sender, System.EventArgs e)
    {
        NewspaperHolder.Instance.OnInteract += NewsPaperHolder_OnInteract;
        
    }

    private void NewspaperManager_OnUpdateNewspaper(object sender, System.EventArgs e)
    {
        UpdateArticleUI();
    }

    private void NewsPaperHolder_OnInteract(object sender, System.EventArgs e)
    {
        Show();
    }

    private void UpdateArticleUI()
    {
        Debug.Log("UpdateArticleUI");
        List<ArticleSO> dailArticleRandomList = NewspaperManager.Instance.GetDailyArticleRandomList();
        List<ArticleOutcomeSO> dailyArticleOutComeList = NewspaperManager.Instance.GetTodayArticleOutComeList();

        image0.sprite = dailyArticleOutComeList[0].picture;
        headline0.text = dailyArticleOutComeList[0].headline;
        content0.text = dailyArticleOutComeList[0].content;
        effect0.text = dailyArticleOutComeList[0].effect;

        image1.sprite = dailyArticleOutComeList[1].picture;
        headline1.text = dailyArticleOutComeList[1].headline;
        content1.text = dailyArticleOutComeList[1].content;
        effect1.text = dailyArticleOutComeList[1].effect;

        image2.sprite = dailyArticleOutComeList[2].picture;
        headline2.text = dailyArticleOutComeList[2].headline;
        content2.text = dailyArticleOutComeList[2].content;
        effect2.text = dailyArticleOutComeList[2].effect;

        image3.sprite = dailArticleRandomList[0].picture;
        headline3.text = dailArticleRandomList[0].headline;
        content3.text = dailArticleRandomList[0].content;

        image4.sprite = dailArticleRandomList[1].picture;
        headline4.text = dailArticleRandomList[1].headline;
        content4.text = dailArticleRandomList[1].content;

        image5.sprite = dailArticleRandomList[2].picture;
        headline5.text = dailArticleRandomList[2].headline;
        content5.text = dailArticleRandomList[2].content;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        TrainManager.Instance.OnDoneSetUp -= TrainManager_OnDoneSetUp;
        NewspaperManager.Instance.OnUpdateNewspaper -= NewspaperManager_OnUpdateNewspaper;
    }
}
