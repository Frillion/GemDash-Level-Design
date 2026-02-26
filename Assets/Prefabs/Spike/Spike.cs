using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
        {
            // For testing, just destroy player
            Destroy(other.gameObject);
            other.GetComponent<PlayerHealth>().TakeDamage(1);

        }
    }
}