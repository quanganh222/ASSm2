using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    private bool playerInRange = false;
    private Transform player;

    private PlayerTeleport playerTeleport;

    private void Awake()
    {
        playerTeleport = GetComponent<PlayerTeleport>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F detected!");

            if (playerInRange)
            {
                Debug.Log("Inside Portal!");

                if (player != null && playerTeleport != null)
                {
                    playerTeleport.Teleport(player);
                }
            }
            else
            {
                Debug.Log("Outside Portal!");
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.transform;
        }
        Debug.Log("Player entered Portal:");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
        Debug.Log("Player exited Portal");
    }
}
