using System.Collections.Generic;
using UnityEngine;

namespace QCommonLib.QTasks
{
    internal class QTaskManager : MonoBehaviour
    {
        internal QLogger Log;
        private Queue<QBatch> MainQueue;
        private Stack<QBatch> FinalQueue;
        private QBatch Current;

        internal void Start()
        {
            MainQueue = new Queue<QBatch>();
            FinalQueue = new Stack<QBatch>();
        }

        internal bool Active => Current != null;

        /// <summary>
        /// MonoBehaviour method that runs on each tick
        /// </summary>
        internal void Update()
        {
            if (Current == null && (MainQueue == null || MainQueue.Count == 0) && (FinalQueue == null || FinalQueue.Count == 0)) return; // No batches exist

            if (Current != null)
            {
                if (Current.Status == QBatch.Statuses.Finished)
                {
                    Current = null;
                }
            }

            // If no current batch is loaded, grab the next one
            if (Current == null)
            {
                if (MainQueue.Count > 0)
                { // Get next batch
                    Current = MainQueue.Dequeue();
                }
                else if (FinalQueue.Count > 0)
                { // If no MainQueue entries remain, get from Final queue
                    Current = FinalQueue.Pop();
                }
                else
                { // Finished the queue
                    return;
                }

                // DebugBatchQueues();
            }

            Current.Update();
        }

        private void DebugBatchQueues()
        {
            if (!Log.IsDebug) return;

            string msg = $"TaskManager Queues";
            if (Current != null) msg += $" - Current: {(Current.Queue == QBatch.Queues.Final ? "T-" : "M-")}{Current.Name}:{Current.Size}";
            if (MainQueue != null && MainQueue.Count > 0)
            {
                msg += "\n  ";
                foreach (QBatch b in MainQueue) msg += $"M-{b.Name}:{b.Size}, ";
            }
            if (FinalQueue != null && FinalQueue.Count > 0)
            {
                msg += "\n  ";
                foreach (QBatch b in FinalQueue) msg += $"T-{b.Name}:{b.Size}, ";
            }
            if ((MainQueue != null && MainQueue.Count > 0) || (FinalQueue != null && FinalQueue.Count > 0))
                msg = msg.Substring(0, msg.Length - 2);
            Log.Debug(msg);
        }

        /// <summary>
        /// Add a new batch to the appropriate queue, if batch has tasks
        /// </summary>
        /// <param name="batch">The batch to add to the end of the queue</param>
        internal void EnqueueBatch(QBatch batch)
        {
            if (batch.Size == 0) return;

            if (batch.Queue == QBatch.Queues.Main)
            {
                if (MainQueue == null) MainQueue = new Queue<QBatch>();
                MainQueue.Enqueue(batch);
            }
            else if (batch.Queue == QBatch.Queues.Final)
            {
                if (FinalQueue == null) FinalQueue = new Stack<QBatch>();
                FinalQueue.Push(batch);
            }
        }

        /// <summary>
        /// Create and queue a new batch with a single task
        /// </summary>
        /// <param name="task">The Task instance to add</param>
        /// <param name="name">Optional name for logging</param>
        /// <param name="queue">Which list, main (default) or final?</param>
        internal void AddSingleTask(QTask task, string name = "", QBatch.Queues queue = QBatch.Queues.Main)
        {
            EnqueueBatch(CreateBatch(new List<QTask> { task }, name, queue));
        }

        /// <summary>
        /// Create and queue a new batch with a single newly created task
        /// </summary>
        /// <param name="thread"></param>
        /// <param name="codeBlock"></param>
        /// <param name="name">Optional name for logging</param>
        /// <param name="queue">Which list, main (default) or final?</param>
        internal void AddSingleTask(QTask.Threads thread, QTask.DCodeBlock codeBlock, string name = "", QBatch.Queues queue = QBatch.Queues.Main)
        {
            EnqueueBatch(CreateBatch(new List<QTask> { CreateTask(thread, codeBlock) }, name, queue));
        }

        /// <summary>
        /// Create and queue a new batch
        /// </summary>
        /// <param name="tasks">Main task list</param>
        /// <param name="name">Optional name for logging</param>
        /// <param name="queue">Which list, main (default) or final?</param>
        internal void AddBatch(List<QTask> tasks, string name, QBatch.Queues queue = QBatch.Queues.Main)
        {
            EnqueueBatch(CreateBatch(tasks, name, queue));
        }

        /// <summary>
        /// Create a new batch
        /// </summary>
        /// <param name="tasks">Main task list</param>
        /// <param name="name">Optional name for logging</param>
        /// <param name="queue">Which list, main (default) or final?</param>
        /// <returns>QBatch instance</returns>
        internal QBatch CreateBatch(List<QTask> tasks, string name, QBatch.Queues queue = QBatch.Queues.Main)
        {
            return new QBatch(tasks, Log, name, queue);
        }

        /// <summary>
        /// Create a new task
        /// </summary>
        /// <param name="thread"></param>
        /// <param name="codeBlock"></param>
        /// <returns>QTask instance</returns>
        internal QTask CreateTask(QTask.Threads thread, QTask.DCodeBlock codeBlock)
        {
            return new QTask(thread, codeBlock, Log);
        }
    }
}
