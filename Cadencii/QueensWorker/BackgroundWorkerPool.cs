using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace cadencii
{
    class BackgroundWorkerParallel
    {
        public bool WorkerReportsProgress = false;
        public delegate void RunWorkerCompletedEventHandlerParallel(int JobID, object sender, RunWorkerCompletedEventArgs e);
        public delegate void AllWorkerCompletedEventHandlerParallel(object sender, RunWorkerCompletedEventArgs e);
        public delegate void ProgressChangedEventHandlerParallel(int JobID, object sender, ProgressChangedEventArgs e);
        public DoWorkEventHandler DoWork = null;
        public RunWorkerCompletedEventHandlerParallel RunWorkerCompleted = null;
        public AllWorkerCompletedEventHandlerParallel AllWorkerCompleted = null;
        public ProgressChangedEventHandlerParallel ProgressChanged = null;
        private List<int> IDTable = new List<int>();
        private List<FormWorkerJobArgument> ArgsTable = new List<FormWorkerJobArgument>();
        private List<WorkerState> StateTable = new List<WorkerState>();
        public void AddWork(int ID, FormWorkerJobArgument jobarg)
        {
            IDTable.Add(ID);
            ArgsTable.Add(jobarg);
            StateTable.Add(jobarg.state);
        }

        public int WorkCount
        {
            get
            {
                return IDTable.Count;
            }
        }

        public void StartAllWork()
        {
            cadencii.QueensWorker.ActionQueues AQ = new QueensWorker.ActionQueues(IDTable.Count, new Func<int, QueensWorker.QueueThreadBase<int>.DoWorkResult>(AEvent));
            AQ.OneCompleted += new Action<int, QueensWorker.QueueThreadBase<int>.CompetedEventArgs>(AQ_OneCompleted);
            AQ.AllCompleted += new Action<QueensWorker.QueueThreadBase<int>.CompetedEventArgs>(AQ_AllCompleted);
            AQ.Start();
        }

        void AQ_AllCompleted(QueensWorker.QueueThreadBase<int>.CompetedEventArgs obj)
        {
            object Result = null;
            bool isCancel = false;
            if (obj.CompetedPrecent != 100)
            {
                isCancel = true;
            }
            RunWorkerCompletedEventArgs e = new RunWorkerCompletedEventArgs(Result, obj.InnerException, isCancel);
            AllWorkerCompleted(this, e);
        }

        void AQ_OneCompleted(int arg1, QueensWorker.QueueThreadBase<int>.CompetedEventArgs arg2)
        {
            object Result = null;
            bool isCancel = false;
            if (arg2.CompetedPrecent != 100)
            {
                isCancel = true;
            }
            RunWorkerCompletedEventArgs e = new RunWorkerCompletedEventArgs(Result,arg2.InnerException,isCancel);
            RunWorkerCompleted(arg1, this, e);
        }

        public cadencii.QueensWorker.QueueThreadBase<int>.DoWorkResult AEvent(int ID)
        {
            cadencii.QueensWorker.QueueThreadBase<int>.DoWorkResult Ret = cadencii.QueensWorker.QueueThreadBase<int>.DoWorkResult.ContinueThread; 
            DoWorkEventArgs e = new DoWorkEventArgs(ArgsTable[ID]);
            /*Thread Th = new Thread(new ParameterizedThreadStart((o)=>{
                object[] oo = (object[])o;
                DoWork(oo[0], (DoWorkEventArgs)oo[1]);
            }));
            Th.Start(new object[2] { this, e });*/
            Console.WriteLine("STT:" + ID.ToString());
            DoWork(this, e);
            Console.WriteLine("FINI:" + ID.ToString());
            return Ret;
        }

        public void ReportProgress(WorkerState Job, int Perc, object UserState)
        {
            if (ProgressChanged != null)
            {
                ProgressChangedEventArgs e = new ProgressChangedEventArgs(Perc, UserState);
                //int id = WorkJob[Job];
                int idx = StateTable.IndexOf(Job);
                int id = IDTable[idx];
                ProgressChanged(id, this, e);
            }
        }
        public void ReportProgress(WorkerState Job, int Perc)
        {
            ReportProgress(Job, Perc, null);
        }
    }
    
}
