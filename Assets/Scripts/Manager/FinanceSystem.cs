using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FinanceSystem : NetworkBehaviour
{
    public static FinanceSystem Instance { get; private set; }
    public event EventHandler<OnPlayerBalanceChangedEventArgs> OnPlayerBalanceChanged;
    public class OnPlayerBalanceChangedEventArgs : EventArgs
    {
        public float currentPlayerBalance;
    }

    private NetworkVariable<float> playerBalance = new NetworkVariable<float>(400);
    private NetworkVariable<float> currentRevenue = new NetworkVariable<float>(0);
    private NetworkVariable<float> storeExpenditure = new NetworkVariable<float>(0);
    private NetworkVariable<float> fuelExpenditure = new NetworkVariable<float>(0);
    private NetworkVariable<float> currentProfit = new NetworkVariable<float>(0);

    private float dailyFuelRateIncrease = 0.05f;
    private float trainWagonFuelCost = 16;
    private int trainWagonCount = 2; // When start there is only 2 wagon

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnStateChange += GameManager_OnStateChange;
        TrainManager.Instance.OnAddingNewWagon += TrainManager_OnAddingNewWagon;
        OpenAndCloseSign.Instance.OnCloseRestaurant += OpenCloseSign_OnCloseRestaurant;
        playerBalance.OnValueChanged += PlayerBalanceChanged;
        
        OnPlayerBalanceChanged?.Invoke(this, new OnPlayerBalanceChangedEventArgs
        {
            currentPlayerBalance = playerBalance.Value,
        });
    }
    private void PlayerBalanceChanged(float previousValue, float newValue)
    {
        OnPlayerBalanceChanged?.Invoke(this, new OnPlayerBalanceChangedEventArgs
        {
            currentPlayerBalance = newValue,
        });
    }

    private void OpenCloseSign_OnCloseRestaurant(object sender, System.EventArgs e)
    {
        ProcessFuelPurchase();
    }

    private void TrainManager_OnAddingNewWagon(object sender, System.EventArgs e)
    {
        trainWagonCount++;
    }

    private void GameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsTrainStop())
        {
            Debug.Log("GameManager.Instance.IsTrainStop()");
            ResetResultVaribleServerRpc();
        }
        if (GameManager.Instance.IsTrainRunning())
        {
            Debug.Log("GameManager.Instance.IsTrainRunning()");
            CalculateFuelExpenditureServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetResultVaribleServerRpc()
    {
        currentRevenue.Value = 0;
        storeExpenditure.Value = 0;
        currentProfit.Value = 0;
        fuelExpenditure.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    private void CalculateFuelExpenditureServerRpc()
    {
        fuelExpenditure.Value = GameManager.Instance.GetDays() * dailyFuelRateIncrease + trainWagonCount * trainWagonFuelCost;
        dailyFuelRateIncrease = dailyFuelRateIncrease * (1 + dailyFuelRateIncrease);
        CalculateCurrentProfitServerRpc(-fuelExpenditure.Value);
    }

    public void AddingToRevenue(float amount)
    {
        AddingToRevenueServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddingToRevenueServerRpc(float amount)
    {
        currentRevenue.Value += amount;
        CalculateCurrentProfitServerRpc(amount);
        AdjustBalanceServerRpc(amount);
    }

    public void ProcessFuelPurchase()
    {
        AdjustBalanceServerRpc(-fuelExpenditure.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AdjustBalanceServerRpc(float amount)
    {
        playerBalance.Value += amount;
    }

    [ServerRpc(RequireOwnership = false)]
    private void CalculateCurrentProfitServerRpc(float amount)
    {
        currentProfit.Value += amount;
    }

    public void ProcessStorePurchase(float amount)
    {
        if (playerBalance == null)
        {
            Debug.LogWarning("playerBalance NetworkVariable is not initialized.");
            return;
        }
        AdjustBalanceServerRpc(-amount);
        storeExpenditure.Value += amount;
        CalculateCurrentProfitServerRpc(-amount);
    }

    public bool CheckPlayerBalanceBeforePurchase(float cost)
    {
        return playerBalance.Value < cost;
    }

    public bool IsRunOutMoneyForFuel()
    {
        return playerBalance.Value < fuelExpenditure.Value;
    }

    public bool IsBankrupt()
    {
        return playerBalance.Value < 0;
    }
    public float GetCurrentRevenue()
    {
        return currentRevenue.Value;
    }
    /*public float GetPlayerBalance()
    {
        return playerBalance.Value;
    }*/
    public float GetStoreExpenditure()
    {
        return storeExpenditure.Value;
    }
    public float GetFuelExpenditure()
    {
        return fuelExpenditure.Value;
    }
    public float GetCurrentProfit()
    {
        return currentProfit.Value;
    }


}
