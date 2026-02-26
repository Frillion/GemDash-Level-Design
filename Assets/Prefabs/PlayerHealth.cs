using UnityEngine;
using AGDDPlatformer;

public class PlayerHealth : MonoBehaviour
{
    public int health = 1;

    PlayerController controller;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            ResetPlayer();
        }
    }

    void ResetPlayer()
    {
        health = 1;                
        controller.ResetPlayer();  
    }
}