using UnityEngine;

// Mỗi cổng khai báo scene đến và tag điểm xuất hiện. SceneTransitionManager xử lý quá trình tải.
public class PlayerTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    public string nextSceneName;
    public string spawnPointTag = "SpawnPoint";

    public void Teleport(Transform player)
    {
        if (SceneTransitionManager.Instance == null)
        {
            new GameObject("SceneTransitionManager").AddComponent<SceneTransitionManager>();
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("Next Scene Name is missing!");
            return;
        }
        SceneTransitionManager.Instance.StartTransition(
            nextSceneName,
            player,
            spawnPointTag
        );
    }
}
