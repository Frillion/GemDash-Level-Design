using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
    Debug.Log("Something hit spike: " + other.name);

    if (other.CompareTag("Player1"))
        {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null)
            {
            Debug.Log("Damaging player");
            health.TakeDamage(1);
            }
        }
    }
}