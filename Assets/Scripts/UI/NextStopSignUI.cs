using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NextStopSignUI : MonoBehaviour
{
    //[SerializeField] private GameObject nextStopSignModel;
    [SerializeField] private TextMeshProUGUI trainStopLocation;

    private void Start()
    {
        Hide();
        GameManager.Instance.OnStateChange += GameManager_OnStateChange;
        TrainManager.Instance.OnDoneSetUp += TrainManager_OnDoneSetUp;
    }

    private void TrainManager_OnDoneSetUp(object sender, System.EventArgs e)
    {
        MenuBoard.Instance.OnInteractMenuBoard += MenuBoard_OnInteractMenuBoard;
    }
    private void MenuBoard_OnInteractMenuBoard(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsTrainStop())
        {
            Show();
            UpdateLocationText();
        }
        else
        {
            Hide();
        }
    }

    
    private void UpdateLocationText()
    {
        if (LandManager.Instance == null)
        {
            Debug.LogWarning("LandManager.Instance == null");
        }
        else if (LandManager.Instance.GetTodayLandSO() == null)
        {
            Debug.LogWarning("LandManager.Instance.GetTodayLandSO() == null");
        }

        trainStopLocation.text = "STOPPING AT " + LandManager.Instance.GetTodayLandSO().landName;
        //Debug.Log("UpdateLocationText: " + trainStopLocation.text);
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
        TrainManager.Instance.OnDoneSetUp -= TrainManager_OnDoneSetUp;
        MenuBoard.Instance.OnInteractMenuBoard -= MenuBoard_OnInteractMenuBoard;
    }
}
