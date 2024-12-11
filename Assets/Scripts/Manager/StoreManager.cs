using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static StoreInteriorTemplateUI;

public class StoreManager : NetworkBehaviour
{
    public static StoreManager Instance { get; private set; }
    public event EventHandler OnTotalCostChange;

    public event EventHandler<OnBuyProductEventArgs> OnBuyProduct;
    public class OnBuyProductEventArgs : EventArgs // Firing event for BarrelGeneratorManager.cs
    {
        /*public float totalCost;*/
        public List<StoreProductSO> storeProductSOList;
        public List<int> storeProductCountList;
    }

    public event EventHandler<OnBuyInteriorCabinetEventArgs> OnBuyInteriorCabinet; 
    public class OnBuyInteriorCabinetEventArgs : EventArgs // Firing event for BarrelGeneratorManager.cs
    {
        public float totalCost;
        public List<StoreInteriorSO> storeInteriorSOList;
        public List<int> storeCabinetCountList;
    }

    [SerializeField] private StoreProductSO[] storeProductSOArray;
    [SerializeField] private StoreInteriorSO[] storeInteriorArray;


    private List<StoreProductSO> dailyRandomStoreProductSOList = new();
    private List<StoreProductSO> cartOrderStoreProductSOList = new();
    private List<StoreInteriorSO> cartOrderStoreInteriorSOList = new();
    private List<int> cartOrderStoreProductCountList = new();
    private List<int> cartOrderStoreInteriorCountList = new();
    private List<int> randomProductIndexes = new();
    private int storeProductMaximumAmount = 6;
    private float storeTotalCost = 0;


    //private NetworkVariable<float> playerMoneyHave = new NetworkVariable<float>(400);

