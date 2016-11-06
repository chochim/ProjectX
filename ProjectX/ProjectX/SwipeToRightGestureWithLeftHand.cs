using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPreview.Kinect;

namespace ProjectX
{
    class SwipeToRightGestureWithLeftHand: GestureBase
    {
        public SwipeToRightGestureWithLeftHand() : base(GestureType.SwipeRightGestureWithLeftHand)
        {
        }

        private CameraSpacePoint validatePosition;
        private CameraSpacePoint startingPosition;

        private float shoulderDiff;


        protected override bool IsGestureValid(Body body)
        {
            var currentHandLeftPoisition = body.Joints[JointType.HandLeft].Position;

            if (validatePosition.X > currentHandLeftPoisition.X)
            {
                return false;
            }
            validatePosition = currentHandLeftPoisition;
            return true;
        }

        protected override bool ValidateBaseCondition(Body body)
        {
            var handLeftPosition = body.Joints[JointType.HandLeft].Position;
            var handRightPoisition = body.Joints[JointType.HandRight].Position;
            var shoulderLeftPosition = body.Joints[JointType.ShoulderLeft].Position;
            var spinePosition = body.Joints[JointType.SpineMid].Position;

            if ((handLeftPosition.Y < shoulderLeftPosition.Y) &&
                 (handLeftPosition.Y > body.Joints[JointType.ElbowLeft].Position.Y) &&
                 (handRightPoisition.Y < spinePosition.Y))
            {
                return true;
            }
            return false;
        }

        protected override bool ValidateGestureEndCondition(Body body)
        {
            double distance = Math.Abs(startingPosition.X - validatePosition.X);
            float currentshoulderDiff = GestureHelper.GetJointDistance(body.Joints[JointType.HandLeft],
                                        body.Joints[JointType.ShoulderRight]);
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
            var shoulderLeftPosition = body.Joints[JointType.ShoulderLeft].Position;
            var spinePosition = body.Joints[JointType.SpineMid].Position;

            if ((handLeftPosition.Y < shoulderLeftPosition.Y) &&
                 (handLeftPosition.Y > body.Joints[JointType.ElbowLeft].Position.Y) &&
                 handRightPoisition.Y < spinePosition.Y)
            {
                shoulderDiff = GestureHelper.GetJointDistance(body.Joints[JointType.HandLeft],
                                                                body.Joints[JointType.ShoulderRight]);
                validatePosition = body.Joints[JointType.HandLeft].Position;
                startingPosition = body.Joints[JointType.HandLeft].Position;
                return true;
            }
            return false;
        }
    }
}
