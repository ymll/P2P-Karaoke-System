using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    public interface Job
    {
        void RunRepeatly(ManualResetEventSlim stopSignal, ManualResetEventSlim continueSignal);
        void CleanUp();
    }
}
