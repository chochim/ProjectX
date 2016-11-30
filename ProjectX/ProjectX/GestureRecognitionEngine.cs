using Microsoft.Kinect;
using System;
using System.Collections.Generic;

namespace ProjectX
{
    class GestureRecognitionEngine
    {
        /// <summary>
        /// Skip Frames After Gesture IsDetected
        /// </summary>
        int SkipFramesAfterGestureIsDetected = 0;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is gesture detected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is gesture detected; otherwise, <c>false</c>.
        /// </value>
        public bool IsGestureDetected { get; set; }

        /// <summary>
        /// Collection of Gesture
        /// </summary>
        private List<GestureBase> gestureCollection = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureRecognitionEngine" /> class.
        /// </summary>
        public GestureRecognitionEngine()
        {
            this.InitilizeGesture();
        }

        /// <summary>
        /// Initilizes the gesture.
        /// </summary>
        private void InitilizeGesture()
        {
            this.gestureCollection = new List<GestureBase>();
            this.gestureCollection.Add(new SwipeToLeftGestureWithRightHand());
            this.gestureCollection.Add(new SwipeToRightGestureWithRightHand());
            this.gestureCollection.Add(new SwipeToLeftGestureWithLeftHand());
            this.gestureCollection.Add(new SwipeToRightGestureWithLeftHand());
        }

        /// <summary>
        /// Occurs when [gesture recognized].
        /// </summary>
        public event EventHandler<GestureEventArgs> GestureRecognized;

        /// <summary>
        /// Gets or sets the type of the gesture.
        /// </summary>
        /// <value>
        /// The type of the gesture.
        /// </value>
        public GestureType GestureType { get; set; }

        /// <summary>
        /// Gets or sets the skeleton.
        /// </summary>
        /// <value>
        /// The skeleton.
        /// </value>
        public Body Body { get; set; }

        /// <summary>
        /// Starts the recognize.
        /// </summary>
        public void StartRecognize()
        {
            if (IsGestureDetected)
            {
                while (SkipFramesAfterGestureIsDetected <= 30)
                {
                    SkipFramesAfterGestureIsDetected++;
                }
                this.RestGesture();
                return;
            }

            foreach (var item in gestureCollection)
            {
                if (item.CheckForGesture(this.Body))
                {
                    if (GestureRecognized != null)
                    {
                        GestureRecognized(this, new GestureEventArgs(GestureRecognitionResult.Success, item.GestureType));
                        IsGestureDetected = true;
                    }

                }
            }
        }

        /// <summary>
        /// Rests the gesture.
        /// </summary>
        private void RestGesture()
        {
            this.gestureCollection = null;
            this.InitilizeGesture();
            this.SkipFramesAfterGestureIsDetected = 0;
            this.IsGestureDetected = false;
        }
    }
}
