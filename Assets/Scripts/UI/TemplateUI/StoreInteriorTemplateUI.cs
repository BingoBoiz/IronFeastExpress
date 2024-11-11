using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreInteriorTemplateUI : MonoBehaviour
{

    public event EventHandler<OnStoreInteriorCountChangedEventArgs> OnStoreInteriorCountChanged;

    public class OnStoreInteriorCountChangedEventArgs : EventArgs
    {
        public StoreInteriorSO interiorSO; 
        public CountChange countChange;
        public float lastInteriorPrice;
    }

    public enum CountChange
    {
        Increase,
        Decrease,
    }

    [SerializeField] private TextMeshProUGUI interiorPrice;
    [SerializeField] private TextMeshProUGUI interiorName;

    [SerializeField] private TextMeshProUGUI currentNumberText;

    [SerializeField] private Transform interiorIcon;
    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;

    private float currentInteriorPrice = 0f;
    private int currentAddInteriorCount = 0;
    private StoreInteriorSO storeInteriorSO;


    private void Awake()
    {
        addButton.onClick.AddListener(() =>
        {
            UpdateBuyInteriorCount(CountChange.Increase);
        });
        removeButton.onClick.AddListener(() =>
        {
            if (currentAddInteriorCount == 0)
            {
                return;
            }
            UpdateBuyInteriorCount(CountChange.Decrease);
        });
    }

    private void Start()
    {
        currentNumberText.text = currentAddInteriorCount.ToString();
    }

    private void UpdateBuyInteriorCount(CountChange countChange)
    {
        float interiorPriceBeforeUpdate = 0;
        switch (countChange) 
        { 
            case CountChange.Increase:
                currentAddInteriorCount++;
                interiorPriceBeforeUpdate = currentInteriorPrice;
                currentInteriorPrice *= storeInteriorSO.priceIncrease;
                break;
            case CountChange.Decrease:
                currentAddInteriorCount--;
                currentInteriorPrice /= storeInteriorSO.priceIncrease;
                interiorPriceBeforeUpdate = currentInteriorPrice;
                break;
        }
        // Adjust the count
        currentNumberText.text = currentAddInteriorCount.ToString();

        // Update the visual price display
        interiorPrice.text = currentInteriorPrice.ToString("F2") + "$";

        OnStoreInteriorCountChanged?.Invoke(this, new OnStoreInteriorCountChangedEventArgs
        {
            interiorSO = storeInteriorSO,
            countChange = countChange,
            lastInteriorPrice = interiorPriceBeforeUpdate,
        });
    }

    public void ResetCountWhenBuy()
    {
        currentAddInteriorCount = 0;
        currentNumberText.text = currentAddInteriorCount.ToString();
    }
    public void UpdateVisualStoreInteriorUI(StoreInteriorSO storeInteriorSO)
    {
        this.storeInteriorSO = storeInteriorSO;
        currentInteriorPrice = storeInteriorSO.price;
        interiorPrice.text = currentInteriorPrice.ToString() + "$";
        //interiorIcon.GetComponent<Image>().sprite = storeInteriorSO.placedCabinetTypeSO.sprite;
        interiorIcon.GetComponent<Image>().sprite = storeInteriorSO.storeImage;
        interiorName.text = storeInteriorSO.storeInteriorName;
    }
}
