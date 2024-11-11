using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI revenueNumber;
    [SerializeField] private TextMeshProUGUI storeCostNumber;
    [SerializeField] private TextMeshProUGUI fuelCostNumber;
    [SerializeField] private TextMeshProUGUI profitNumber;
    [SerializeField] private Transform satisfactionStarContent;
    [SerializeField] private Button nextDayButton;

    private void Start()
    {
        Hide();
        GameManager.Instance.OnStateChange += GameManager_OnStateChange;
        nextDayButton.onClick.AddListener(() =>
        {
            GameManager.Instance.SetTrainStopGameState();
            GameManager.Instance.NextDay();
        });
    }

    private void GameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsShowingResult() && !FinanceSystem.Instance.IsBankrupt())
        {
            Show();
            UpdateText();
        }
        else
        {
            Hide();
        }
    }

    private void UpdateText()
    {
        revenueNumber.text = FinanceSystem.Instance.GetCurrentRevenue().ToString();
        storeCostNumber.text = FinanceSystem.Instance.GetStoreExpenditure().ToString();
        fuelCostNumber.text = FinanceSystem.Instance.GetFuelExpenditure().ToString();
        profitNumber.text = FinanceSystem.Instance.GetCurrentProfit().ToString();
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
