using UnityEngine;
using Unity.Cinemachine;

public class CameraTargetFinder : MonoBehaviour
{
    private CinemachineCamera cineCamera;

    private void Awake()
    {
        cineCamera = GetComponent<CinemachineCamera>();
    }

    private void Start()
    {
        BindPlayer();
    }

    private void LateUpdate()
    {
        if (cineCamera != null && cineCamera.Target.TrackingTarget == null) BindPlayer();
    }

    private void BindPlayer()
    {
        if (cineCamera == null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            cineCamera.Target.TrackingTarget = player.transform;
        }
    }
}
