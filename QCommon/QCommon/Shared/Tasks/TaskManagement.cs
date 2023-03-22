using ColossalFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace QCommonLib.QTasks
{
    internal class QTaskManager : MonoBehaviour
    {
        internal QLogger Log = null;
        private Queue<QBatch> MainQueue;
        private Stack<QBatch> FinalQueue;
        private QBatch Current;
        public bool active = false;

        internal void Start()
        {
            MainQueue = new Queue<QBatch>();
            FinalQueue = new Stack<QBatch>();
        }

        internal bool Active => Current != null;
        internal bool Empty => Current == null && (MainQueue == null || MainQueue.Count == 0) && (FinalQueue == null || FinalQueue.Count == 0);

        /// <summary>
        /// MonoBehaviour method that runs on each tick, and is called when batches are added
        /// </summary>
        internal void Update()
        {
            if (Empty) { active = false; return; } // No batches exist

            if (!active) StartCoroutine(UpdateImplementation());
        }

        private IEnumerator UpdateImplementation()
        {
            QTimer timer = new QTimer();
            active = true;
            uint counter = 0;
            do
            {
                if (Empty) { active = false; yield break; } // No batches exist

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
                        active = false;
                        yield break;
                    }

                    //DebugBatchQueues();
                }

                Current.Update();

                if (counter++ > 1_000_000_000)
                {
                    Log.Info($"Aborting TaskManager loop due to counter exceeding maximum", "[Q08]");
                    active = false;
                    yield break;
                }

                //yield return new WaitForSeconds(0.005f);
                if (timer.MS > 10) yield return null;
            }
            while (true);
        }

        private void DebugBatchQueues(bool extended = false)
        {
            if (!Log.IsDebug) return;

            StringBuilder sb = new StringBuilder($"Task Manager Queues ({(MainQueue == null ? "<null>" : MainQueue.Count.ToString())}+{(FinalQueue == null ? "<null>" : FinalQueue.Count.ToString())}) [{QCommon.GetThreadName()}]");
            if (Current != null) sb.Append($" - Current: {(Current.Queue == QBatch.Queues.Final ? "F-" : "M-")}{Current.Name}:{Current.Size}");
            if (extended)
            {
                if (MainQueue != null && MainQueue.Count > 0)
                {
                    sb.Append(Environment.NewLine + "  ");
                    foreach (QBatch b in MainQueue) sb.Append($"M-{b.Name}:{b.Size}, ");
                }
                if (FinalQueue != null && FinalQueue.Count > 0)
                {
                    sb.Append(Environment.NewLine + "  ");
                    foreach (QBatch b in FinalQueue) sb.Append($"F-{b.Name}:{b.Size}, ");
                }
            }
            Log.Debug(sb.ToString());
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

            QueueOnMain(() => Update());
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


        /// <summary>
        /// Queue code on the Simulation thread
        /// </summary>
        /// <param name="action">The code to execute</param>
        public static void QueueOnSimulation(Action action)
        {
            Singleton<SimulationManager>.instance.AddAction(action);
        }
        /// <summary>
        /// Queue code on the Main thread
        /// </summary>
        public static void QueueOnMain(Action action)
        {
            Singleton<SimulationManager>.instance.m_ThreadingWrapper.QueueMainThread(action);
        }
    }
}
