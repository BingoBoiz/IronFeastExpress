using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse3D : MonoBehaviour
{
    public static Mouse3D Instance { get; private set; }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask mouseColliderLayerMask = new LayerMask();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one Mouse3D instance found!");
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask))
        {
            transform.position = raycastHit.point;
        }
    }

    public static Vector3 GetMouseWorldPosition()
    {
        if (Instance == null)
        {
            Debug.LogError("Mouse3D Instance is null!");
            return Vector3.zero;
        }

        return Instance.GetMouseWorldPosition_Instance();
    } 

    private Vector3 GetMouseWorldPosition_Instance()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask))
        {
            Debug.Log(raycastHit.point);
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

}
