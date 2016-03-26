using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    class Util
    {
        public static void AssertZero(string errorMessage, int result)
        {
            AssertTrue(errorMessage, result == 0);
        }

        public static void AssertNonNegative(string errorMessage, int result)
        {
            AssertTrue(errorMessage, result >= 0);
        }

        public static void AssertFalse(string errorMessage, bool result)
        {
            AssertTrue(errorMessage, !result);
        }

        public static void AssertTrue(string errorMessage, bool result)
        {
            if (!result)
            {
                throw new ApplicationException(errorMessage);
            }
        }
    }
}
