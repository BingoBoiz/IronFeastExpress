using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TrainStopUI : MonoBehaviour
{
    public static TrainStopUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Hide();
        GameManager.Instance.OnStateChange += GameManager_OnStateChange;
    }

    private void GameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsTrainStop())
        {
            Show();
            
        }
        if (!GameManager.Instance.IsTrainStop())
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

    private void OnDestroy()
    {
        GameManager.Instance.OnStateChange -= GameManager_OnStateChange;

    }
}
