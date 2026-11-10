using UnityEngine;

// Khi Player bước vào vùng trigger, gửi tên chunk đã gán trong Inspector cho ChunkManager.
public class ChunkTrigger : MonoBehaviour
{
    public ChunkManager chunkManager;

    public string chunkName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (chunkManager == null || string.IsNullOrWhiteSpace(chunkName)) return;
            chunkManager.LoadChunk(chunkName);

            Debug.Log("Player entered: " + chunkName);
        }
    }
}
