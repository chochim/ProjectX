using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPreview.Kinect;

namespace ProjectX
{
    class GestureRecognitionEngine
    {
        float previousDisance = 0.0f;

        public GestureRecognitionEngine()
        {

        }

        public event EventHandler<GestureEventArgs> GestureRecognized;
        public event EventHandler<GestureEventArgs> GestureNotRecognized;

        public GestureType GestureType { get; set; }
        public Body Body { get; set; }

        public void StartRecognize()
        {
            switch (this.GestureType)
            {
                case GestureType.SwipeLeftGesture:
                    this.MatchSwipeLeftGesture(this.Body);
                    break;
                case GestureType.SwipeRightGesture:
                    this.MatchSwipeRightGesture(this.Body);
                    break;
                default:
                    break;
            }
        }

        private Boolean isRightGestureBeingTracked(Body body)
        {
            return body.Joints[JointType.HandRight].TrackingState == TrackingState.Tracked &&
                    body.Joints[JointType.HandLeft].TrackingState == TrackingState.Tracked;
        }

        private Boolean isLeftGestureBeingTracked(Body body)
        {
            return false;
        }


        private void MatchSwipeRightGesture(Body body)
        {
            if (body == null)
            {
                return;
            }
            if (isRightGestureBeingTracked(body))
            {
                float currentDistance = GetJointDistance(body.Joints[JointType.HandRight], body.Joints[JointType.HandLeft]);
                if (currentDistance < 0.1f && previousDisance > 0.1f)
                {
                    GestureRecognized?.Invoke(this, new GestureEventArgs(GestureRecognitionResult.Success));
                }
                previousDisance = currentDistance;
            }
        }

        private float GetJointDistance(Joint firstJoint, Joint secondJoint)
        {
            float distX = firstJoint.Position.X - secondJoint.Position.X;
            float distY = firstJoint.Position.Y - secondJoint.Position.Y;
            float distZ = firstJoint.Position.Z - secondJoint.Position.Z;

            return (float)Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2) + Math.Pow(distZ, 2));
        }

        private void MatchSwipeLeftGesture(Body body)
        {
            throw new NotImplementedException();
        }
    }

    
}
