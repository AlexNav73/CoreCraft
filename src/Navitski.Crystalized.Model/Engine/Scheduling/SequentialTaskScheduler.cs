using System.Diagnostics.CodeAnalysis;

namespace Navitski.Crystalized.Model.Engine.Scheduling;

[ExcludeFromCodeCoverage]
internal class SequentialTaskScheduler : TaskScheduler
{
    public static readonly SequentialTaskScheduler Instance = new SequentialTaskScheduler();

    [ThreadStatic]
    private static bool _currentThreadIsProcessingItems;

    private readonly LinkedList<Task> _queue;

    private int _runningParallelTasks = 0;

    private SequentialTaskScheduler()
    {
        _queue = new LinkedList<Task>();
    }

    protected override void QueueTask(Task task)
    {
        lock (_queue)
        {
            _queue.AddLast(task);
            if (_runningParallelTasks == 0)
            {
                _runningParallelTasks++;
                // NOTE: In the article about TaskScheduler in the example
                // UnsafeQueueUserWorkItem is used to speed up queuing task
                // by skipping thread restrictions checks. If a thread with
                // restrictions calls UnsafeQueueUserWorkItem, a new thread will
                // execute delegate under default restrictions. It is unsafe.
                // In future, plug-in system could be implemented and using unsafe
                // method could open a security hole. For now, performance is ok and
                // usage of unsafe method is not necessary.
                ThreadPool.QueueUserWorkItem(Execute, null);
            }
        }
    }

    private void Execute(object? state)
    {
        _currentThreadIsProcessingItems = true;
        try
        {
            while (true)
            {
                Task task;

                lock (_queue)
                {
                    if (_queue.Count == 0)
                    {
                        _runningParallelTasks--;
                        break;
                    }

                    task = _queue.First!.Value;
                    _queue.RemoveFirst();
                }

                TryExecuteTask(task);
            }
        }
        finally
        {
            _currentThreadIsProcessingItems = false;
        }
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        if (!_currentThreadIsProcessingItems)
        {
            return false;
        }

        if (taskWasPreviouslyQueued)
        {
            if (TryDequeue(task))
            {
                return TryExecuteTask(task);
            }

            return false;
        }

        return TryExecuteTask(task);
    }

    protected override IEnumerable<Task>? GetScheduledTasks()
    {
        var lockTaken = false;
        try
        {
            Monitor.TryEnter(_queue, ref lockTaken);
            if (lockTaken)
            {
                return _queue;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        finally
        {
            if (lockTaken)
            {
                Monitor.Exit(_queue);
            }
        }
    }

    protected sealed override bool TryDequeue(Task task)
    {
        lock (_queue)
        {
            return _queue.Remove(task);
        }
    }
}
