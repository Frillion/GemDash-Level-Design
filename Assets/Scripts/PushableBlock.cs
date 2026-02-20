using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AGDDPlatformer
{
    public class PushableBlock : KinematicObject
    {
        GameObject CollidingObject = null;
        KinematicObject CollidingObjectBody = null;


        void OnCollisionStay2D(Collision2D other)
        {
            //Store the other collision object's KinematicObject to improve performance
            if (CollidingObject != other.gameObject)
            {
                CollidingObject = other.gameObject;
                CollidingObjectBody = CollidingObject.GetComponent<KinematicObject>();
            }

            //Return if the other object is not a KinematicObject
            if (CollidingObjectBody == null) { return; }

            Vector2 distanceVector = CollidingObject.transform.position - transform.position;
            float dot = Vector2.Dot(CollidingObjectBody.velocity, distanceVector) * gravityModifier;

            //Make sure that the pusher is not above the object (relative to gravity)
            if (distanceVector.y * CollidingObjectBody.gravityModifier > 0)
            {
                if (AttatchedTo == CollidingObjectBody) Detatch();
                return;
            }

            //Make sure that that the pusher is not travelling away from the object
            if (CollidingObjectBody.velocity.sqrMagnitude > 0 && Mathf.Abs(dot) > 0.01f && dot > 0)
            {
                if (AttatchedTo == CollidingObjectBody) Detatch();
                return;
            }

            //If we are already connected to a player through some chain, then that takes precedence
            KinematicObject curr = this;
            while (curr.GetAttatchedTo() != null)
            {
                if (curr.GetAttatchedTo().tag.StartsWith("Player")) { return; }
                curr = curr.GetAttatchedTo();
            }

            AttatchTo(CollidingObjectBody);
        }

        void OnCollisionExit2D(Collision2D other)
        {
            var otherBody = other.gameObject.GetComponent<KinematicObject>();
            if (otherBody == null) { return; }

            if (other.gameObject == CollidingObject)
            {
                CollidingObject = null;
                CollidingObjectBody = null;
            }

            Detatch();
        }
    }
}