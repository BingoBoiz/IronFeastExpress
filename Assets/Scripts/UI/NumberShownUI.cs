using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NumberShownUI : MonoBehaviour
{
    [SerializeField] private GameObject hasAmountGameObject;
    [SerializeField] private TextMeshProUGUI numberCount;

    private IContainAmount containAmount;

    private void Awake()
    {
        numberCount.text = "0";
        containAmount = hasAmountGameObject.GetComponent<IContainAmount>();
        if (containAmount == null)
        {
            Debug.LogError("Game Object: " + hasAmountGameObject + "does not have a component that implements IContainAmount!");
        }
        containAmount.OnAmountChanged += ContainAmount_OnAmountChanged;
    }

  /*  public override void OnNetworkSpawn()
    {
        
    }*/

    private void ContainAmount_OnAmountChanged(object sender, IContainAmount.OnAmountChangedEventArgs e)
    {
        numberCount.text = e.amount.ToString();
    }
}
