using UnityEngine;
using Unity.Cinemachine;

// Gắn camera theo Player bằng tag để scene mới dùng được cả Player vừa tạo lẫn Player đi qua cổng.
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

    // Thử tìm lại khi camera chưa có mục tiêu, vì Player có thể xuất hiện sau camera.
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
