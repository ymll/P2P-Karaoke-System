using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    class Util
    {
        public static void ThrowExceptionWhenResultNotZero(string errorMessage, int result)
        {
            if (result != 0)
            {
                throw new Exception(errorMessage);
            }
        }
    }
}
