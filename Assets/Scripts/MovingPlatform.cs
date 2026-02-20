using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace AGDDPlatformer
{
    public class MovingPlatform : KinematicObject
    {
        public float Speed; //Probably should be the same as player max speed
        public Transform StartPoint;
        public Transform EndPoint;

        enum Points
        {
            Start, End
        }

        private Points GoingTowards = Points.End;

        void Update()
        {
            //Move the platform back and forth between the start and end points
            Vector2 startToEnd = EndPoint.position - StartPoint.position;
            Vector2 progressToEnd = EndPoint.position - transform.position;
            Vector2 progressToStart = StartPoint.position - transform.position;
            if (GoingTowards == Points.End)
            {
                velocity = progressToEnd.normalized * Speed;
            }
            else
            {
                velocity = progressToStart.normalized * Speed;
            }

            if (GoingTowards == Points.End && Vector2.Dot(progressToEnd, startToEnd) <= 0)
            {
                GoingTowards = Points.Start;
            }
            else if (GoingTowards == Points.Start && Vector2.Dot(progressToStart, -startToEnd) <= 0)
            {
                GoingTowards = Points.End;
            }
        }

        void OnCollisionStay2D(Collision2D other)
        {
            var otherBody = other.gameObject.GetComponent<KinematicObject>();
            if (otherBody == null) { return; }

            //Attatch if something is grounded on the platform
            if (otherBody.GetGroundedOnObject() == gameObject)
            {
                otherBody.AttatchTo(this);
            }
            else
            {
                //If it is not grounded we can detatch. 
                otherBody.Detatch();
                //If it is a player, give them a small boost to simulate inertia.
                otherBody.GetComponent<PlayerController>()?.SetJumpBoost(new Vector2(velocity.x, 0));
            }
        }

        void OnCollisionExit2D(Collision2D other)
        {
            var otherBody = other.gameObject.GetComponent<KinematicObject>();
            if (otherBody == null) { return; }

            //We can detatch if the object exits the collision.
            otherBody.Detatch();
            //If it is a player, give them a small boost to simulate inertia.
            otherBody.GetComponent<PlayerController>()?.SetJumpBoost(new Vector2(velocity.x, 0));
        }
    }
}