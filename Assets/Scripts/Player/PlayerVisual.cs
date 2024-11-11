using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private List<GameObject> characterModels; // List of character model GameObjects

    private void Awake()
    {
        // Ensure all models are disabled initially
        SetCharacterModelActive(-1);
    }

    public void SetPlayerCharacter(int modelIndex)
    {
        // Toggle visibility of each model based on the index
        SetCharacterModelActive(modelIndex);
    }

    private void SetCharacterModelActive(int modelIndex)
    {
        for (int i = 0; i < characterModels.Count; i++)
        {
            characterModels[i].SetActive(i == modelIndex);
        }
    }

}
