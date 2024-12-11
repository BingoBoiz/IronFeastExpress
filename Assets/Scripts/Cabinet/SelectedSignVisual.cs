using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedSignVisual : MonoBehaviour
{
    // Only the this script listen from the event that fire from Player script when visualing the counter being selected
    // Con: All of these selected counter visual are going to listen to the same event (May need to improve in the future if scaling)

    [SerializeField] private BaseSign baseSign; // Temp
    [SerializeField] private GameObject selectedClearSign;

    private void Start() 
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedSignChanged += Player_OnSelectedSignChanged;
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
            Player.LocalInstance.OnSelectedSignChanged -= Player_OnSelectedSignChanged;
            Player.LocalInstance.OnSelectedSignChanged += Player_OnSelectedSignChanged;
        }
    }

    private void Player_OnSelectedSignChanged(object sender, Player.OnSelectedSignChangedEventArgs e)
    {
        if (GameManager.Instance.IsTrainStop() || GameManager.Instance.IsTrainRunning())
        {
            if (e.selectedSign == baseSign)
            {
                ShowSelectedVisual();
            }
            else HideSelectedVisual();
        }
        else HideSelectedVisual();
    }

    public void ShowSelectedVisual()
    {
        selectedClearSign.SetActive(true);
    }
    public void HideSelectedVisual()
    {
        selectedClearSign.SetActive(false);
    }

    private void OnDestroy()
    {
        Player.LocalInstance.OnSelectedSignChanged -= Player_OnSelectedSignChanged;
        Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
    }
}
