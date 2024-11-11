using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private Transform inventoryBoxTemplate;

    private Transform parent;

    private void Awake()
    {
        //PlayingGameUI.Instance.OnClickStore += PlayingGameUI_OnClickStore;
        parent = transform.parent;
    }

    private void Start()
    {
        UpdateVisualInventoryUI();
    }
    
    private void UpdateVisualInventoryUI()
    {
        InventoryManager.Instance.SetAllTheInventoryBoxTemplateIntoContent(inventoryBoxTemplate, content);
    }

    
}
