using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace ProjectX
{
    class SwipeToRightGestureWithRightHand: GestureBase
    {
        public SwipeToRightGestureWithRightHand() : base(GestureType.SwipeRightGestureWithRightHand) { }

        private CameraSpacePoint validatePosition;
        private CameraSpacePoint startingPosition;

        private float shoulderDiff;

        /// <summary>
        /// Validates the gesture start condition.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns></returns>
        protected override bool ValidateGestureStartCondition(Body skeleton)
        {
            var handRightPoisition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var spinePosition = skeleton.Joints[JointType.SpineMid].Position;

            if ((handRightPoisition.Y < shoulderRightPosition.Y) &&
              (handRightPoisition.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
              && (handLeftPosition.Y < spinePosition.Y))
            {
                shoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandRight],
                                                                skeleton.Joints[JointType.ShoulderLeft]);
                validatePosition = skeleton.Joints[JointType.HandRight].Position;
                startingPosition = skeleton.Joints[JointType.HandRight].Position;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is gesture valid] [the specified skeleton data].
        /// </summary>
        /// <param name="skeleton">The skeleton data.</param>
        /// <returns>
        ///   <c>true</c> if [is gesture valid] [the specified skeleton data]; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsGestureValid(Body skeleton)
        {
            var currentHandRightPoisition = skeleton.Joints[JointType.HandRight].Position;
            if (validatePosition.X > currentHandRightPoisition.X)
            {
                return false;
            }
            validatePosition = currentHandRightPoisition;
            return true;
        }

        protected override bool ValidateGestureEndCondition(Body body)
        {
            double distance = Math.Abs(validatePosition.X - startingPosition.X);
            float currentshoulderDiff = GestureHelper.GetJointDistance(body.Joints[JointType.HandRight],
                                        body.Joints[JointType.ShoulderRight]);

            if (distance > 0.1 && currentshoulderDiff > shoulderDiff)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Valids the base condition.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns></returns>
        protected override bool ValidateBaseCondition(Body skeleton)
        {
            var handRightPoisition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var spinePosition = skeleton.Joints[JointType.SpineMid].Position;

            if (//(handRightPoisition.Y < shoulderRightPosition.Y) &&
                 (handRightPoisition.Y > skeleton.Joints[JointType.ElbowRight].Position.Y) &&
                 (handLeftPosition.Y < spinePosition.Y))
            {
                return true;
            }
            return false;
        }
    }
}
