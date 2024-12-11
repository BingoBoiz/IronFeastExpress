using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LoseUI : MonoBehaviour
{
    [SerializeField] private Button reTryButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI daysSurvivalCount;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            NetworkManager.Singleton.Shutdown();
            Loader.LoadNetwork(Loader.Scene.MainMenu);
        });
    }

    private void Start()
    {
        Hide();
        //GameManager.Instance.OnStateChange += GameManager_OnStateChange;
        OpenAndCloseSign.Instance.OnBankRupt += OpenCloseSign_OnBankRupt;
    }

    private void OpenCloseSign_OnBankRupt(object sender, System.EventArgs e)
    {
        Show();
    }

    private void GameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsShowingResult())
        {
            if (FinanceSystem.Instance.IsBankrupt()) 
            {
                Show();
            }
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        daysSurvivalCount.text = GameManager.Instance.GetDays().ToString();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        OpenAndCloseSign.Instance.OnBankRupt -= OpenCloseSign_OnBankRupt;
    }
}
