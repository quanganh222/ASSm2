using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Metroidvania.Gameplay;

// Quản lý màn hình chờ và chuyển map, đồng thời giữ Player cùng HUD để mang trạng thái sang scene mới.
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Loading Screen")]
    public CanvasGroup loadingScreen;
    public Slider progressBar;

    [Header("Transition Settings")]
    public float fadeDuration = 0.5f;

    private bool isTransitioning = false;
    private readonly HashSet<GameObject> persistentHud = new HashSet<GameObject>();
    private GameObject persistentPlayer;
    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (loadingScreen == null) return;
        loadingScreen.transform.SetParent(transform, true);
        loadingScreen.alpha = 0f;
        loadingScreen.blocksRaycasts = false;
        loadingScreen.interactable = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void StartTransition(
        string sceneName,
        Transform player,
        string spawnPointTag)
    {
        if (isTransitioning || GameManager.Instance?.RunEnded == true)
            return;

        if (player == null || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Player missing or scene not enabled in Build Profiles: " + sceneName, this);
            return;
        }

        if (player.TryGetComponent<PlayerHealth>(out var health))
            PreserveHud(health.Region);
        if (player.TryGetComponent<PlayerEnergyBarUI>(out var energyUI))
            PreserveHud(energyUI.Region);

        // Tách manager và Player khỏi cây scene để Unity giữ chúng khi nạp map mới.
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
        player.SetParent(null, true);
        DontDestroyOnLoad(player.gameObject);
        persistentPlayer = player.gameObject;
        StartCoroutine(TransitionRoutine(
            sceneName, player, spawnPointTag));
    }

    private void PreserveHud(RectTransform region)
    {
        if (region == null) return;

        var canvas = region.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Giữ Canvas gốc đứng riêng. Canvas con của Player đã được giữ theo cây Player.
        var rootCanvas = canvas.rootCanvas;
        if (rootCanvas != null && rootCanvas.transform.parent == null)
        {
            persistentHud.Add(rootCanvas.gameObject);
            DontDestroyOnLoad(rootCanvas.gameObject);
        }
    }

    public void ClearRunObjects()
    {
        foreach (var canvas in persistentHud)
            if (canvas != null)
                Destroy(canvas);
        if (persistentPlayer != null)
            Destroy(persistentPlayer);
        Destroy(gameObject);
    }

    private IEnumerator TransitionRoutine(
        string sceneName,
        Transform player,
        string spawnPointTag)
    {
        isTransitioning = true;
        // Nếu scene đích có Player đặt sẵn, tắt bản đó trước khi tiếp tục với Player mang từ scene trước.
        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> onLoaded = (scene, mode) =>
        {
            if (mode != LoadSceneMode.Single) return;

            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                    if (candidate != player && candidate.CompareTag("Player"))
                    {
                        candidate.gameObject.SetActive(false);
                        Destroy(candidate.gameObject);
                    }
        };
        SceneManager.sceneLoaded += onLoaded;
        try
        {
        Debug.Log("TransitionRoutine started!");

        // Hiện màn hình chờ và chặn thao tác trong lúc chuyển map.
        if (loadingScreen != null) loadingScreen.blocksRaycasts = true;

        yield return StartCoroutine(Fade(1f));

        // Tải scene bất đồng bộ để vẫn cập nhật được thanh tiến trình.
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);
        Debug.Log("Loading scene: " + sceneName);
        if (operation == null)
        {
            Debug.LogError("Scene loading failed!");
            yield break;
        }

        // Unity báo tiến độ tối đa 0.9 trước khi cho kích hoạt scene. Quy đổi mốc này thành 100% trên thanh tải.
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            if (progressBar != null)
                progressBar.value = operation.progress / 0.9f;

            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;

        Debug.Log("Scene is ready! Activating...");

        operation.allowSceneActivation = true;

        while (!operation.isDone)
            yield return null;


        // Tìm điểm xuất hiện trong scene đích sau khi scene đã kích hoạt.
        Debug.Log("Scene loading completed!");
        Debug.Log("Searching for SpawnPoint...");

        Transform spawnPoint = FindSpawnPoint(SceneManager.GetActiveScene(), spawnPointTag);

        Debug.Log("SpawnPoint Tag: " + spawnPointTag);
        Debug.Log("SpawnPoint Found: " + (spawnPoint != null));

        if (player != null)
        {
            Debug.Log("Player Before: " + player.position);
        }
        else
        {
            Debug.LogError("Player Transform is NULL!");
        }

        if (spawnPoint != null && player != null)
        {
            Debug.Log("Target Position: " + spawnPoint.position);
            PlacePlayer(player, spawnPoint);
            Debug.Log("Player After: " + player.position);
        }
        else
        {
            Debug.LogError("Cannot move Player to SpawnPoint!");
        }


        // Đợi một frame sau khi đặt Player rồi làm mờ màn hình chờ.
        yield return null;

        yield return StartCoroutine(Fade(0f));

        if (loadingScreen != null) loadingScreen.blocksRaycasts = false;

        if (progressBar != null)
            progressBar.value = 0f;

        isTransitioning = false;
        }
        // Dọn callback và bỏ chặn UI kể cả khi coroutine thoát sớm.
        finally
        {
            SceneManager.sceneLoaded -= onLoaded;
            isTransitioning = false;
            if (loadingScreen != null)
            {
                loadingScreen.alpha = 0f;
                loadingScreen.blocksRaycasts = false;
            }
        }
    }

    internal static Transform FindSpawnPoint(Scene scene, string tag)
    {
        Transform spawnPoint = null;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                if (candidate.gameObject.tag == tag)
                    spawnPoint = candidate;
        return spawnPoint;
    }

    internal static void PlacePlayer(Transform player, Transform spawnPoint)
    {
        Vector3 destination = spawnPoint.position;
        player.position = destination;
        if (player.TryGetComponent<Rigidbody2D>(out var body))
        {
            body.position = destination;
            body.linearVelocity = Vector2.zero;
        }

        // Đồng bộ collider với vị trí mới và xóa trạng thái camera cũ để camera bắt ngay điểm dịch chuyển.
        Physics2D.SyncTransforms();
        foreach (var camera in FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Exclude))
            if (camera.gameObject.scene == spawnPoint.gameObject.scene && camera.Follow == player)
                camera.PreviousStateIsValid = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (loadingScreen == null) yield break;
        float startAlpha = loadingScreen.alpha;
        float elapsed = 0f;

        // Dùng thời gian thực để hiệu ứng mờ vẫn chạy khi timeScale bằng 0.
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            loadingScreen.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                elapsed / fadeDuration
            );

            yield return null;
        }

        loadingScreen.alpha = targetAlpha;
    }
}
