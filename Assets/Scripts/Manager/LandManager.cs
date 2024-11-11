using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LandManager : NetworkBehaviour
{
    public static LandManager Instance { get; private set; }
    [SerializeField] private List<LandSO> landSOList;

    private LandSO todayLandSO;
    private List<CustomerSO> todayCustomerSOList;
    private List<LandSO> landSOListTemp;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        landSOListTemp = new List<LandSO>(landSOList);
        GetNewLandSOServerRpc();
        GameManager.Instance.OnStateChange += GameManager_OnStateChange;
    }

    private void GameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsTrainStop())
        {
        }
        else if (GameManager.Instance.IsShowingResult())
        {
            GetNewLandSOServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetNewLandSOServerRpc()
    {
        if (landSOListTemp.Count == 0)
        {
            landSOListTemp = new List<LandSO>(landSOList);
        }

        int randomIndex = Random.Range(0, landSOListTemp.Count);
        todayLandSO = landSOListTemp[randomIndex];

        int index = GetNewLandSOIndex(todayLandSO);
        GetNewLandSOClientRpc(index);

        landSOListTemp.RemoveAt(randomIndex);
    }

    [ClientRpc]
    private void GetNewLandSOClientRpc(int index)
    {
        todayLandSO = GetLandSOByIndex(index);
        RefreshCustomerList();
    }

    private void RefreshCustomerList()
    {
        if (todayLandSO != null)
        {
            todayCustomerSOList = new List<CustomerSO>(todayLandSO.customers);
        }
        else
        {
            Debug.LogWarning("todayLandSO is null. Customer list cannot be refreshed.");
        }
    }

    private LandSO GetLandSOByIndex(int index)
    {
        return landSOList[index];
    }

    private int GetNewLandSOIndex(LandSO landSO)
    {
        return landSOList.IndexOf(landSO);
    }

    public CustomerSO GetRandomCustomerSO() 
    {
        Debug.Log("GetRandomCustomerSO: " + todayCustomerSOList.Count);
        return todayCustomerSOList[Random.Range(0, todayCustomerSOList.Count)];
    }

    public LandSO GetTodayLandSO()
    {
        return todayLandSO;
    }


}
