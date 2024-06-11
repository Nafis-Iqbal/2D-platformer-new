using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraZoomOverrideScript : MonoBehaviour
{

    public CinemachineVirtualCamera regionVirtualCamera;

    void Start()
    {
        regionVirtualCamera.m_Follow = transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!CameraController.Instance.cameraZoomOverrideMode)
            {
                CameraController.Instance.CameraSystemOverrideForLevelSection(regionVirtualCamera);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (CameraController.Instance.cameraZoomOverrideMode)
            {
                CameraController.Instance.DisableCameraOverrideMode();
            }
        }
    }
}
