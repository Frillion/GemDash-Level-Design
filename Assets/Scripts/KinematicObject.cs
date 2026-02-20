using System;
using UnityEngine;
using System.Collections.Generic;

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

        protected GameObject groundedOnObject = null;
        protected Vector2 groundNormal = new Vector2(0, 1);
        protected Rigidbody2D body;
        protected ContactFilter2D contactFilter;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

        protected const float minMoveDistance = 0.001f;
        protected const float shellRadius = 0.01f;


        protected List<KinematicObject> AttatchedObjects = new List<KinematicObject>();
        protected KinematicObject AttatchedTo = null;

        protected void OnEnable()
        {
            body = GetComponent<Rigidbody2D>();
            body.isKinematic = true;
        }

        protected void OnDisable()
        {
            body.isKinematic = false;
        }

        protected void Start()
        {
            contactFilter.useTriggers = false;
            contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            contactFilter.useLayerMask = true;
        }

        protected void FixedUpdate()
        {
            if (isFrozen)
                return;

            velocity += gravityModifier * Physics2D.gravity * 0.01f;

            isGrounded = false;
            groundedOnObject = null;

            Vector2 deltaPosition = velocity * Time.deltaTime;
            Vector2 groundVector = new Vector2(groundNormal.y, -groundNormal.x);
            Vector2 groundMove = groundVector * deltaPosition.x;
            PerformMovement(groundMove, false);

            Vector2 airMove = Vector2.up * deltaPosition.y;
            PerformMovement(airMove, true);
        }

        void PerformMovement(Vector2 move, bool yMovement)
        {
            //Push attatched KinematicObjects
            foreach (KinematicObject attatchedObject in AttatchedObjects)
            {
                if (Vector2.Dot(attatchedObject.transform.position - transform.position, move) >= 0)
                {
                    attatchedObject.PerformMovement(move, yMovement);
                }
            }

            float distance = move.magnitude;

            if (distance > minMoveDistance)
            {
                //check if we hit anything in current direction of travel
                int count = body.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
                for (int i = 0; i < count; i++)
                {
                    Vector2 currentNormal = hitBuffer[i].normal;

                    //is this surface flat enough to land on?
                    if ((gravityModifier >= 0 && currentNormal.y > minGroundNormalY) ||
                        (gravityModifier < 0 && currentNormal.y < -minGroundNormalY))
                    {
                        isGrounded = true;
                        groundedOnObject = hitBuffer[i].collider.gameObject;
                        // if moving up, change the groundNormal to new surface normal.
                        if (yMovement)
                        {
                            groundNormal = currentNormal;
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
                        //We are airborne, but hit something, so cancel vertical up and horizontal velocity.
                        if (gravityModifier >= 0 && currentNormal.y < -0.01f)
                        {
                            velocity.y = Mathf.Min(velocity.y, 0);
                        }

                        if (gravityModifier < 0 && currentNormal.y > 0.01f)
                        {
                            velocity.y = Mathf.Max(velocity.y, 0);
                        }

                        if (Mathf.Sign(currentNormal.x) != Mathf.Sign(velocity.x))
                        {
                            velocity.x = 0;
                        }
                    }

                    //remove shellDistance from actual move distance.
                    var modifiedDistance = hitBuffer[i].distance - shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }

            //Perform actual move
            body.position += move.normalized * distance;
            bool hitSomething = distance != move.magnitude;

            //Pull attatched KinematicObjects
            foreach (KinematicObject attatchedObject in AttatchedObjects)
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
        public void AttatchTo(KinematicObject Other)
        {
            if (AttatchedTo == Other) { return; } //Already attatched to this KinematicObject
            Detatch(); //Make sure to properly detatch from any other object
            Other.AttatchedObjects.Add(this);
            AttatchedTo = Other;

            if (Other.AttatchedTo == this)
            {
                Other.Detatch();
            }
        }

        //Detatch from attatched KinematicObject
        public void Detatch()
        {
            if (AttatchedTo != null)
            {
                AttatchedTo.AttatchedObjects.Remove(this);
                AttatchedTo = null;
            }
        }

        //What object am I standing on?
        public GameObject GetGroundedOnObject()
        {
            return groundedOnObject;
        }

        public KinematicObject GetAttatchedTo()
        {
            return AttatchedTo;
        }
    }
}
