using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCabinetVisual : MonoBehaviour
{
    // Only the this script listen from the event that fire from Player script when visualing the counter being selected
    // Con: All of these selected counter visual are going to listen to the same event (May need to improve in the future if scaling)

    [SerializeField] private BaseCabinet baseCounter;
    [SerializeField] private GameObject selectedClearCounter;

    private void Start() //Using start because it run after Awake function in Player script
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCabinetChanged += Player_OnSelectedCounterChanged;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
    }

    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCabinetChanged -= Player_OnSelectedCounterChanged;
            Player.LocalInstance.OnSelectedCabinetChanged += Player_OnSelectedCounterChanged;
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCabinetChangedEventArgs e)
    {
        //Debug.Log("Player_OnSelectedCounterChanged: " + e.selectedCounter + "counter");
        if (e.selectedCabinet == baseCounter)
        {
            ShowSelectedVisual();
        }
        else HideSelectedVisual();
    }

    public void ShowSelectedVisual()
    {
        selectedClearCounter.SetActive(true);
    }
    public void HideSelectedVisual()
    {
        selectedClearCounter.SetActive(false);
    }
    public void OnDestroy()
    {
        Player.LocalInstance.OnSelectedCabinetChanged -= Player_OnSelectedCounterChanged;
    }
}
