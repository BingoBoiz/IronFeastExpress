using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StoreManagerUI : MonoBehaviour
{
    public static StoreManagerUI Instance {  get; private set; }

    [SerializeField] private Transform productScrollView;
    [SerializeField] private Transform productScrollViewContent;
    [SerializeField] private Transform productSingleTemplate;

    [SerializeField] private Transform interiorScrollView;
    [SerializeField] private Transform interiorScrollViewContent;
    [SerializeField] private Transform interiorSingleTemplate;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button productButton;
    [SerializeField] private Button interiorButton;
    [SerializeField] private TextMeshProUGUI totalCostText;

    private void Awake()
    {
        interiorScrollView.gameObject.SetActive(false);

        Instance = this;
        productSingleTemplate.gameObject.SetActive(false);

        closeButton.onClick.AddListener(() =>
        {
            Hide();
            StoreDesk.Instance.ToggleIsInteractingServerRpc();
            StoreUI.Instance.Hide();
        });

        buyButton.onClick.AddListener(() =>
        {
            StoreManager.Instance.BuyEverythingInOrderCart();
        });

        productButton.onClick.AddListener(() =>
        {
            productScrollView.gameObject.SetActive(true);
            interiorScrollView.gameObject.SetActive(false);

        });

        interiorButton.onClick.AddListener(() =>
        {
            interiorScrollView.gameObject.SetActive(true);
            productScrollView.gameObject.SetActive(false);
        });
    }

    private void Start()
    {
        Hide();
        StoreDesk.OnInteractStore += StoreDesk_OnClickStore;
        GameManager.Instance.OnStateChange += GameManager_OnStateChange;
        StoreManager.Instance.OnTotalCostChange += StoreManager_OnTotalCostChange;
        StoreManager.Instance.GetStoreInterior(interiorSingleTemplate, interiorScrollViewContent);
        totalCostText.text = StoreManager.Instance.GetTotalCost().ToString("F2");
    }

    private void StoreManager_OnTotalCostChange(object sender, System.EventArgs e)
    {
        totalCostText.text = StoreManager.Instance.GetTotalCost().ToString("F2");
    }
    private void GameManager_OnStateChange(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsTrainStop())
        {
            StoreManager.Instance.RequestRandomStoreProducts();
            
        }
        else if (GameManager.Instance.IsTrainRunning())
        {
            
        }
        else if (GameManager.Instance.IsShowingResult())
        {
            StoreManager.Instance.ClearStoreProduct(productScrollViewContent);
        }
    }

    private void StoreDesk_OnClickStore(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public Transform GetStoreProductSingleTemplate()
    {
        return productSingleTemplate;
    }

    public Transform GetProductScrollViewContent()
    {
        return productScrollViewContent;
    }
    public Transform GetInteriorScrollViewContent()
    {
        return interiorScrollViewContent;
    }
    private void OnDestroy()
    {
        StoreDesk.OnInteractStore -= StoreDesk_OnClickStore;
        GameManager.Instance.OnStateChange -= GameManager_OnStateChange;
        StoreManager.Instance.OnTotalCostChange -= StoreManager_OnTotalCostChange;
    }

}
