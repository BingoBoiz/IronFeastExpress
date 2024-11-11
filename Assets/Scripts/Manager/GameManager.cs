using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {  get; private set; }

    public event EventHandler OnToggleLocalGamePause;
    public event EventHandler OnStateChange;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnDayIncrease;

    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnPaused;

    private enum GameState
    {
        WaitingForPlayersToReady,
        TrainStop, //Train stop for player to read article, buy ingredient and change food menu...
        TrainRunning, // Wait for customer to order, cook and deliver dishes
        ShowResult, // Show revenue, cost and customer satisfaction while wait for player to hit next day
    }

    [SerializeField] private Transform playerPrefabs;

    private NetworkVariable<GameState> state = new NetworkVariable<GameState>(GameState.WaitingForPlayersToReady);
    private NetworkVariable<float> trainRunningTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDictionary;

    private float trainRunningTimeMax = 300f;
    private bool isLocalPlayerReady;
    private bool isLocalGamePaused = false;
    private bool autoTestGamePauseState;


    private int days;

    public void Awake()
    {
        Instance = this;
        days = 1;
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();

    }
    private void Start()
    {
        GameInput.Instance.OnLocalPauseAction += GameInput_OnLocalPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefabs);

            // Manually spawn an object as PlayerObject
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        autoTestGamePauseState = true;
        // This will trigger the LateUpdate()
    }

    private void State_OnValueChanged(GameState previousValue, GameState newValue)
    {
        OnStateChange?.Invoke(this, EventArgs.Empty);
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        Debug.Log("isGamePaused boolean has changed from: " + !isGamePaused.Value + " to " + isGamePaused.Value);
        if (isGamePaused.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnPaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        switch (state.Value)
        {
            case GameState.WaitingForPlayersToReady:
                break;
            case GameState.TrainStop:
                break;
            case GameState.TrainRunning:
                trainRunningTimer.Value -= Time.deltaTime;
                if (trainRunningTimer.Value < 0f)
                {
                    FinanceSystem.Instance.ProcessFuelPurchase();
                    state.Value = GameState.ShowResult;
                }
                break;
            case GameState.ShowResult:
                break;
        }
        //Debug.Log(state);
    }

    private void LateUpdate()
    {
        //Handling when player are pausing and disconect after
        if (autoTestGamePauseState)
        {
            autoTestGamePauseState = false;
            CheckGamePausedState();
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == GameState.WaitingForPlayersToReady)
        {
            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
            //OnStateChange?.Invoke(this, EventArgs.Empty);
            //trainStopTimer = trainStopTimeMax;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default) //ServerRpcParams is a built-in type part or network for game object
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;


        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) //Dictionary not contain the key or the player is not ready
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            state.Value = GameState.TrainStop;
        }
    }

    private void GameInput_OnLocalPauseAction(object sender, EventArgs e)
    {
        // Player can pause game
        ToggleLocalPauseGame();
    }

    public void ToggleLocalPauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;
        OnToggleLocalGamePause?.Invoke(this, EventArgs.Empty);
        if (isLocalGamePaused)
        {
            //Time.timeScale = 0f;
            PauseGameServerRpc();
        }
        else
        {
            //Time.timeScale = 1f;
            UnPauseGameServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;

        CheckGamePausedState();

    }

    [ServerRpc(RequireOwnership = false)]
    private void UnPauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;

        CheckGamePausedState();
    }

    private void CheckGamePausedState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId])
            {
                // This player is paused
                isGamePaused.Value = true;
                return;
            }
        }

        // All players are unpaused
        isGamePaused.Value = false;
    }

    public bool IsTrainRunForAWhile()
    {
        return trainRunningTimer.Value > trainRunningTimeMax * 0.2;
    }
    
    public bool IsTrainRunning()
    {
        return state.Value == GameState.TrainRunning;
    }

    public bool IsTrainStop()
    {
        return state.Value == GameState.TrainStop;
    }

    public bool IsShowingResult() 
    {
        return state.Value == GameState.ShowResult;
    }

    public bool IsWaitingForPlayersToReady()
    {
        return state.Value == GameState.WaitingForPlayersToReady;
    }

    public bool IsGamePause()
    {
        return isLocalGamePaused;
    }

    public void SetTrainStopGameState()
    {
        SetTrainStopGameStateServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetTrainStopGameStateServerRpc()
    {
        state.Value = GameState.TrainStop;
    }

    public void SetShowResultState()
    {
        SetShowResultStateServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetShowResultStateServerRpc()
    {
        state.Value = GameState.ShowResult;
    }

    public void SetTrainRunningGameState()
    {
        SetTrainRunningGameStateServerRpc();

        PlayerPlacingCabinet.LocalInstance.SetIsHoldingCabinet(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetTrainRunningGameStateServerRpc()
    {
        state.Value = GameState.TrainRunning;
        trainRunningTimer.Value = trainRunningTimeMax;
    }

    public int GetDays() { return days; }

    public void NextDay()
    {
        NextDayServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void NextDayServerRpc()
    {
        NextDayClientRpc();
    }

    [ClientRpc]
    private void NextDayClientRpc()
    {
        days++;
        OnDayIncrease?.Invoke(this, EventArgs.Empty);
    }
}
