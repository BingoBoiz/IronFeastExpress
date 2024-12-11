using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerPauseWaitUI : MonoBehaviour
{

    private void Start()
    {
        GameManager.Instance.OnMultiplayerGamePaused += GameManager_OnMultiplayerGamePaused;
        GameManager.Instance.OnMultiplayerGameUnPaused += GameManager_OnMultiplayerGameUnPaused;
        Hide();
    }

    private void GameManager_OnMultiplayerGameUnPaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnMultiplayerGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnMultiplayerGamePaused -= GameManager_OnMultiplayerGamePaused;
        GameManager.Instance.OnMultiplayerGameUnPaused -= GameManager_OnMultiplayerGameUnPaused;
    }
}
