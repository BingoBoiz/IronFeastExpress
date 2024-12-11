using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// NOTE: BADCODE
// Currently mixing logic and UI and NetworkBehaviour

public class MenuManagerUI : NetworkBehaviour
{
    public static MenuManagerUI Instance {  get; private set; }
    //public event EventHandler OnDiscoveryNewDish;

    //[SerializeField] private RecipeBookUI recipeBookUI;
    [SerializeField] private Transform menuDishContentTransform;
    [SerializeField] private Transform menuDishContentDraggableTransform;
    [SerializeField] private Transform menuDishTemplateTransform;
    [SerializeField] private Transform menuDishTemplateDraggableTransform;
    [SerializeField] private Transform recipeBookContent;
    [SerializeField] private Transform recipeTemplate;

    [SerializeField] private Button discoveryNewDishButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private TextMeshProUGUI currentPage;
    [SerializeField] private TextMeshProUGUI maxPage;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button previousPageButton;

    private void Awake()
    {
        Instance = this;
        discoveryNewDishButton.onClick.AddListener(() =>
        {
            RecipeBookManager.Instance.DiscoveryNewDishForTesting(recipeBookContent);
        });

        nextPageButton.onClick.AddListener(() =>
        {
            RecipeBookManager.Instance.ChangeRecipeBookContentByPage(2);
            currentPage.text = RecipeBookManager.Instance.GetCurrentPage().ToString();
            CheckTurnOffPageButton();
        });
        previousPageButton.onClick.AddListener(() =>
        {
            RecipeBookManager.Instance.ChangeRecipeBookContentByPage(-2);
            currentPage.text = RecipeBookManager.Instance.GetCurrentPage().ToString();
            CheckTurnOffPageButton();
        });
        closeButton.onClick.AddListener(() =>
        {
            MenuBoardUI.Instance.Hide();
            RecipeBookUI.Instance.Hide();
            Hide();
        });
    }

    private void CheckTurnOffPageButton()
    {
        if (!RecipeBookManager.Instance.ValidatePageIndex(2))
        {
            nextPageButton.gameObject.SetActive(false);
            return;
        }
        if (!RecipeBookManager.Instance.ValidatePageIndex(-2))
        {
            previousPageButton.gameObject.SetActive(false);
            return;
        }
        nextPageButton.gameObject.SetActive(true);
        previousPageButton.gameObject.SetActive(true);
    }

    private void Start()
    {
        Hide();

        TrainManager.Instance.OnDoneSetUp += TrainManager_OnDoneSetUp;
        currentPage.text = RecipeBookManager.Instance.GetCurrentPage().ToString();
        maxPage.text = RecipeBookManager.Instance.GetBookMaxPage().ToString();
    }

    private void TrainManager_OnDoneSetUp(object sender, System.EventArgs e)
    {
        TodayMenuManager.Instance.SetAllMenuDishTemplateToContent(menuDishTemplateTransform, menuDishTemplateDraggableTransform, menuDishContentTransform, menuDishContentDraggableTransform);
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

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void AddTemplateToMenuBoardList(FinishDishSO finishDishSO) 
    {
        //TodayMenuManager.Instance.AddTemplateToMenuBoardList(finishDishSO, menuDishTemplateTransform, menuDishTemplateDraggableTransform, menuDishContentTransform, menuDishContentDraggableTransform);
        int dishIndex = RecipeBookManager.Instance.GetFinishDishSOIndex(finishDishSO);

        AddTemplateToMenuBoardListServerRpc(dishIndex);
    }
    [ServerRpc(RequireOwnership = false)]
    private void AddTemplateToMenuBoardListServerRpc(int dishIndex)
    {
        AddTemplateToMenuBoardListClientRpc(dishIndex);
    }
    
    [ClientRpc]
    private void AddTemplateToMenuBoardListClientRpc(int dishIndex)
    {
        FinishDishSO finishDishSO = RecipeBookManager.Instance.GetFinishDishSOByIndex(dishIndex);

        Transform addedDishDraggableTransform = Instantiate(menuDishTemplateDraggableTransform, menuDishContentDraggableTransform);
        addedDishDraggableTransform.GetComponent<DraggableMenuDishTemplateUI>().SetDishIndex(dishIndex);
        
        Transform addedDishTransform = Instantiate(menuDishTemplateTransform, menuDishContentTransform);
        addedDishTransform.GetComponent<MenuBoardDishTemplateUI>().UpdateMenuDishTemplateVisual(finishDishSO);

        TodayMenuManager.Instance.AddDishToMenuBoardList(finishDishSO);

    }

    public void RemoveTemplateToMenuBoardList(int dishIndex)
    {
        RemoveTemplateToMenuBoardListServerRpc(dishIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveTemplateToMenuBoardListServerRpc(int dishIndex)
    {
        RemoveTemplateToMenuBoardListClientRpc(dishIndex);
    }

    [ClientRpc]
    private void RemoveTemplateToMenuBoardListClientRpc(int dishIndex)
    {
        FinishDishSO finishDishSO = RecipeBookManager.Instance.GetFinishDishSOByIndex(dishIndex);
        int templateIndex = TodayMenuManager.Instance.GetIndexFromTempDishMenuListSO(finishDishSO);

        Destroy(menuDishContentDraggableTransform.GetChild(templateIndex).gameObject);
        Destroy(menuDishContentTransform.GetChild(templateIndex).gameObject);
        TodayMenuManager.Instance.RemoveDishFromMenuBoardList(dishIndex);
    }

    public override void OnDestroy()
    {
        TrainManager.Instance.OnDoneSetUp -= TrainManager_OnDoneSetUp;
        MenuBoard.Instance.OnInteractMenuBoard -= MenuBoard_OnInteractMenuBoard;
    }

    public Transform GetMenuDishContentDraggableTransform() { return menuDishContentDraggableTransform; }

    
}
