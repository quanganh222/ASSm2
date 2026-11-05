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
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            cineCamera.Target.TrackingTarget = player.transform;
        }
        else
        {
            Debug.LogError("Player not found!");
        }
    }
}
