using AGDDPlatformer;
using UnityEngine;

public class FallingPlatform : MovingPlatform
{
    [SerializeField] bool isMovingPlatform = false;

    [Header("Falling Platform Settings")]
    [SerializeField] float fallDelay = 0.5f;
    private float fallTimer = 0f;
    private bool isFalling = false;
    private bool collapsed = false;

    [Header("Regen Settings")]
    [SerializeField] bool doesRegen = false;
    [SerializeField] float regenTime = 2f;
    private float regenTimer = 0f;
    private bool isRegening = false;

    protected override void Update()
    {
        if (isMovingPlatform)
            base.Update();

        HandleCollapsingPlatform();
        HandleRegeningPlatform();

    }

    protected override void OnCollisionStay2D(Collision2D other)
    {
        base.OnCollisionStay2D(other);
        PlayerController potentialPlayer = other.gameObject.GetComponent<PlayerController>();
        if (potentialPlayer != null && !isFalling)
        {
            isFalling = true;
            HandleCollapsingVFX();
        }
    }


    private void HandleCollapsingPlatform()
    {
        if (isFalling)
        {

            if (fallTimer < fallDelay)
                fallTimer += Time.deltaTime;
            else
                collapsed = true;
        }

        if (collapsed)
        {
            boxCollider.enabled = false;
            if (doesRegen)
            {
                isRegening = true;
                collapsed = false;
                isFalling = false;
                fallTimer = 0f;
            }    
        }
    }


    private void HandleRegeningPlatform()
    {
        if (isRegening)
        {
            if(regenTimer < regenTime)
                regenTimer += Time.deltaTime;
            else
            {
                boxCollider.enabled = true;
                isRegening = false;
                regenTimer = 0f;
            }

        }
    }


    private void HandleCollapsingVFX()
    {
        // TODO: Setup falling visual FX when the player lands on the object
    }
}
