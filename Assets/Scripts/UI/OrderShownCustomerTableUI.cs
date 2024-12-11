using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OrderShownCustomerTableUI : MonoBehaviour
{
    [SerializeField] private Image clockTimer;
    [SerializeField] private Image timerImage;
    [SerializeField] private Image dishIconImage;
    [SerializeField] private Image dishIconMaskImage;

    [SerializeField] private CustomerTable customerTable;

    // Temp
    [SerializeField] private TextMeshProUGUI stateText;
   
    private void Start()
    {
        customerTable.OnTimerValueChanged += Table_OnTimerValueChanged;
        customerTable.OnStateValueChanged += Table_OnStateValueChanged;
        customerTable.OnCustomerOrderDish += CustomerTable_OnCustomerOrderDish;
        timerImage.fillAmount = 0f;
        HideClock();
        HideDishIcon();
    }

    private void CustomerTable_OnCustomerOrderDish(object sender, CustomerTable.OnCustomerOrderDishEventArgs e)
    {
        //ShowDishIcon();
        dishIconImage.sprite = e.customerChosenOrderDish.finishDishIcon;
    }

    private void Table_OnStateValueChanged(object sender, CustomerTable.OnStateValueChangedEventArgs e)
    {
        stateText.text = e.state.ToString();
        if (!(e.state == CustomerTable.TableState.AwaitingFoodDelivery))
        {
            HideDishIcon();
        }
        else
        {
            ShowDishIcon();
        }
    }

    private void Table_OnTimerValueChanged(object sender, CustomerTable.OnTimerValueChangedEventArgs e)
    {
        timerImage.fillAmount = e.timerClockNormalized;
        if (e.timerClockNormalized == 0f || e.timerClockNormalized == 1f)
        {
            HideClock();
        }
        else
        {
            ShowClock();
        }
    }

    private void ShowClock()
    {
        clockTimer.gameObject.SetActive(true);
    }
    private void HideClock()
    {
        clockTimer.gameObject.SetActive(false);
    }
    private void ShowDishIcon()
    {
        dishIconMaskImage.gameObject.SetActive(true);
    }
    private void HideDishIcon()
    {
        dishIconMaskImage.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        customerTable.OnTimerValueChanged -= Table_OnTimerValueChanged;
        customerTable.OnStateValueChanged -= Table_OnStateValueChanged;
        customerTable.OnCustomerOrderDish -= CustomerTable_OnCustomerOrderDish;
    }
}
