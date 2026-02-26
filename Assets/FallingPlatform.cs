using System.Collections;
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
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float shakeMagnitude = 0.1f;
    private Vector3 originalLocalPosition;
    private Color originalColor;

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

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
        originalColor = spriteRenderer.material.color;
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
                Color c = originalColor;
                spriteRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }

        }
    }


    private void HandleCollapsingVFX()
    {
        if (isFalling && !collapsed)
        {
            StartCoroutine(ShakeThenFade());
        }
    }


    private IEnumerator ShakeThenFade()
    {
        while (isFalling && !collapsed)
        {
            float xOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = originalLocalPosition + new Vector3(xOffset, 0f, 0f);

            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, 0.3f);
    }
}
