using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewspaperManagerUI : MonoBehaviour
{
    public static NewspaperManagerUI Instance {  get; private set; }

    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private TextMeshProUGUI headline;
    [SerializeField] private TextMeshProUGUI content;

    [SerializeField] private TextMeshProUGUI pageCurrentIndexText;
    [SerializeField] private TextMeshProUGUI pageIndexMaxText;

    private void Awake()
    {
        Instance = this;
        
        previousButton.onClick.AddListener(() =>
        {
            NewspaperManager.Instance.ChangeToPreviousArticle();
            UpdateArticleUI();
        });

        nextButton.onClick.AddListener(() =>
        {
            NewspaperManager.Instance.ChangeToNextArticle();
            UpdateArticleUI();
        });

        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        Hide();
        TrainManager.Instance.OnDoneSetUp += TrainManager_OnDoneSetUp;
        GameManager.Instance.OnStateChange += GameManager_OnStateChange;

    }

    private void TrainManager_OnDoneSetUp(object sender, System.EventArgs e)
    {
        NewspaperHolder.Instance.OnInteract += NewsPaperHolder_OnInteract;
    }

    private void GameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsTrainStop())
        {
            NewspaperManager.Instance.UpdateArticle();
        }
    }

    private void NewsPaperHolder_OnInteract(object sender, System.EventArgs e)
    {
        Show();
        UpdateArticleUI();
    }

    private void UpdateArticleUI()
    {
        headline.text = NewspaperManager.Instance.GetArticleHeadline();
        content.text = NewspaperManager.Instance.GetArticleContent();
        pageCurrentIndexText.text = NewspaperManager.Instance.GetPageCurrentIndex();
        pageIndexMaxText.text = NewspaperManager.Instance.GetPageIndexMax();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
