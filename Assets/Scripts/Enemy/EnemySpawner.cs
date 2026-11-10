using System;
using UnityEngine;

namespace Metroidvania.Gameplay
{
    // Tạo quái ngay khi map bắt đầu để quái đã có mặt lúc Player đi tới. Có thể chọn prefab cố định hoặc bốc ngẫu nhiên.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1000)]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Serializable]
        private struct SpawnEntry
        {
            public GameObject prefab;
            public Transform spawnPoint;
        }

        [SerializeField] private SpawnEntry[] enemies = Array.Empty<SpawnEntry>();
        [SerializeField] private GameObject[] randomPrefabs = Array.Empty<GameObject>();
        [SerializeField, Min(0f)] private float batHeightAboveGround = 1.5f;

        private void Start()
        {
            var pool = Array.FindAll(randomPrefabs, prefab => prefab != null);
            var groundFilter = new ContactFilter2D();
            groundFilter.SetLayerMask(LayerMask.GetMask("Ground", "Default"));
            groundFilter.SetNormalAngle(45f, 135f);
            groundFilter.useTriggers = false;
            var groundHits = new RaycastHit2D[1];

            for (int i = 0; i < enemies.Length; i++)
            {
                var entry = enemies[i];
                GameObject prefab = pool.Length > 0
                    ? pool[UnityEngine.Random.Range(0, pool.Length)]
                    : entry.prefab;
                if (prefab == null || entry.spawnPoint == null)
                {
                    Debug.LogWarning($"EnemySpawner: entry {i} needs a prefab and spawn point.", this);
                    continue;
                }

                Vector3 position = entry.spawnPoint.position;
                if (pool.Length > 0)
                    position = GetRandomSpawnPosition(prefab, position, groundFilter, groundHits);

                Instantiate(prefab, position, entry.spawnPoint.rotation, transform);
            }
        }

        private Vector3 GetRandomSpawnPosition(GameObject prefab, Vector3 position,
            ContactFilter2D groundFilter, RaycastHit2D[] groundHits)
        {
            Vector2 rayStart = (Vector2)position + Vector2.up * 2f;
            if (Physics2D.Raycast(rayStart, Vector2.down, groundFilter, groundHits, 10f) == 0 ||
                groundHits[0].fraction == 0f)
                return position;

            // Dùng mặt sàn và mép dưới collider để đặt quái mặt đất. Dơi được đặt cao hơn sàn theo cấu hình.
            float floorY = groundHits[0].point.y;
            if (prefab.TryGetComponent<EnemyBatFlight>(out _))
                position.y = floorY + batHeightAboveGround;
            else if (prefab.TryGetComponent<BoxCollider2D>(out var box))
                position.y = floorY + (box.size.y * 0.5f - box.offset.y) * prefab.transform.localScale.y + 0.02f;
            else if (prefab.TryGetComponent<CapsuleCollider2D>(out var capsule))
                position.y = floorY + (capsule.size.y * 0.5f - capsule.offset.y) * prefab.transform.localScale.y + 0.02f;

            return position;
        }
    }
}
