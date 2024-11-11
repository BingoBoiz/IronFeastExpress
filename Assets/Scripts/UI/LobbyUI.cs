using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button joincCodeButton;

    [SerializeField] private LobbyCreateUI lobbyCreateUI;
    [SerializeField] private TMP_InputField joinCodeIPF;
    [SerializeField] private TMP_InputField playerNameIPF;

    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;


    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            KitchenGameLobbyManager.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenu);

        });

        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });

        joinLobbyButton.onClick.AddListener(() =>
        {
            KitchenGameLobbyManager.Instance.QuickJoin();

        });

        joincCodeButton.onClick.AddListener(() =>
        {
            KitchenGameLobbyManager.Instance.JoinWithCode(joinCodeIPF.text);
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerNameIPF.text = KitchenGameMultiplayer.Instance.GetPlayerName();
        playerNameIPF.onValueChanged.AddListener((string newName) =>
        {
            KitchenGameMultiplayer.Instance.SetPlayerName(newName);
        });

        KitchenGameLobbyManager.Instance.OnLobbyListChanged += KitchenGameLobby_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void KitchenGameLobby_OnLobbyListChanged(object sender, KitchenGameLobbyManager.OnLobbyListChangeEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer) 
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyTemplateUI>().SetLobby(lobby);
        }
    }

    private void OnDestroy()
    {
        KitchenGameLobbyManager.Instance.OnLobbyListChanged -= KitchenGameLobby_OnLobbyListChanged;

    }
}
