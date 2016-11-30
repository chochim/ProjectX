using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectX
{
    public abstract class GestureBase
    {
        public GestureBase(GestureType type)
        {
            this.CurrentFrameCount = 0;
            this.GestureType = type;
        }

        public bool IsRecognizedStarted { get; set; }

        private int CurrentFrameCount { get; set; }
        public GestureType GestureType { get; set; }

        protected virtual int MaximumNumberOfFrameToProcess
        {
            get
            {
                return 15;
            }
        }

        public long GestureTimeStamp { get; set; }

        protected abstract bool ValidateGestureStartCondition(Body body);

        protected abstract bool ValidateGestureEndCondition(Body body);

        protected abstract bool ValidateBaseCondition(Body body);

        protected abstract bool IsGestureValid(Body body);

        public virtual bool CheckForGesture(Body body)
        {
            if (IsRecognizedStarted == false)
            {
                if (ValidateGestureStartCondition(body))
                {
                    IsRecognizedStarted = true;
                    CurrentFrameCount = 0;
                }
            }
            else
            {
                if (CurrentFrameCount == MaximumNumberOfFrameToProcess)
                {
                    IsRecognizedStarted = false;
                    if (ValidateBaseCondition(body) && ValidateGestureEndCondition(body))
                    {
                        return true;
                    }
                }

                CurrentFrameCount++;

                if (!IsGestureValid(body) && !ValidateBaseCondition(body))
                {
                    IsRecognizedStarted = false;
                }
            }
            return false;
        }
    }
}
