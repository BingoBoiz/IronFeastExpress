using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBoardUI : MonoBehaviour
{
    public static MenuBoardUI Instance {  get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Hide();
        TrainManager.Instance.OnDoneSetUp += TrainManager_OnDoneSetUp;
    }

    private void TrainManager_OnDoneSetUp(object sender, System.EventArgs e)
    {
        MenuBoard.Instance.OnInteractMenuBoard += MenuBoard_OnInteractMenuBoard;
    }
    private void MenuBoard_OnInteractMenuBoard(object sender, EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        TrainManager.Instance.OnDoneSetUp -= TrainManager_OnDoneSetUp;

    }
}
