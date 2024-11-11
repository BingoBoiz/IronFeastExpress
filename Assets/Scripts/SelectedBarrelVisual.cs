using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SelectedBarrelVisual : MonoBehaviour
{
    [SerializeField] private MonoBehaviour storeItemBehaviour;
    [SerializeField] private GameObject selectedBarel;

    private ISelectable storeItem;

    private void Start()
    {
        // Debug log to verify the actual type of storeItemBehaviour
        Debug.Log("Type of storeItemBehaviour: " + storeItemBehaviour.GetType());

        // Cast the MonoBehaviour to ISelectable
        storeItem = storeItemBehaviour.GetComponent<ISelectable>();
        if (storeItem == null)
        {
            Debug.LogError("Assigned storeItemBehaviour does not implement ISelectable.");
        }

        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedStoreItemChanged += Player_OnSelectedBarrelChanged;
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
            Player.LocalInstance.OnSelectedStoreItemChanged -= Player_OnSelectedBarrelChanged;
            Player.LocalInstance.OnSelectedStoreItemChanged += Player_OnSelectedBarrelChanged;
        }
    }

    private void Player_OnSelectedBarrelChanged(object sender, Player.OnSelectedStoreItemChangedEventAgrs e)
    {
        if (e.selectedStoreItem == storeItem)
        {
            ShowSelectedVisual();
        }
        else HideSelectedVisual();
    }

    public void ShowSelectedVisual()
    {
        selectedBarel.SetActive(true);
    }
    public void HideSelectedVisual()
    {
        selectedBarel.SetActive(false);
    }

    private void OnDestroy()
    {
        Player.LocalInstance.OnSelectedStoreItemChanged -= Player_OnSelectedBarrelChanged;
    }
}
