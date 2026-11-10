using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    private HashSet<string> loadingChunks = new HashSet<string>();

    public void LoadChunk(string chunkName)
    {
        if (string.IsNullOrWhiteSpace(chunkName) || !Application.CanStreamedLevelBeLoaded(chunkName)) return;
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

