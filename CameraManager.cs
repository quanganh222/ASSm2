using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("All Cameras")]
    public CinemachineCamera[] cameras;

    [Header("Starting Camera")]
    public CinemachineCamera initialCamera;

    private void Start()
    {
        if (initialCamera != null)
        {
            SwitchCamera(initialCamera);
        }
    }

    public void SwitchCamera(CinemachineCamera targetCamera)
    {
        if (targetCamera == null) return;

        foreach (CinemachineCamera cam in cameras)
        {
            if (cam != null)
            {
                cam.Priority = 0;
            }
        }

        targetCamera.Priority = 20;

        Debug.Log("Active Camera: " + targetCamera.name);
    }
}