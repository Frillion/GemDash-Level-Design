using System;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField]private GameObject camera;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") && !other.isTrigger)
        {
            camera.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1") && !other.isTrigger)
        {
            camera.SetActive(false);
        }
    }
}
