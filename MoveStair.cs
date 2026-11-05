using UnityEngine;

public class MovingPlatform2D : MonoBehaviour
{
    [Header("Cài đặt di chuyển")]
    [Tooltip("Độ cao tối đa bệ sẽ di chuyển lên tính từ vị trí gốc")]
    [SerializeField] private float moveDistance = 4f;
    [Tooltip("Tốc độ di chuyển")]
    [SerializeField] private float speed = 2.5f;

    private Vector2 startPoint;
    private Vector2 targetPoint;

    private void Start()
    {
        // Lưu vị trí ban đầu (điểm thấp nhất)
        startPoint = transform.position;
        // Điểm nâng cao nhất tính thẳng đứng theo trục Y
        targetPoint = startPoint + Vector2.up * moveDistance;
    }

    private void Update()
    {
        // Di chuyển mượt mà về phía targetPoint
        transform.position = Vector2.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);

        // Đảo chiều khi đã chạm tới đích
        if (Vector2.Distance(transform.position, targetPoint) < 0.05f)
        {
            targetPoint = (targetPoint == startPoint) ? startPoint + Vector2.up * moveDistance : startPoint;
        }
    }

    // Xử lý giữ Player di chuyển cùng bệ đỡ khi đứng lên
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Kiểm tra Player phải đứng trên bề mặt bệ (tiếp xúc từ trên xuống)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    collision.transform.SetParent(transform);
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Thả Player ra khỏi bệ khi nhảy hoặc rời đi
            collision.transform.SetParent(null);
        }
    }

    // Vẽ đường đi trực quan trong cửa sổ Scene để căn chỉnh
    private void OnDrawGizmosSelected()
    {
        Vector2 start = Application.isPlaying ? startPoint : (Vector2)transform.position;
        Vector2 end = start + Vector2.up * moveDistance;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.15f);
        Gizmos.DrawWireSphere(end, 0.15f);
    }
}