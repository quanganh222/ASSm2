using UnityEngine;
using Unity.Cinemachine;

public class MapCameraTrigger : MonoBehaviour
{
    public CameraManager cameraManager;

    public CinemachineCamera targetCamera;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (cameraManager == null)
            {
                Debug.LogError("CameraManager is missing!");
                return;
            }

            if (targetCamera == null)
            {
                Debug.LogError("TargetCamera is missing!");
                return;
            }

            cameraManager.SwitchCamera(targetCamera);
        }
    }
}
