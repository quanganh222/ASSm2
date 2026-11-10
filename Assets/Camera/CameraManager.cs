using UnityEngine;
using Unity.Cinemachine;

// Mỗi khu vực có camera Cinemachine riêng. Camera thật lấy khung hình từ camera ảo đang được ưu tiên.
public class CameraManager : MonoBehaviour
{
    [Header("All Cameras")]
    public CinemachineCamera[] cameras;

    [Header("Starting Camera")]
    public CinemachineCamera initialCamera;
    private Transform player;
    private bool initialized;

    private void Start()
    {
        ConfigureOutputCamera();
        if (initialCamera != null) SwitchCamera(initialCamera);
        InitializeForPlayer();
    }

    private void LateUpdate()
    {
        if (!initialized || player == null) InitializeForPlayer();
    }

    private void ConfigureOutputCamera()
    {
        Camera output = null;
        // Chỉ giữ Audio Listener trên camera xuất hình để tránh hai listener khi mở nhiều scene.
        var physical = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
        foreach (var camera in physical)
            if (camera.GetComponent<CinemachineBrain>() != null &&
                (output == null || camera.gameObject.scene == gameObject.scene))
                output = camera;
        if (output != null)
        {
            foreach (var camera in physical)
            {
                if (camera != output && camera.CompareTag("MainCamera")) camera.enabled = false;
                if (camera.TryGetComponent<AudioListener>(out var listener)) listener.enabled = camera == output;
            }
            output.enabled = true;
        }
    }

    private void InitializeForPlayer()
    {
        var actor = GameObject.FindGameObjectWithTag("Player");
        if (actor == null) return;
        player = actor.transform;

        // Chọn camera theo vùng giới hạn gần Player nhất để tránh giữ camera của map cũ.
        cameras = Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Exclude);
        CinemachineCamera nearest = null;
        float best = float.PositiveInfinity;
        foreach (var camera in cameras)
        {
            camera.Target.TrackingTarget = player;
            var confiner = camera.GetComponent<CinemachineConfiner2D>();
            Vector2 anchor = camera.transform.position;
            if (confiner != null && confiner.BoundingShape2D != null)
                anchor = confiner.BoundingShape2D.ClosestPoint(player.position);
            float distance = ((Vector2)player.position - anchor).sqrMagnitude;
            if (distance < best || (distance == best && camera == initialCamera))
            {
                best = distance;
                nearest = camera;
            }
        }
        if (nearest != null) SwitchCamera(nearest);
        else if (initialCamera != null) SwitchCamera(initialCamera);
        initialized = true;
    }

    public void SwitchCamera(CinemachineCamera targetCamera)
    {
        if (targetCamera == null) return;
        if (player != null) targetCamera.Target.TrackingTarget = player;

        if (cameras == null) cameras = new CinemachineCamera[0];
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
