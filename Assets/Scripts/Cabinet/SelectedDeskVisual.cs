using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedDeskVisual : MonoBehaviour
{
    // Only the this script listen from the event that fire from Player script when visualing the counter being selected
    // Con: All of these selected counter visual are going to listen to the same event (May need to improve in the future if scaling)

    [SerializeField] private BaseDesk baseDesk;
    [SerializeField] private GameObject selectedClearDesk;

    private void Start() //Using start because it run after Awake function in Player script
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedDeskChanged += Player_OnSelectedDeskChanged;
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
            Player.LocalInstance.OnSelectedDeskChanged -= Player_OnSelectedDeskChanged;
            Player.LocalInstance.OnSelectedDeskChanged += Player_OnSelectedDeskChanged;
        }
    }

    private void Player_OnSelectedDeskChanged(object sender, Player.OnSelectedDeskChangedEventArgs e)
    {
        //Debug.Log("Player_OnSelectedCounterChanged: " + e.selectedCounter + "counter");
        if (e.selectedDesk == baseDesk && GameManager.Instance.IsTrainStop())
        {
            ShowSelectedVisual();
        }
        else HideSelectedVisual();
    }

    public void ShowSelectedVisual()
    {
        selectedClearDesk.SetActive(true);
    }
    public void HideSelectedVisual()
    {
        selectedClearDesk.SetActive(false);
    }
    public void OnDestroy()
    {
        Player.LocalInstance.OnSelectedDeskChanged -= Player_OnSelectedDeskChanged;
        Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
    }
}
