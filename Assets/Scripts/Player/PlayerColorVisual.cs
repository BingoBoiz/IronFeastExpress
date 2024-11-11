using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer headMeshRenderer;
    [SerializeField] private MeshRenderer bodyMeshRenderer;

    private Material playerMaterial;

    private void Awake()
    {
        playerMaterial = new Material(headMeshRenderer.material);
        headMeshRenderer.material = playerMaterial;
        bodyMeshRenderer.material = playerMaterial;
    }

    public void SetPlayerColor(Color color)
    {
        playerMaterial.color = color;
    }

}