    private Transform contentStoreManagerUI;
    //private StoreProductSO buyStoreProductSO;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        contentStoreManagerUI = StoreManagerUI.Instance.GetProductScrollViewContent();
    }

    public void RequestRandomStoreProducts()
    {
        if (IsServer)
        {
            RequestRandomStoreProductsServerRpc();
        }
        SetAllTheStoreProductTemplateIntoContent();
    }

    public void GetStoreInterior(Transform storeInteriorTemplate, Transform contentStoreInteriorManagerUI)
    {
        foreach (StoreInteriorSO storeInteriorSO in storeInteriorArray)
        {
            //Debug.Log("storeInteriorSO: "+ storeInteriorSO);
            Transform productInteriorTransform = Instantiate(storeInteriorTemplate, contentStoreInteriorManagerUI);

            productInteriorTransform.gameObject.SetActive(true);

            StoreInteriorTemplateUI storeInteriorTemplateUI = productInteriorTransform.GetComponent<StoreInteriorTemplateUI>();
            storeInteriorTemplateUI.UpdateVisualStoreInteriorUI(storeInteriorSO);

            storeInteriorTemplateUI.OnStoreInteriorCountChanged += StoreInteriorTemplateUI_OnStoreInteriorCountChanged;
        }
    }

    private void StoreInteriorTemplateUI_OnStoreInteriorCountChanged(object sender, StoreInteriorTemplateUI.OnStoreInteriorCountChangedEventArgs e)
    {
        int interiorIndex; 
        switch (e.countChange)
        {
            case CountChange.Increase:
                if (cartOrderStoreInteriorSOList.Contains(e.interiorSO)) // Already in the list
                {
                    interiorIndex = cartOrderStoreInteriorSOList.IndexOf(e.interiorSO);
                    cartOrderStoreInteriorCountList[interiorIndex]++;
                }
                else // Adding new first time
                {
                    cartOrderStoreInteriorSOList.Add(e.interiorSO);
                    cartOrderStoreInteriorCountList.Add(1);
                }
                CalculateTotalCostServerRpc(e.lastInteriorPrice);
                break;
            case CountChange.Decrease:
                interiorIndex = cartOrderStoreInteriorSOList.IndexOf(e.interiorSO);
                if (cartOrderStoreInteriorCountList[interiorIndex]>1) // After decrease still have at least 1 left
                {
                    cartOrderStoreInteriorCountList[interiorIndex]--;
                }
                else // After decrease will remove this interior completely out of the list
                {
                    cartOrderStoreInteriorCountList.RemoveAt(interiorIndex);
                    cartOrderStoreInteriorSOList.RemoveAt(interiorIndex);
                }
                CalculateTotalCostServerRpc(-e.lastInteriorPrice);
                break;
        }
        Debug.Log("The price of the " + e.interiorSO.storeInteriorName+ " is: " + e.lastInteriorPrice);
    }

    private List<int> GetRandomProductIndexes()
    {
        randomProductIndexes.Clear(); // Clear list before adding new indexes
        List<StoreProductSO> tempStoreProductSOList = new List<StoreProductSO>(storeProductSOArray);

        for (int i = 0; i < storeProductMaximumAmount; i++)
        {
            if (tempStoreProductSOList.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, tempStoreProductSOList.Count);
                StoreProductSO selectedProduct = tempStoreProductSOList[randomIndex];

                // Save storeProductSO index of storeProductSOArray 
                int originalIndex = Array.IndexOf(storeProductSOArray, selectedProduct);

                randomProductIndexes.Add(originalIndex);
                tempStoreProductSOList.RemoveAt(randomIndex); // Remove for avoid being replicate
            }
        }
        return randomProductIndexes;
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestRandomStoreProductsServerRpc()
    {
        List<int> randomProductIndexes = GetRandomProductIndexes();
        if (!IsHost)
        {
            Debug.Log("[ClientRpc].Count: " + randomProductIndexes.Count);
        }
        // Call ClientRpc to sent the results to all the clients
        SendRandomDailyStoreProductsClientRpc(randomProductIndexes.ToArray());
    }

    [ClientRpc]
    private void SendRandomDailyStoreProductsClientRpc(int[] randomProductIndexes)
    {
        dailyRandomStoreProductSOList.Clear();
        if (!IsHost)
        {
            Debug.Log("[ClientRpc].Length: " + randomProductIndexes.Length);
        }
        foreach (int index in randomProductIndexes)
        {
            if (index >= 0 && index < storeProductSOArray.Length)
            {
                StoreProductSO randomStoreProductSO = storeProductSOArray[index];
                dailyRandomStoreProductSOList.Add(randomStoreProductSO);
                //Debug.Log("Random Product: " + randomStoreProductSO.name);
            }
        }
    }

    public void ClearStoreProduct(Transform content)
    {
        //Cycle through the content to reset everything 
        foreach (Transform child in content)
        {
            if (child.GetComponent<StoreProductTemplateUI>() == null)
            {
                //Debug.Log("Child being ignore");
                continue;
            }
            Destroy(child.gameObject);
            Debug.Log(child.gameObject.GetComponent<StoreProductTemplateUI>().GetProductName() + " just get destroy");
        }
        dailyRandomStoreProductSOList.Clear();
    }

    private void SetAllTheStoreProductTemplateIntoContent()
    {
        //Debug.Log("SetAllTheStoreProductTemplateIntoContent");
        // Clear content before adding new items (optional but recommended)
        foreach (Transform child in contentStoreManagerUI)
        {
            Debug.Log("Check if the client is destroyng child");
            Destroy(child.gameObject);
        }

        Transform storeProductTemplate = StoreManagerUI.Instance.GetStoreProductSingleTemplate();
        foreach (StoreProductSO storeProductSO in dailyRandomStoreProductSOList)
        {
            // Make a copy of storeProductSingleTemplate inside content
            Transform productStoreTransform = Instantiate(storeProductTemplate, contentStoreManagerUI);
            //Debug.Log("Instantiate: " + storeProductSO.name);
            productStoreTransform.gameObject.SetActive(true);

            // Update the visual of the StoreProductSingleUI through an StoreProductSO of that copy 'storeProductSingleTemplate'
            var storeProductTemplateUI = productStoreTransform.GetComponent<StoreProductTemplateUI>();
            storeProductTemplateUI.UpdateVisualStoreProductUI(storeProductSO);
            storeProductTemplateUI.OnStoreProductCountChanged += StoreProductSingleUI_OnStoreProductCountChanged;
        }
    }

    private void StoreProductSingleUI_OnStoreProductCountChanged(object sender, StoreProductTemplateUI.OnStoreProductCountChangedEventArgs e)
    {
        // Find the matching product in the dailyRandomStoreProductSOList by name
        StoreProductSO matchingProduct = dailyRandomStoreProductSOList.FirstOrDefault(product => product.kitchenObjectSO.objectName == e.productName);

        if (matchingProduct == null)
        {
            Debug.LogWarning("Cannot find the product name: " + e.productName);
            return;
        }

        // Check if the product is already in the cart
        int productIndex = cartOrderStoreProductSOList.IndexOf(matchingProduct);
        if (productIndex >= 0)
        {
            // Product already in the cart, update its count and recalculate total cost
            cartOrderStoreProductCountList[productIndex] += e.changeAmount;
            CalculateTotalCostServerRpc(matchingProduct.productPrice * e.changeAmount);
        }
        else
        {
            // Add new product to the cart
            cartOrderStoreProductSOList.Add(matchingProduct);
            cartOrderStoreProductCountList.Add(e.changeAmount);
            CalculateTotalCostServerRpc(matchingProduct.productPrice * e.changeAmount);
        }

        Debug.Log("The price of the " + matchingProduct.kitchenObjectSO.objectName + " is: " + matchingProduct.productPrice);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CalculateTotalCostServerRpc(float totalSingleProductCost)
    {
        CalculateTotalCostClientRpc(totalSingleProductCost);
    }

    [ClientRpc]
    private void CalculateTotalCostClientRpc(float totalSingleProductCost)
    {
        storeTotalCost += totalSingleProductCost;
        OnTotalCostChange?.Invoke(this, EventArgs.Empty);
    }

    public void BuyEverythingInOrderCart()
    {
        // Convert lists to arrays for RPC
        int[] cartOrderStoreProductCountArray = cartOrderStoreProductCountList.ToArray();
        int[] cartOrderStoreInteriorCountArray = cartOrderStoreInteriorCountList.ToArray();
        int[] cartOrderStoreProductIndexArray = cartOrderStoreProductSOList.Select(product => Array.IndexOf(storeProductSOArray, product)).ToArray();
        int[] cartOrderStoreInteriorIndexArray = cartOrderStoreInteriorSOList.Select(interior => Array.IndexOf(storeInteriorArray, interior)).ToArray();

        BuyProductLogicServerRpc(cartOrderStoreProductCountArray, cartOrderStoreInteriorCountArray, cartOrderStoreProductIndexArray, cartOrderStoreInteriorIndexArray);
    }

    [ServerRpc(RequireOwnership = false)]
    private void BuyProductLogicServerRpc(int[] cartOrderStoreProductCountArray, int[] cartOrderStoreInteriorCountArray, int[] cartOrderStoreProductIndexArray, int[] cartOrderStoreInteriorIndexArray)
    {
        if (FinanceSystem.Instance.CheckPlayerBalanceBeforePurchase(storeTotalCost))
        {
            Debug.LogWarning("Not enough money");
            return;
        }

        FinanceSystem.Instance.ProcessStorePurchase(storeTotalCost);
        Debug.Log("BuyProductLogicServerRpc");
        if (cartOrderStoreProductCountArray.Length != 0)
        {

            // Convert index array back to product objects for event
            List<StoreProductSO> storeProductSOList = cartOrderStoreProductIndexArray.Select(index => storeProductSOArray[index]).ToList();
            List<int> storeProductCountList = cartOrderStoreProductCountArray.ToList();

            OnBuyProduct.Invoke(this, new OnBuyProductEventArgs
            {
                storeProductSOList = storeProductSOList,
                storeProductCountList = storeProductCountList,
            });

            BuyProductLogicClientRpc(cartOrderStoreProductCountArray, cartOrderStoreProductIndexArray);

        }
        if (cartOrderStoreInteriorCountArray.Length != 0)
        {
            List<StoreInteriorSO> storeInteriorSOList = cartOrderStoreInteriorIndexArray.Select(index => storeInteriorArray[index]).ToList();
            List<int> storeCabinetCountList = cartOrderStoreInteriorCountArray.ToList();

            OnBuyInteriorCabinet.Invoke(this, new OnBuyInteriorCabinetEventArgs
            {
                storeCabinetCountList = storeCabinetCountList, 
                storeInteriorSOList = storeInteriorSOList,
            });

            BuyInteriorLogicClientRpc(cartOrderStoreInteriorCountArray, cartOrderStoreInteriorIndexArray);
        }
    }

    [ClientRpc]
    private void BuyProductLogicClientRpc(int[] cartOrderStoreProductCountArray, int[] cartOrderStoreProductIndexArray)
    {
        Debug.Log("cartOrderStoreProductSOList.Count: " + cartOrderStoreProductCountArray.Length);

        cartOrderStoreProductSOList.Clear();
        cartOrderStoreProductCountList.Clear();
        storeTotalCost = 0;
        OnTotalCostChange?.Invoke(this, EventArgs.Empty);
        ResetProductTemplateUI();
    }
    private void ResetProductTemplateUI()
    {
        foreach (Transform child in contentStoreManagerUI)
        {
            if (child.TryGetComponent(out StoreProductTemplateUI template))
            {
                template.ResetCountWhenBuy();
            }
        }
    }

    [ClientRpc]
    private void BuyInteriorLogicClientRpc(int[] cartOrderStoreInteriorCountArray, int[] cartOrderStoreInteriorIndexArray)
    {
        Debug.Log("cartOrderStoreInteriorCountList.Count: " + cartOrderStoreInteriorCountList.Count);

        // Reset the list interior the cart 
        cartOrderStoreInteriorSOList.Clear();
        cartOrderStoreInteriorCountList.Clear();

        // Reset TotalCost back to 0
        storeTotalCost = 0;
        OnTotalCostChange?.Invoke(this, EventArgs.Empty);
        ResetInteriorTemplateUI();
    }

    private void ResetInteriorTemplateUI()
    {
        foreach (Transform child in StoreManagerUI.Instance.GetInteriorScrollViewContent())
        {
            if (child.TryGetComponent(out StoreInteriorTemplateUI template))
            {
                template.ResetCountWhenBuy();
            }
        }
    }

    public float GetTotalCost()
    {
        return storeTotalCost;
    }

    public List<StoreProductSO> GetDailyRandomStoreProductSOList()
    {
        return dailyRandomStoreProductSOList;
    }

    public StoreInteriorSO GetStoreInteriorSOByIndex(int index)
    {
        return storeInteriorArray[index];
    }

    public int GetStoreInteriorSOIndex(StoreInteriorSO storeInteriorSO)
    {
        for (int i = 0; i < storeInteriorArray.Length; i++)
        {
            if (storeInteriorArray[i] == storeInteriorSO)
            {
                return i;
            }
        }
        // Return -1 if not found
        return -1;
    }
}
