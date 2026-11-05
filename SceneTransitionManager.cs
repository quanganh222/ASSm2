using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Loading Screen")]
    public CanvasGroup loadingScreen;
    public Slider progressBar;

    [Header("Transition Settings")]
    public float fadeDuration = 0.5f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        loadingScreen.alpha = 0f;
        loadingScreen.blocksRaycasts = false;
        loadingScreen.interactable = false;
    }

    public void StartTransition(
        string sceneName,
        Transform player,
        string spawnPointTag)
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionRoutine(
            sceneName, player, spawnPointTag));
    }

    private IEnumerator TransitionRoutine(
        string sceneName,
        Transform player,
        string spawnPointTag)
    {
        isTransitioning = true;
        Debug.Log("TransitionRoutine started!");

        // 1. Show Loading Screen
        loadingScreen.blocksRaycasts = true;

        yield return StartCoroutine(Fade(1f));

        // 2. Load new Scene asynchronously
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);
        Debug.Log("Loading scene: " + sceneName);
        if (operation == null)
        {
            Debug.LogError("Scene loading failed!");
            yield break;
        }

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


        // 4. Find SpawnPoint
        Debug.Log("Scene loading completed!");
        Debug.Log("Searching for SpawnPoint...");

        GameObject spawnPoint =
            GameObject.FindGameObjectWithTag(spawnPointTag);

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
            Debug.Log("Target Position: " + spawnPoint.transform.position);

            player.position = spawnPoint.transform.position;

            Physics2D.SyncTransforms();

            Debug.Log("Player After: " + player.position);
        }
        else
        {
            Debug.LogError("Cannot move Player to SpawnPoint!");
        }


        // 5. Hide Loading Screen
        yield return null;

        yield return StartCoroutine(Fade(0f));

        loadingScreen.blocksRaycasts = false;

        if (progressBar != null)
            progressBar.value = 0f;

        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = loadingScreen.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

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