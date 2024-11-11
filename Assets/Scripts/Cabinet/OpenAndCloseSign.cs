using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OpenAndCloseSign : BaseSign
{
    public static OpenAndCloseSign Instance { get; private set; }
    public event EventHandler OnCloseRestaurant;
    public event EventHandler OnBankRupt;

    [SerializeField] private Transform OpenSign;
    [SerializeField] private Transform CloseSign;

    private void Awake()
    {
        Instance = this;
    }

    public override void Interact(Player player)
    {
        if (GameManager.Instance.IsTrainRunning() && GameManager.Instance.IsTrainRunForAWhile())
        {
            SwitchCloseSign();
            if (!FinanceSystem.Instance.IsRunOutMoneyForFuel())
            {
                OnCloseRestaurant?.Invoke(this, EventArgs.Empty);
                GameManager.Instance.SetShowResultState();
            }
            else
            {
                TriggerBankRuptServerRpc();
            }
        }
        else if (GameManager.Instance.IsTrainStop())
        {
            
            SwitchOpenSign();
            GameManager.Instance.SetTrainRunningGameState();
        }
        else
        {
            Debug.LogWarning("This should not be reach");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerBankRuptServerRpc()
    {
        TriggerBankRuptClientRpc();
    }

    [ClientRpc]
    private void TriggerBankRuptClientRpc()
    {
        OnBankRupt?.Invoke(this, EventArgs.Empty);
    }

    private void SwitchOpenSign()
    {
        OpenSign.gameObject.SetActive(true);
        CloseSign.gameObject.SetActive(false);
    }
    private void SwitchCloseSign()
    {
        OpenSign.gameObject.SetActive(false);
        CloseSign.gameObject.SetActive(true);
    }
}
