using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectX
{
    class GestureEventArgs: EventArgs
    {
        public GestureRecognitionResult Result
        {
            get; internal set;
        }
            
        public GestureEventArgs(GestureRecognitionResult result)
        {
            this.Result = result;
        }
    }
}
