using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 1; // spikes kill immediately

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
            Die();
    }

    public void Die()
    {
        // For now, just destroy the player
        Destroy(gameObject);
        // Later you can add respawn, UI, sound effects, etc.
    }
}