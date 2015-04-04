using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cadencii.QueensWorker
{
    public class ActionQueues : QueueThreadBase<int>
    {
        public ActionQueues(IEnumerable<int> QueensList, Func<int,DoWorkResult> Event)
            : base(QueensList)
        {
            DoAction = Event;
        }

        Func<int,DoWorkResult> DoAction = null;
        public ActionQueues(long TotalCount, Func<int,DoWorkResult> Event)
            : base(TotalCount)
        {
            DoAction = Event;
        }

        protected override DoWorkResult DoWork(int pendingID)
        {
            DoWorkResult isOK = DoAction(pendingID);
            return isOK;
        }

    }
}