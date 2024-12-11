using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TodayMenuManager : MonoBehaviour
{
    public static TodayMenuManager Instance {  get; private set; }

    [SerializeField] private FinishDishListSO currentDishMenuListSO;
    private List<FinishDishSO> tempDishMenuListSO;

    // NETCODE RPC CANNOT HAVE TRANFROM PARAMETER SO GET FROM MENUMANAGERUI
    private Transform menuDishTemplate;
    private Transform menuDishDraggableTemplate;
    private Transform menuDishContent;
    private Transform menuDishDraggableContent;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        tempDishMenuListSO = new(currentDishMenuListSO.finishDishSOList);
    }

    public void SetAllMenuDishTemplateToContent(Transform menuDishTemplate, Transform menuDishDraggableTemplate, Transform menuDishContent, Transform menuDishDraggableContent)
    {
        foreach (Transform child in menuDishContent)
        {
            Debug.Log("Check if the client is destroyng child");
            Destroy(child.gameObject);
        }

        //Cycle through the tempDishMenuListSO
        foreach (FinishDishSO menuDishSO in tempDishMenuListSO)
        {
            //Debug.Log("Dish Index:" + tempDishMenuListSO.finishDishSOList.IndexOf(menuDishSO));
            int dishIndex = RecipeBookManager.Instance.GetFinishDishSOIndex(menuDishSO);
            // Make a copy of menuDishTemplate inside content
            Transform finishDishTransform = Instantiate(menuDishTemplate, menuDishContent);
            Transform addedDishDraggableTransform = Instantiate(menuDishDraggableTemplate, menuDishDraggableContent);

            // Update the visual of the RecipeTemplateUI through an finishDishSO of that copy 'recipeTemplate'
            finishDishTransform.GetComponent<MenuBoardDishTemplateUI>().UpdateMenuDishTemplateVisual(menuDishSO);
            addedDishDraggableTransform.GetComponent<DraggableMenuDishTemplateUI>().SetDishIndex(dishIndex);
        }
    }

    public void AddDishToMenuBoardList(FinishDishSO finishDishSO)
    {
        tempDishMenuListSO.Add(finishDishSO);
    }

    public void RemoveDishFromMenuBoardList(int dishIndex)
    {
        FinishDishSO finishDishSO = RecipeBookManager.Instance.GetFinishDishSOByIndex(dishIndex);
        tempDishMenuListSO.Remove(finishDishSO);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveTemplateToDishMenuListServerRpc(int dishIndex)
    {
        RemoveTemplateToDishMenuListClientRpc(dishIndex);
    }

    [ClientRpc]
    private void RemoveTemplateToDishMenuListClientRpc(int dishIndex)
    {
        FinishDishSO dishSO = RecipeBookManager.Instance.GetFinishDishSOByIndex(dishIndex);
        tempDishMenuListSO.Remove(dishSO);
    }

    public FinishDishListSO GetTodayMenuDishList() { return currentDishMenuListSO; }
    public int GetIndexFromTempDishMenuListSO(FinishDishSO dishSO)
    {
        return tempDishMenuListSO.IndexOf(dishSO);
    }
}
