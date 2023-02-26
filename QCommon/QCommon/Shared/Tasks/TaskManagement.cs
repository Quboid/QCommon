using System.Collections.Generic;
using UnityEngine;

namespace QCommonLib.QTasks
{
    internal class QTaskManager : MonoBehaviour
    {
        internal QLogger Log;
        private Queue<QBatch> Batches;
        private QBatch Current;

        internal void Start()
        {
            Batches = new Queue<QBatch>();
        }

        internal bool Active => Current != null;

        /// <summary>
        /// MonoBehaviour method that runs on each tick
        /// </summary>
        internal void Update()
        {
            if (Batches == null || Batches.Count == 0) return; // No tasks exist

            if (Current != null)
            {
                if (Current.Status == QBatch.Statuses.Finished)
                {
                    Batches.Dequeue();
                    Current = null;
                }
            }

            // If no current batch is loaded, grab the next one
            if (Current == null)
            {
                if (Batches.Count > 0)
                { // Get next batch
                    Current = Batches.Peek();
                    //Log.Debug($"BBB02 {Current.Name}:{Current.Size}");
                }
                else
                { // Finished the queue
                    return;
                }
            }

            Current.Update();
        }

        /// <summary>
        /// Add a new batch to the queue, if batch has tasks and/or prefix/postfix
        /// </summary>
        /// <param name="batch">The batch to add to the end of the queue</param>
        internal void EnqueueBatch(QBatch batch)
        {
            //Log.Debug($"BBB01 Enqueuing {batch.Name}:{batch.Size} (ignore:{(batch.Size == 0 && batch.Prefix == null && batch.Postfix == null)})");
            if (batch.Size == 0 && batch.Prefix == null && batch.Postfix == null) return;

            if (Batches == null) Batches = new Queue<QBatch>();
            Batches.Enqueue(batch);
        }

        /// <summary>
        /// Create and queue a new batch with a single task
        /// </summary>
        /// <param name="task">The Task instance to add</param>
        /// <param name="name">Optional name for logging</param>
        internal void AddSingleTask(QTask task, string name = "")
        {
            EnqueueBatch(CreateBatch(new List<QTask> { task }, null, null, name));
        }

        /// <summary>
        /// Create and queue a new batch with a single newly created task
        /// </summary>
        /// <param name="thread"></param>
        /// <param name="codeBlock"></param>
        /// <param name="name">Optional name for logging</param>
        internal void AddSingleTask(QTask.Threads thread, QTask.DCodeBlock codeBlock, string name = "")
        {
            EnqueueBatch(CreateBatch(new List<QTask> { CreateTask(thread, codeBlock) }, null, null, name));
        }

        /// <summary>
        /// Create and queue a new batch
        /// </summary>
        /// <param name="tasks">Main task list</param>
        /// <param name="prefix">Task to run to completion before main task list has started</param>
        /// <param name="postfix">Task to run after main task list has finished</param>
        /// <param name="name">Optional name for logging</param>
        internal void AddBatch(List<QTask> tasks, QTask prefix, QTask postfix, string name)
        {
            EnqueueBatch(CreateBatch(tasks, prefix, postfix, name));
        }

        /// <summary>
        /// Create a new batch
        /// </summary>
        /// <param name="tasks">Main task list</param>
        /// <param name="prefix">Task to run to completion before main task list has started</param>
        /// <param name="postfix">Task to run after main task list has finished</param>
        /// <param name="name">Optional name for logging</param>
        /// <returns>QBatch instance</returns>
        internal QBatch CreateBatch(List<QTask> tasks, QTask prefix, QTask postfix, string name)
        {
            return new QBatch(tasks, prefix, postfix, Log, name);
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
