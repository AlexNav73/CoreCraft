using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PricingCalc.Model.Engine.Commands
{
    internal class SequentialTaskScheduler : TaskScheduler
    {
        public static readonly SequentialTaskScheduler Instance = new SequentialTaskScheduler();

        private readonly Queue<Task> _queue;
        private readonly object _mutex = new object();

        private SequentialTaskScheduler()
        {
            _queue = new Queue<Task>();
        }

        protected override void QueueTask(Task task)
        {
            if (task is not null)
            {
                lock (_queue)
                {
                    _queue.Enqueue(task);
                }
                ThreadPool.QueueUserWorkItem(Execute, null);
            }
        }

        private void Execute(object? state)
        {
            // TODO: Yield thread if it can't lock on mutex
            lock (_mutex)
            {
                Task? task = null;

                lock (_queue)
                {
                    if (_queue.Count > 0)
                        task = _queue.Dequeue();
                }

                if (task != null)
                {
                    TryExecuteTask(task);
                }
            }
        }

        protected override IEnumerable<Task>? GetScheduledTasks()
        {
            lock (_queue)
            {
                return _queue.ToArray();
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }
    }
}
