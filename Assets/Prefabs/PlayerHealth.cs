using UnityEngine;
using AGDDPlatformer;

public class PlayerHealth : MonoBehaviour
{
    public int health = 1;

    PlayerController controller;
    bool isDead;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        controller.enabled = false; // freeze player movement
        UIManager.instance.ShowRespawnText();
    }

    void Update()
    {
        if (isDead && Input.anyKeyDown)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        health = 1;
        isDead = false;

        controller.ResetPlayer();
        controller.enabled = true;

        UIManager.instance.HideRespawnText();
    }
}