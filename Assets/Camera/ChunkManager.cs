using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// Nạp các khu vực bổ sung bằng Additive để giữ scene hiện tại.
public class ChunkManager : MonoBehaviour
{
    private HashSet<string> loadingChunks = new HashSet<string>();

    public void LoadChunk(string chunkName)
    {
        if (string.IsNullOrWhiteSpace(chunkName) || !Application.CanStreamedLevelBeLoaded(chunkName)) return;
        // Chặn yêu cầu trùng ngay cả khi chunk đang tải và chưa được Unity đánh dấu là đã nạp.
        Scene chunk = SceneManager.GetSceneByName(chunkName);

        if (chunk.isLoaded || loadingChunks.Contains(chunkName))
        {
            return;
        }

        StartCoroutine(LoadChunkCoroutine(chunkName));
    }

    private IEnumerator LoadChunkCoroutine(string chunkName)
    {
        loadingChunks.Add(chunkName);

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            chunkName,
            LoadSceneMode.Additive
        );

        yield return operation;

        loadingChunks.Remove(chunkName);

        Debug.Log("Loaded Chunk: " + chunkName);
    }

    public void UnloadChunk(string chunkName)
    {
        Scene chunk = SceneManager.GetSceneByName(chunkName);

        if (!chunk.isLoaded)
        {
            return;
        }

        SceneManager.UnloadSceneAsync(chunkName);

        Debug.Log("Unloaded Chunk: " + chunkName);
    }
}

