using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PKaraokeSystem.PlaybackLogic
{
    public class JobThread
    {
        private readonly ManualResetEventSlim StopSignal;
        private readonly ManualResetEventSlim ContinueSignal;

        private volatile bool IsStarted;
        private Thread Thread;

        public JobThread(string jobName, Job job)
            : this(jobName, job, null, null)
        {

        }

        public JobThread(string jobName, Job job, ManualResetEventSlim stopSignal, ManualResetEventSlim continueSignal)
        {
            if (stopSignal == null)
            {
                this.StopSignal = new ManualResetEventSlim(false);
            }
            else
            {
                this.StopSignal = stopSignal;
            }
            this.ContinueSignal = continueSignal;

            if (continueSignal == null)
            {
                this.ContinueSignal = new ManualResetEventSlim(true);
            }
            else
            {
                this.ContinueSignal = continueSignal;
            }

            this.Thread = new Thread(() =>
            {
                while (!StopSignal.Wait(0))
                {
                    ContinueSignal.Wait();
                    job.RunRepeatly(StopSignal, ContinueSignal);
                }
                job.CleanUp();

                IsStarted = false;
                StopSignal.Reset();
                ContinueSignal.Set();
            });
            this.Thread.Name = jobName;
        }

        public void Start()
        {
            if (IsStarted)
            {
                throw new Exception("The job is running already");
            }

            IsStarted = true;
            Thread.Start();
        }
    }
}
