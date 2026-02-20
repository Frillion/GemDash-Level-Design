using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AGDDPlatformer
{
    public class KinematicObject : MonoBehaviour
    {
        [Header("Settings")]
        public float minGroundNormalY = 0.65f;
        public float gravityModifier = 1;

        [Header("Info")]
        public Vector2 velocity;
        public bool isGrounded;
        public bool isFrozen;

        protected GameObject GroundedOnObject = null;
        protected Vector2 GroundNormal = new Vector2(0, 1);
        protected Rigidbody2D Body;
        protected ContactFilter2D ContactFilter;
        protected readonly RaycastHit2D[] HitBuffer = new RaycastHit2D[16];

        protected const float MinMoveDistance = 0.001f;
        protected const float ShellRadius = 0.01f;


        protected readonly List<KinematicObject> AttatchedObjects = new List<KinematicObject>();
        protected KinematicObject AttatchedTo = null;

        protected void OnEnable()
        {
            Body = GetComponent<Rigidbody2D>();
            Body.bodyType = RigidbodyType2D.Kinematic;
        }

        protected void OnDisable()
        {
            Body.bodyType = RigidbodyType2D.Dynamic;
        }

        protected void Start()
        {
            ContactFilter.useTriggers = false;
            ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            ContactFilter.useLayerMask = true;
        }

        protected void FixedUpdate()
        {
            if (isFrozen)
                return;

            velocity += Physics2D.gravity * (gravityModifier * 0.01f);

            isGrounded = false;
            GroundedOnObject = null;

            var deltaPosition = velocity * Time.deltaTime;
            var groundVector = new Vector2(GroundNormal.y, -GroundNormal.x);
            var groundMove = groundVector * deltaPosition.x;
            PerformMovement(groundMove, false);

            var airMove = Vector2.up * deltaPosition.y;
            PerformMovement(airMove, true);
        }

        private void PerformMovement(Vector2 move, bool yMovement)
        {
            //Push attatched KinematicObjects
            foreach (var attatchedObject in AttatchedObjects.Where(attatchedObject => 
                         Vector2.Dot(attatchedObject.transform.position - transform.position, move) >= 0))
            {
                attatchedObject.PerformMovement(move, yMovement);
            }

            var distance = move.magnitude;

            if (distance > MinMoveDistance)
            {
                //check if we hit anything in current direction of travel
                var count = Body.Cast(move, ContactFilter, HitBuffer, distance + ShellRadius);
                for (var i = 0; i < count; i++)
                {
                    var currentNormal = HitBuffer[i].normal;

                    //is this surface flat enough to land on?
                    if ((gravityModifier >= 0 && currentNormal.y > minGroundNormalY) ||
                        (gravityModifier < 0 && currentNormal.y < -minGroundNormalY))
                    {
                        isGrounded = true;
                        GroundedOnObject = HitBuffer[i].collider.gameObject;
                        // if moving up, change the groundNormal to new surface normal.
                        if (yMovement)
                        {
                            GroundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }

                    if (isGrounded)
                    {
                        //how much of our velocity aligns with surface normal?
                        var projection = Vector2.Dot(velocity, currentNormal);
                        if (projection < 0)
                        {
                            //slower velocity if moving against the normal (up a hill).
                            velocity -= projection * currentNormal;
                        }
                    }
                    else
                    {
                        velocity.y = gravityModifier switch
                        {
                            //We are airborne, but hit something, so cancel vertical up and horizontal velocity.
                            >= 0 when currentNormal.y < -0.01f => Mathf.Min(velocity.y, 0),
                            < 0 when currentNormal.y > 0.01f => Mathf.Max(velocity.y, 0),
                            _ => velocity.y
                        };

                        if (!Mathf.Approximately(Mathf.Sign(currentNormal.x), Mathf.Sign(velocity.x)))
                        {
                            velocity.x = 0;
                        }
                    }

                    //remove shellDistance from actual move distance.
                    var modifiedDistance = HitBuffer[i].distance - ShellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }

            //Perform actual move
            Body.position += move.normalized * distance;
            var hitSomething = !Mathf.Approximately(distance, move.magnitude);

            //Pull attatched KinematicObjects
            foreach (var attatchedObject in AttatchedObjects)
            {
                if (Vector2.Dot(attatchedObject.transform.position - transform.position, move) < 0)
                {
                    attatchedObject.PerformMovement(move.normalized * distance, yMovement);
                }
                else if (hitSomething) //Pull back if pushed too far
                {
                    attatchedObject.PerformMovement((move.normalized * distance)-move, yMovement);
                }
            }
        }

        //Attatch to another KinematicObject
        //this object will then inherit the other object's movements, but can still move on its own
        public void AttatchTo(KinematicObject other)
        {
            if (AttatchedTo == other) { return; } //Already attatched to this KinematicObject
            Detatch(); //Make sure to properly detatch from any other object
            other.AttatchedObjects.Add(this);
            AttatchedTo = other;

            if (other.AttatchedTo == this)
            {
                other.Detatch();
            }
        }

        //Detatch from attatched KinematicObject
        public void Detatch()
        {
            if (AttatchedTo == null) return;
            AttatchedTo.AttatchedObjects.Remove(this);
            AttatchedTo = null;
        }

        //What object am I standing on?
        public GameObject GetGroundedOnObject()
        {
            return GroundedOnObject;
        }

        public KinematicObject GetAttatchedTo()
        {
            return AttatchedTo;
        }
    }
}
