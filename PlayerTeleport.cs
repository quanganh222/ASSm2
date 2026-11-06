using UnityEngine;

public class PlayerTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    public string nextSceneName;
    public string spawnPointTag = "SpawnPoint";

    public void Teleport(Transform player)
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager not found!");
            return;
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("Next Scene Name is missing!");
            return;
        }
        DontDestroyOnLoad(player.gameObject);
        SceneTransitionManager.Instance.StartTransition(
            nextSceneName,
            player,
            spawnPointTag
        );
    }
}
