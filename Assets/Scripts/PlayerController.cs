using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AGDDPlatformer
{
    public class PlayerController : KinematicObject
    {
        private static readonly int Running = Animator.StringToHash("Running");
        private static readonly int Blend = Shader.PropertyToID("_Blend");

        [Header("Movement")]
        public float maxSpeed = 7;
        public float jumpSpeed = 7;
        public float jumpDeceleration = 0.5f; // Upwards slow after releasing jump button
        public float cayoteTime = 0.1f; // Lets player jump just after leaving ground
        public float jumpBufferTime = 0.1f; // Lets the player input a jump just before becoming grounded
        [SerializeField] private Animator playerAnim;

        [Header("Dash")]
        public float dashSpeed;
        public float dashTime;
        public float dashCooldown;
        public Color canDashColor;
        public Color cantDashColor;
        [SerializeField] private AnimationCurve dashEaseIn;
        [SerializeField] private AnimationCurve dashEaseOut;
        private float _currentDashBlend;
        private CancellationTokenSource dashTokenSource;
        [SerializeField] private ParticleSystem dashParticles;
        float lastDashTime;
        Vector2 dashDirection;
        public bool isDashing;
        bool canDash;
        bool wantsToDash;

        [Header("Audio")]
        public AudioSource source;
        public AudioClip jumpSound;
        public AudioClip dashSound;

        private Vector2 _resetPosition;
        private bool _startOrientation;

        float lastJumpTime;
        float lastGroundedTime;
        bool canJump;
        bool jumpReleased;
        Vector2 move;
        float defaultGravityModifier;

        SpriteRenderer spriteRenderer;

        Vector2 jumpBoost;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            GameManager.OnLevelReset += ResetPlayer;
            ResetToken();
            var emission = dashParticles.emission;
            emission.enabled = false;

            lastJumpTime = -jumpBufferTime * 2;

            _resetPosition = transform.position;
            _startOrientation = spriteRenderer.flipX;

            defaultGravityModifier = gravityModifier;
        }

        private void ResetToken()
        {
            dashTokenSource?.Cancel();
            dashTokenSource?.Dispose();

            dashTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(),
                    CancellationToken.None);
        }

        private void OnDestroy()
        {
            GameManager.OnLevelReset -= ResetPlayer;
        }

        public void SetResetPosition(Vector2 position)
        {
            _resetPosition = position;
        }

        private async UniTask FadeInOutDash(CancellationToken token)
        {
            var emission = dashParticles.emission;
            
            try
            {
                _currentDashBlend = 0;
                var startTime = Time.time;
                emission.enabled = true;
                while (!token.IsCancellationRequested)
                {
                    var currentTime = Time.time - startTime;
                    if (currentTime >= dashTime) return;
                
                    _currentDashBlend = currentTime >= dashTime / 2
                        ? dashEaseOut.Evaluate(currentTime / dashTime / 2)
                        : dashEaseIn.Evaluate(currentTime / dashTime / 2);
                
                    spriteRenderer.material.SetFloat(Blend, _currentDashBlend);

                    await UniTask.NextFrame(cancellationToken:token);
                }
            }
            finally{
                _currentDashBlend = 0;
                emission.enabled = false;
                spriteRenderer.material.SetFloat(Blend, _currentDashBlend);
            }
        }

        private void Update()
        {
            isFrozen = GameManager.Instance.timeStopped;

            /* --- Read Input --- */
            move.x = Input.GetAxisRaw("Horizontal");

            playerAnim.SetBool(Running, move.x != 0);

            if (gravityModifier < 0)
            {
                move.x *= -1;
            }

            move.y = Input.GetAxisRaw("Vertical");

            if (Input.GetButtonDown("Jump"))
            {
                // Store jump time so that we can buffer the input
                lastJumpTime = Time.time;
            }

            if (Input.GetButtonUp("Jump"))
            {
                jumpReleased = true;
            }

            // Clamp directional input to 8 directions for dash
            Vector2 desiredDashDirection = new Vector2(
                move.x == 0 ? 0 : (move.x > 0 ? 1 : -1),
                move.y == 0 ? 0 : (move.y > 0 ? 1 : -1));
            if (desiredDashDirection == Vector2.zero)
            {
                // Dash in facing direction if there is no directional input;
                desiredDashDirection = spriteRenderer.flipX ? -Vector2.right : Vector2.right;
            }
            desiredDashDirection = desiredDashDirection.normalized;
            if (Input.GetButtonDown("Dash"))
            {
                wantsToDash = true;
            }

            /* --- Compute Velocity --- */

            if (canDash && wantsToDash)
            {
                isDashing = true;
                dashDirection = desiredDashDirection;
                lastDashTime = Time.time;
                canDash = false;
                gravityModifier = 0;

                FadeInOutDash(dashTokenSource.Token).Forget();
                source.PlayOneShot(dashSound);
            }
            wantsToDash = false;

            if (isDashing)
            {
                velocity = dashDirection * dashSpeed;
                if (Time.time - lastDashTime >= dashTime)
                {
                    isDashing = false;

                    gravityModifier = defaultGravityModifier;
                    if ((gravityModifier >= 0 && velocity.y > 0) ||
                        (gravityModifier < 0 && velocity.y < 0))
                    {
                        velocity.y *= jumpDeceleration;
                    }
                }
            }
            else
            {
                if (isGrounded)
                {
                    // Store grounded time to allow for late jumps
                    lastGroundedTime = Time.time;
                    canJump = true;
                    if (!isDashing && Time.time - lastDashTime >= dashCooldown)
                        canDash = true;
                }

                // Check time for buffered jumps and late jumps
                float timeSinceJumpInput = Time.time - lastJumpTime;
                float timeSinceLastGrounded = Time.time - lastGroundedTime;

                if (canJump && timeSinceJumpInput <= jumpBufferTime && timeSinceLastGrounded <= cayoteTime)
                {
                    velocity.y = Mathf.Sign(gravityModifier) * jumpSpeed;
                    canJump = false;
                    isGrounded = false;

                    source.PlayOneShot(jumpSound);
                }
                else if (jumpReleased)
                {
                    // Decelerate upwards velocity when jump button is released
                    if ((gravityModifier >= 0 && velocity.y > 0) ||
                        (gravityModifier < 0 && velocity.y < 0))
                    {
                        velocity.y *= jumpDeceleration;
                    }
                    jumpReleased = false;
                }

                velocity.x = move.x * maxSpeed;

                if (isGrounded || (velocity + jumpBoost).magnitude < velocity.magnitude)
                {
                    jumpBoost = Vector2.zero;
                }
                else
                {
                    velocity += jumpBoost;
                    jumpBoost -= jumpBoost * Mathf.Min(1f, Time.deltaTime);
                }
            }

            /* --- Adjust Sprite --- */

            // Assume the sprite is facing right, flip it if moving left
            if (move.x > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
            else if (move.x < -0.01f)
            {
                spriteRenderer.flipX = true;
            }

            spriteRenderer.color = canDash ? canDashColor : cantDashColor;
        }

        public void ResetPlayer()
        {
            ResetToken();
            transform.position = _resetPosition;
            spriteRenderer.flipX = _startOrientation;

            lastJumpTime = -jumpBufferTime * 2;

            velocity = Vector2.zero;
        }

        public void ResetDash()
        {
            canDash = true;
        }

        //Add a short mid-air boost to the player (unrelated to dash). Will be reset upon landing.
        public void SetJumpBoost(Vector2 jumpBoost)
        {
            this.jumpBoost = jumpBoost;
        }
    }
}
