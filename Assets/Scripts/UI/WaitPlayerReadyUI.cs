using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitPlayerReadyUI : MonoBehaviour
{
    private void Awake()
    {
        Show();
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        Hide();
        //GameManager.Instance.
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
