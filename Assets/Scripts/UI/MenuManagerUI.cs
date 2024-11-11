using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

    // NOTE: 
    // Currently mixing logic and UI

public class MenuManagerUI : MonoBehaviour
{
    public static MenuManagerUI Instance {  get; private set; }
    //public event EventHandler OnDiscoveryNewDish;

    //[SerializeField] private RecipeBookUI recipeBookUI;
    [SerializeField] private Transform menuDishContentTransform;
    [SerializeField] private Transform menuDishTemplateTransform;
    [SerializeField] private Transform recipeBookContent;
    [SerializeField] private Transform recipeTemplate;

    [SerializeField] private Button discoveryNewDishButton;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        Instance = this;
        discoveryNewDishButton.onClick.AddListener(() =>
        {
            RecipeBookManager.Instance.DiscoveryNewDishForTesting(recipeBookContent);
        });

        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        Hide();
        //OnDiscoveryNewDish += RecipeBookUI_OnDiscoveryNewDish;
        //TrainStopUI.Instance.OnClickMenu += PlayingGameUI_OnClickMenu;

        TrainManager.Instance.OnDoneSetUp += TrainManager_OnDoneSetUp;

        RecipeBookManager.Instance.OnUpdateRecipeBook += RecipeBook_OnUpdateRecipeBook;

        RecipeBookManager.Instance.SetAllTheRecipeTemplateIntoContent(recipeTemplate, recipeBookContent);
    }

    private void TrainManager_OnDoneSetUp(object sender, System.EventArgs e)
    {
        MenuBoard.Instance.OnInteractMenuBoard += MenuBoard_OnInteractMenuBoard;
    }

    private void MenuBoard_OnInteractMenuBoard(object sender, EventArgs e)
    {
        Show();
        TodayMenuManager.Instance.SetAllMenuDishTemplateToContent(menuDishTemplateTransform, menuDishContentTransform);
    }

    private void RecipeBook_OnUpdateRecipeBook(object sender, System.EventArgs e)
    {
        Debug.Log("RecipeBook_OnUpdateRecipeBook");
        RecipeBookManager.Instance.UpdateRecipeBook(recipeBookContent);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void AddTemplateToDishMenuList(FinishDishSO finishDishSO)
    {
        TodayMenuManager.Instance.AddTemplateToDishMenuList(finishDishSO, menuDishTemplateTransform, menuDishContentTransform);
    }

    public Transform GetTodayMenuContentTransform() { return menuDishContentTransform; } 
}
