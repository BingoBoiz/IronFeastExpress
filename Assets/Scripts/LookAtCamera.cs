using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private enum CameraMode
    {
        LookAt,
        LookAtInverted,
        CameraForward,
        CameraForwardInverted,
    }
    [SerializeField] private CameraMode mode = CameraMode.LookAtInverted;

    private void LateUpdate()
    {
        if (Camera.main == null) // Wait for initialize main camera in Player.cs
        {
            return; 
        }
        switch (mode)
        {
            case CameraMode.LookAt:
                transform.LookAt(Camera.main.transform);
                break;
            case CameraMode.LookAtInverted:
                Vector3 dirFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + dirFromCamera);
                break;
            case CameraMode.CameraForward:
                transform.forward = Camera.main.transform.forward;
                transform.LookAt(Camera.main.transform);
                break;
            case CameraMode.CameraForwardInverted:
                transform.forward = -Camera.main.transform.forward;
                break;
        }

    }
}
