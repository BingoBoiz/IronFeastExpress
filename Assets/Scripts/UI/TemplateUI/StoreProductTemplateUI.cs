using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StoreProductTemplateUI : MonoBehaviour
{
    public event EventHandler<OnStoreProductCountChangedEventArgs> OnStoreProductCountChanged;

    public class OnStoreProductCountChangedEventArgs: EventArgs
    {
        public string productName; //Identify StoreProductSO by using string is terrible, hope i will fix this in the future
        public int changeAmount;
    }

    [SerializeField] private TextMeshProUGUI productPrice;
    [SerializeField] private TextMeshProUGUI productName;

    [SerializeField] private TextMeshProUGUI currentNumberText;

    [SerializeField] private Transform productIcon;
    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;

    private float productPriceFloat = 0f;
    private int currentAddKitchenObjectCount = 0;

    private void Awake()
    {
        addButton.onClick.AddListener(() =>
        {
            UpdateBuyKitchenObjectCount(1);
            
            //OnBuyProduct?.Invoke(this, new OnBuyProductEventArgs { productName = productName.text});
        });
        removeButton.onClick.AddListener(() =>
        {
            if (currentAddKitchenObjectCount == 0)
            {
                return;
            }
            UpdateBuyKitchenObjectCount(-1);
        });
    }

    private void UpdateBuyKitchenObjectCount(int changeAmount)
    {
        currentAddKitchenObjectCount += changeAmount;
        currentNumberText.text = currentAddKitchenObjectCount.ToString();
        OnStoreProductCountChanged?.Invoke(this, new OnStoreProductCountChangedEventArgs
        {
            productName = productName.text,
            changeAmount = changeAmount
        });
    }

    private void Start()
    {
        currentNumberText.text = currentAddKitchenObjectCount.ToString();
    }

    public void UpdateVisualStoreProductUI(StoreProductSO storeProductSO)
    {
        productPriceFloat = storeProductSO.productPrice;
        productPrice.text = productPriceFloat.ToString() + "$";
        productIcon.GetComponent<Image>().sprite = storeProductSO.kitchenObjectSO.sprite;
        productName.text = storeProductSO.kitchenObjectSO.objectName;
    }

    public void ResetCountWhenBuy()
    {
        currentAddKitchenObjectCount = 0;
        currentNumberText.text = currentAddKitchenObjectCount.ToString();
    }

    public string GetProductName()
    {
        return productName.text;
    }

    public int GetAddKitchenObjectCount() 
    {
        return currentAddKitchenObjectCount;
    }
}
