using System;

namespace ProjectX
{
    class GestureEventArgs: EventArgs
    {
        public GestureRecognitionResult Result
        {
            get; internal set;
        }

        public GestureType GestureType
        {
            get; internal set;
        }

        public GestureEventArgs(GestureRecognitionResult result, GestureType type)
        {
            this.Result = result;
            this.GestureType = type;
        }
    }
}
