using AGDDPlatformer;
using UnityEngine;

public class SimplyMoveTheSpikesAwesomeScript : MonoBehaviour
{
    [SerializeField] float moveSpeed = 0.5f;
    public bool toggleMoving = false;
    private Vector3 originalPosition;



    private void Awake()
    {
        originalPosition = transform.position;
    }


    void Update()
    {
        if (toggleMoving)
            transform.position += transform.up * moveSpeed * Time.deltaTime;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
        {
            toggleMoving = true;
        }
    }


    private void OnEnable()
    {
        GameManager.OnLevelReset += HandleLevelReset;
    }

    private void OnDisable()
    {
        GameManager.OnLevelReset -= HandleLevelReset;
    }

    private void HandleLevelReset()
    {
        transform.position = originalPosition;
        toggleMoving = false;
    }

}
