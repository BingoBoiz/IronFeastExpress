using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreUI : MonoBehaviour
{
    public static StoreUI Instance {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Hide();
        StoreDesk.OnInteractStore += StoreDesk_OnInteractStore;
    }

    private void StoreDesk_OnInteractStore(object sender, System.EventArgs e)
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
        StoreDesk.OnInteractStore -= StoreDesk_OnInteractStore;
    }
}
