using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TableOrderShownUI : MonoBehaviour
{
    [SerializeField] private CustomerTable customerTable;
    [SerializeField] private Transform awaitingFoodDeliveryIcon;

    private FinishDishSO dishShownSO;


    private void Start()
    {
        customerTable.OnCustomerOrderDish += CustomerTable_OnCustomerOrderDish;
    }

    private void CustomerTable_OnCustomerOrderDish(object sender, CustomerTable.OnCustomerOrderDishEventArgs e)
    {
        dishShownSO = e.customerChosenOrderDish;
        awaitingFoodDeliveryIcon.gameObject.GetComponent<Image>().sprite = dishShownSO.finishDishIcon;
    }

    private void OnDestroy()
    {
        customerTable.OnCustomerOrderDish -= CustomerTable_OnCustomerOrderDish;

    }
}
