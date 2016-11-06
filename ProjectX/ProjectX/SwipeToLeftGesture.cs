using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPreview.Kinect;

namespace ProjectX
{
    public class SwipeToLeftGesture : GestureBase
    {
        public SwipeToLeftGesture() : base(GestureType.SwipeLeftGesture)
        {
        }

        private CameraSpacePoint validatePosition;
        private CameraSpacePoint startingPosition;
        
        private float shoulderDiff;


        protected override bool IsGestureValid(Body body)
        {
            var currentHandRightPoisition = body.Joints[JointType.HandRight].Position;

            if (validatePosition.X < currentHandRightPoisition.X)
            {
                return false;
            }
            validatePosition = currentHandRightPoisition;
            return true;
        }

        protected override bool ValidateBaseCondition(Body body)
        {
            var handRightPoisition = body.Joints[JointType.HandRight].Position;
            var handLeftPosition = body.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = body.Joints[JointType.ShoulderRight].Position;
            var spinePosition = body.Joints[JointType.SpineMid].Position;

            if ((handRightPoisition.Y < shoulderRightPosition.Y) &&
                 (handRightPoisition.Y > body.Joints[JointType.ElbowRight].Position.Y) && 
                 (handLeftPosition.Y < spinePosition.Y))
            {
                return true;
            }
            return false;
        }

        protected override bool ValidateGestureEndCondition(Body body)
        {
            double distance = Math.Abs(startingPosition.X - validatePosition.X);
            float currentshoulderDiff = GestureHelper.GetJointDistance(body.Joints[JointType.HandRight],
                                        body.Joints[JointType.ShoulderLeft]);
            if (distance > 0.1 && currentshoulderDiff < shoulderDiff)
            {
                return true;
            }          
            return false;
        }

        protected override bool ValidateGestureStartCondition(Body body)
        {
            var handRightPoisition = body.Joints[JointType.HandRight].Position;
            var handLeftPosition = body.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = body.Joints[JointType.ShoulderRight].Position;
            var spinePosition = body.Joints[JointType.SpineMid].Position;
                       
            if ((handRightPoisition.Y < shoulderRightPosition.Y) &&
                 (handRightPoisition.Y > body.Joints[JointType.ElbowRight].Position.Y) &&
                 handLeftPosition.Y < spinePosition.Y)
            {
                shoulderDiff = GestureHelper.GetJointDistance(body.Joints[JointType.HandRight],
                                                                body.Joints[JointType.ShoulderLeft]);
                validatePosition = body.Joints[JointType.HandRight].Position;
                startingPosition = body.Joints[JointType.HandRight].Position;
                return true;
            }
            return false;
        }
    }
}
