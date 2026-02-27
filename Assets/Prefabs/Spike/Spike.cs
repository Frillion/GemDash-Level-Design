using AGDDPlatformer;
using UnityEngine;

public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something hit spike: " + other.name);

        if (other.CompareTag("Player1"))
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            GameManager.Instance.ResetLevel();
            //player.ResetPlayer();
        }
    }
}