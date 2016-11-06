using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPreview.Kinect;

namespace ProjectX
{
    public abstract class GestureBase
    {
        public GestureBase(GestureType type)
        {
            this.CurrentFrameCount = 0;
            this.GestureType = type;
        }

        public Boolean IsRecognizedStarted { get; set; }

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

        protected abstract Boolean ValidateGestureStartCondition(Body body);

        protected abstract Boolean ValidateGestureEndCondition(Body body);

        protected abstract Boolean ValidateBaseCondition(Body body);

        protected abstract Boolean IsGestureValid(Body body);

        public virtual Boolean CheckForGesture(Body body)
        {
            if (this.IsRecognizedStarted == false)
            {
                if (this.ValidateGestureStartCondition(body))
                {
                    this.IsRecognizedStarted = true;
                    this.CurrentFrameCount = 0;
                }
            }
            else
            {
                if (this.CurrentFrameCount == this.MaximumNumberOfFrameToProcess)
                {
                    this.IsRecognizedStarted = false;
                    if (ValidateBaseCondition(body) && ValidateGestureEndCondition(body))
                    {
                        return true;
                    }

                    this.CurrentFrameCount++;

                    if (!IsGestureValid(body) && !ValidateBaseCondition(body))
                    {
                        this.IsRecognizedStarted = false;
                    }
                }
            }
            return false;
        }
    }
}
