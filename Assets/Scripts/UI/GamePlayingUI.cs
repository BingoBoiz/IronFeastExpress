using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI daysCount;

    private void Start()
    {
        GameManager.Instance.OnDayIncrease += GameManager_OnDayIncrease;
        FinanceSystem.Instance.OnPlayerBalanceChanged += FinanceSystem_OnPlayerBalanceChanged;
    }

    private void FinanceSystem_OnPlayerBalanceChanged(object sender, FinanceSystem.OnPlayerBalanceChangedEventArgs e)
    {
        Debug.Log("FinanceSystem_OnPlayerBalanceChanged");
        moneyText.text = e.currentPlayerBalance.ToString("F2");
    }

    private void GameManager_OnDayIncrease(object sender, EventArgs e)
    {
        daysCount.text = "Days : " + GameManager.Instance.GetDays().ToString();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnDayIncrease -= GameManager_OnDayIncrease;
        FinanceSystem.Instance.OnPlayerBalanceChanged -= FinanceSystem_OnPlayerBalanceChanged;
    }
}
