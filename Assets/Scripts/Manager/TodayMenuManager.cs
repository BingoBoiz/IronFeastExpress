using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TodayMenuManager : NetworkBehaviour
{
    public static TodayMenuManager Instance {  get; private set; }


    [SerializeField] private FinishDishListSO currentDishMenuListSO;

    private List<FinishDishSO> tempDishMenuListSO;

    private void Awake()
    {
        Instance = this;
    }
    public override void OnNetworkSpawn()
    {
        tempDishMenuListSO = new(currentDishMenuListSO.finishDishSOList);
    }

    public void SetAllMenuDishTemplateToContent(Transform menuDishTemplate, Transform menuDishContent)
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

            // Make a copy of menuDishTemplate inside content
            Transform finishDishTransform = Instantiate(menuDishTemplate, menuDishContent);

            // Update the visual of the RecipeTemplateUI through an finishDishSO of that copy 'recipeTemplate'
            finishDishTransform.GetComponent<MenuDishTemplateUI>().UpdateMenuDishTemplateVisual(menuDishSO);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateMenuDishUIServerRpc()
    {
        Debug.Log("");
        UpdateMenuDishUIClientRpc();
    }

    [ClientRpc]
    private void UpdateMenuDishUIClientRpc()
    {

    }

    public void AddTemplateToDishMenuList(FinishDishSO finishDishSO, Transform menuDishTemplate, Transform menuDishContent)
    {
        int dishIndex = RecipeBookManager.Instance.GetFinishDishSOIndex(finishDishSO);
        AddTemplateToDishMenuListServerRpc(dishIndex);
        Transform addedDishTransform = Instantiate(menuDishTemplate, menuDishContent);
        addedDishTransform.GetComponent<MenuDishTemplateUI>().UpdateMenuDishTemplateVisual(finishDishSO);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddTemplateToDishMenuListServerRpc(int dishIndex)
    {
        AddTemplateToDishMenuListClientRpc(dishIndex);
    }

    [ClientRpc]
    private void AddTemplateToDishMenuListClientRpc(int dishIndex)
    {
        FinishDishSO finishDishSO = RecipeBookManager.Instance.GetFinishDishSOByIndex(dishIndex);
        tempDishMenuListSO.Add(finishDishSO);
    }

    public FinishDishListSO GetTodayMenuDishList() { return currentDishMenuListSO; }
}
