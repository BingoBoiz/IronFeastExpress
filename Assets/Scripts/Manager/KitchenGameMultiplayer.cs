using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using System.Drawing;
using Unity.Services.Authentication;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public const int MAX_PLAYER_AMOUNT = 4;
    private const int MAX_PLAYER_NAME_CHARACTER = 50;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";

    public static KitchenGameMultiplayer Instance { get; private set; }

    public static bool playMultiplayer = true;

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;


    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    //[SerializeField] private List<UnityEngine.Color> playerColorList;
    [SerializeField] private List<GameObject> playerSelectedCharacterList;

    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "Player" + UnityEngine.Random.Range(100, 1000));
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void Start()
    {
        /*if (!playMultiplayer)
        {
            // Singleplayer
            StartHost();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }*/
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                //Disconnected
                playerDataNetworkList.RemoveAt(i);
            }
        }

    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            //colorId = GetFirstUnusedColorId(),
            characterModelIndex = GetFirstUnusedCharacterModel(),
        });
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);

    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        //Debug.Log("request.ClientNetworkId: " + request.ClientNetworkId);

        if (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
        {
            
            Debug.Log("LocalClientId: " + NetworkManager.Singleton.LocalClientId);
        }
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            response.Approved = false;
            response.Reason = "The game has already started";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            response.Approved = false;
            response.Reason = "There can only be maximum 4 players";
            return;
        }
        response.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectCallback;

        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        // Cannot modify playerName directly so create a copy and modify that copy
        PlayerData copyPlayerData = playerDataNetworkList[playerDataIndex];

        copyPlayerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = copyPlayerData;
    }


    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        // Cannot modify playerId directly so create a copy and modify that copy
        PlayerData copyPlayerData = playerDataNetworkList[playerDataIndex];

        copyPlayerData.playerId = playerId;

        playerDataNetworkList[playerDataIndex] = copyPlayerData;
    }

    public void KickPlayerByClientId(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);

        // For some reason this OnClientDisconnectCallback is not trigger when DisconnectClient so we have to trigger it manually
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }

    public PlayerData GetClientDataByPlayerIndex(int playerIdex)
    {
        return playerDataNetworkList[playerIdex];
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        Debug.Log("Index is "+ GetKitchenObjectSOIndex(kitchenObjectSO));

        SpawnKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOByIndex(kitchenObjectSOIndex);

        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        // Make sure the code win run even if the client is lagging
        if (kitchenObjectParent.HasKitchenObject()) { return; }

        // Create that prefabs copy to the world
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefabs);

        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        Debug.Log("Server is about to spawn: " + kitchenObject.name);
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    public KitchenObjectSO GetKitchenObjectSOByIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }

    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        if (kitchenObjectNetworkObject == null)
        {
            return;
        }
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        ClearKitchenObjectOnParentClientRpc(kitchenObjectNetworkObjectReference);

        kitchenObject.DestroySelf();

    }

    [ClientRpc]
    private void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        kitchenObject.ClearKitchenObjectparent();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public bool IsColorSelected()
    {
        return true;
    }

    public int GetPlayerIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return default;
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    public PlayerData GetPlayerData()
    {
        // Use 'NetworkManager.Singleton.LocalClientId' to find your own player object https://docs-multiplayer.unity3d.com/netcode/current/basics/networkobject/
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    /*public UnityEngine.Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }*/

    public GameObject GetPlayerCharacter(int characterModelIndex)
    {
        return playerSelectedCharacterList[characterModelIndex];
    }

    /*public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorId))
        {
            // Color not available 
            return;
        }
        int playerDataIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        // Cannot modify colorId directly so create a copy and modify that copy
        PlayerData copyPlayerData = playerDataNetworkList[playerDataIndex];

        copyPlayerData.colorId = colorId;

        playerDataNetworkList[playerDataIndex] = copyPlayerData;
    }*/

    public void ChangePlayerCharacter(int characterModelIndex)
    {
        ChangePlayerCharacterServerRpc(characterModelIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerCharacterServerRpc(int characterModelIndex, ServerRpcParams serverRpcParams = default)
    {
        if (!IsCharacterAvailable(characterModelIndex))
        {
            // Character not available 
            return;
        }
        int playerDataIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        // Cannot modify characterModelIndex directly so create a copy and modify that copy
        PlayerData copyPlayerData = playerDataNetworkList[playerDataIndex];

        copyPlayerData.characterModelIndex = characterModelIndex;

        playerDataNetworkList[playerDataIndex] = copyPlayerData;
    }

    /*private bool IsColorAvailable(int colorId)
    {
        foreach (PlayerData player in playerDataNetworkList)
        {
            if (player.colorId == colorId)
            {
                return false;
            }
        }
        return true;
    }*/

    private bool IsCharacterAvailable(int characterModelIndex)
    {
        foreach (PlayerData player in playerDataNetworkList)
        {
            if (player.characterModelIndex == characterModelIndex)
            {
                return false;
            }
        }
        return true;
    }

    /*private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < playerColorList.Count; i++)
        {
            // Cycle through the playerColorList
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return 0;
    }*/

    private int GetFirstUnusedCharacterModel()
    {
        for (int i = 0; i < playerSelectedCharacterList.Count; i++)
        {
            // Cycle through the playerColorList
            if (IsCharacterAvailable(i))
            {
                return i;
            }
        }
        return 0;
    }

}
