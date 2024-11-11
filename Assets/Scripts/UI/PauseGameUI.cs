using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PauseGameUI : MonoBehaviour
{
    [SerializeField] private Button MainMenuButton;
    [SerializeField] private Button SettingButton;
    [SerializeField] private Button ContinueButton;

    [SerializeField] private Transform popupSetting;


    private bool isLocalPauseGame;

    private void Awake()
    {
        MainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenu);
        });
        SettingButton.onClick.AddListener(() =>
        {
            popupSetting.gameObject.SetActive(true);
        });
        ContinueButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ToggleLocalPauseGame();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnToggleLocalGamePause += GameManager_OnToggleLocalGamePause;
        Hide();
        isLocalPauseGame = false;
    }

    private void GameManager_OnToggleLocalGamePause(object sender, System.EventArgs e)
    {
        isLocalPauseGame = !isLocalPauseGame;
        if (isLocalPauseGame)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
