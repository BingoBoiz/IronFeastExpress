using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RecipeBookUI : MonoBehaviour
{
    public static RecipeBookUI Instance {  get; private set; }

    [SerializeField] private Transform recipeBookContentPageEven;
    [SerializeField] private Transform recipeBookContentPageOdd;
    [SerializeField] private Transform recipeBookTemplate1;
    [SerializeField] private Transform recipeBookTemplate2;
    [SerializeField] private Transform recipeBookTemplate3;
    [SerializeField] private Transform recipeBookTemplate4;
    [SerializeField] private Transform recipeIngredientContentPageEven;
    [SerializeField] private Transform recipeIngredientContentPageOdd;
    [SerializeField] private Transform recipeIngredientTemplate1;
    [SerializeField] private Transform recipeIngredientTemplate2;
    [SerializeField] private Transform recipeIngredientTemplate3;
    [SerializeField] private Transform recipeIngredientTemplate4;

    private FinishDishSO[] currentBookHoldDish = new FinishDishSO[4];

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Hide();

        TrainManager.Instance.OnDoneSetUp += TrainManager_OnDoneSetUp;
        RecipeBookManager.Instance.OnUpdateRecipeBook += RecipeBookManager_OnUpdateRecipeBook;
        RecipeBookManager.Instance.OnDiscoverNewDish += RecipeBookManager_OnDiscoverNewDish;
        UpdateBookVisual();

    }

    private void RecipeBookManager_OnDiscoverNewDish(object sender, EventArgs e)
    {
        UpdateBookVisual(); // Lazy way
    }

    private void RecipeBookManager_OnUpdateRecipeBook(object sender, EventArgs e)
    {
        UpdateBookVisual();
    }

    private void TrainManager_OnDoneSetUp(object sender, System.EventArgs e)
    {
        MenuBoard.Instance.OnInteractMenuBoard += MenuBoard_OnInteractMenuBoard;
    }
    private void MenuBoard_OnInteractMenuBoard(object sender, EventArgs e)
    {
        Show();
    }

    private void UpdateBookVisual()
    {
        currentBookHoldDish = RecipeBookManager.Instance.GetBookHoldDish();
        for (int i = 0; i < currentBookHoldDish.Length; i++)
        {
            switch (i)
            {
                case 0:
                    recipeBookTemplate1.GetComponent<RecipeBookTemplateUI>().UpdateRecipeBookTemplateVisual(currentBookHoldDish[i]);
                    recipeIngredientTemplate1.GetComponent<RecipeIngredientTemplateUI>().UpdateRecipeIngredientTemplateVisual(currentBookHoldDish[i]);
                    break;
                case 1:
                    recipeBookTemplate2.GetComponent<RecipeBookTemplateUI>().UpdateRecipeBookTemplateVisual(currentBookHoldDish[i]);
                    recipeIngredientTemplate2.GetComponent<RecipeIngredientTemplateUI>().UpdateRecipeIngredientTemplateVisual(currentBookHoldDish[i]);
                    break;
                case 2:
                    recipeBookTemplate3.GetComponent<RecipeBookTemplateUI>().UpdateRecipeBookTemplateVisual(currentBookHoldDish[i]);
                    recipeIngredientTemplate3.GetComponent<RecipeIngredientTemplateUI>().UpdateRecipeIngredientTemplateVisual(currentBookHoldDish[i]);
                    break;
                case 3:
                    recipeBookTemplate4.GetComponent<RecipeBookTemplateUI>().UpdateRecipeBookTemplateVisual(currentBookHoldDish[i]);
                    recipeIngredientTemplate4.GetComponent<RecipeIngredientTemplateUI>().UpdateRecipeIngredientTemplateVisual(currentBookHoldDish[i]);
                    break;
            }
        }
    }
    int count = 0;
    public RecipeBookTemplateUI GetRecipeBookTemplateUI(int index)
    {
        count++;
        Debug.Log("count: " + count);
        Debug.Log("index: " + index);
        switch (index)
        {
            case 1:
                return recipeBookTemplate1.GetComponent<RecipeBookTemplateUI>();
            case 2:
                return recipeBookTemplate2.GetComponent<RecipeBookTemplateUI>();
            case 3:
                return recipeBookTemplate3.GetComponent<RecipeBookTemplateUI>();
            case 4:
                return recipeBookTemplate4.GetComponent<RecipeBookTemplateUI>();
            default:
                Debug.LogError("This should not be reach");
                break;
        }
        return null;
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
        RecipeBookManager.Instance.OnUpdateRecipeBook -= RecipeBookManager_OnUpdateRecipeBook;
        RecipeBookManager.Instance.OnDiscoverNewDish -= RecipeBookManager_OnDiscoverNewDish;
    }
}
