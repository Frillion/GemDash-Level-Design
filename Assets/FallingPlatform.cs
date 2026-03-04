using System;
using System.Collections;
using System.Threading;
using AGDDPlatformer;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class FallingPlatform : MovingPlatform
{
    private static readonly int DissolveStrength = Shader.PropertyToID("_dissolveStrength");
    [SerializeField] bool isMovingPlatform = false;

    [Header("Falling Platform Settings")]
    [SerializeField] float fallDelay = 0.5f;
    private bool isFalling = false;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float shakeMagnitude = 0.1f;
    private Vector3 originalLocalPosition;
    
    [Header("Fade Out Settings")]
    [SerializeField] private AnimationCurve fadeoutCurve;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private float fadeOutCurrentTime;

    [Header("Regen Settings")]
    [SerializeField] bool doesRegen = false;
    [SerializeField] float regenTime = 2f;

    private CancellationTokenSource fadeCancellationTokens;

    protected override void Update()
    {
        if (isMovingPlatform)
            base.Update();
    }

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
        GameManager.OnLevelReset += Restart;
        Restart();
    }

    private void OnDestroy()
    {
        GameManager.OnLevelReset -= Restart;
    }

    private void Restart()
    {
        fadeCancellationTokens?.Dispose();

        fadeCancellationTokens =
            CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(),
                CancellationToken.None);
    }

    protected override void OnCollisionStay2D(Collision2D other)
    {
        base.OnCollisionStay2D(other);
        var potentialPlayer = other.gameObject.GetComponent<PlayerController>();
        
        if (potentialPlayer == null || isFalling) return;
        isFalling = true;
        FadeOutAfterSeconds(fallDelay, fadeCancellationTokens.Token).Forget();
    }

    private async UniTask FadeOutAfterSeconds(float seconds, CancellationToken token)
    {
        try
        {
            await UniTask.WaitForSeconds(seconds, cancellationToken: token);
            while (!token.IsCancellationRequested)
            {
                if (fadeOutCurrentTime >= fadeOutDuration)
                {
                    if (doesRegen) FadeInAfterSeconds(regenTime, token).Forget();
                    boxCollider.enabled = false;
                    isFalling = false;
                    return;
                }


                var xOffset = Random.Range(-1f, 1f) * shakeMagnitude;
                transform.localPosition = originalLocalPosition + new Vector3(xOffset, 0f, 0f);

                fadeOutCurrentTime += Time.deltaTime;
                spriteRenderer.material.SetFloat(DissolveStrength,
                    fadeoutCurve.Evaluate(fadeOutCurrentTime / fadeOutDuration));

                await UniTask.NextFrame(cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
            isFalling = false;
            fadeOutCurrentTime = 0;
            if (boxCollider != null) { boxCollider.enabled = true; }

            if (spriteRenderer != null)
            {
                spriteRenderer.material.SetFloat(DissolveStrength, fadeoutCurve.Evaluate(0));
            }

        }
    }
    
    private async UniTask FadeInAfterSeconds(float seconds, CancellationToken token)
    {
        try
        {
            await UniTask.WaitForSeconds(seconds, cancellationToken: token);

            while (!token.IsCancellationRequested)
            {
                boxCollider.enabled = true;
                if (fadeOutCurrentTime <= 0) return;

                fadeOutCurrentTime -= Time.deltaTime;
                spriteRenderer.material.SetFloat(DissolveStrength,
                    fadeoutCurve.Evaluate(fadeOutCurrentTime / fadeOutDuration));

                await UniTask.NextFrame(cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
            fadeOutCurrentTime = 0;
            if (boxCollider != null) { boxCollider.enabled = true; }

            if (spriteRenderer != null)
            {
                spriteRenderer.material.SetFloat(DissolveStrength, fadeoutCurve.Evaluate(0));
            }
        }
    }
}
