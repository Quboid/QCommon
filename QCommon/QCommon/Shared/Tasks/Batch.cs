using System;
using System.Collections.Generic;
using UnityEngine;

namespace QCommonLib.QTasks
{
    internal class QBatch
    {
        internal const int MAX_LIFE_LIMIT = 300; // Batch execution maximum life ceiling
        internal const int PER_TASK = 2; // Max time per task
        internal const int MIN_LIFE_LIMIT = QTask.MAX_LIFE + 5; // Batch execution max life floor 
        private readonly int maxLife;
        private QTimer Timer { get; set; } = null;

        private readonly QLogger Log;

        internal List<QTask> Tasks;
        internal int Size => Tasks.Count;
        internal string Name;
        internal Queues Queue;

        /// <summary>
        /// The batch's status
        /// </summary>
        internal Statuses Status { get => _status; set => _status = value; }
        private Statuses _status;

        internal QBatch(List<QTask> tasks, QLogger log, string name, Queues queue = Queues.Main)
        {
            Tasks = tasks;

            Status = Statuses.Start;
            Name = name;
            Log = log;

            maxLife = Mathf.Clamp(Size * PER_TASK, MIN_LIFE_LIMIT, MAX_LIFE_LIMIT) / 5;
            Queue = queue;
        }

        internal void Update()
        {
            if (Timer == null)
            {
                Timer = new QTimer();
            }
            else if (Timer.Seconds > maxLife)
            {
                foreach (QTask t in Tasks)
                {
                    t.Status = QTask.Statuses.Finished;
                }
                Status = Statuses.Finished;
                return;
            }

            try
            {
                switch (Status)
                {
                    case Statuses.Start:
                        if (Size > 0)
                        {
                            foreach (QTask t in Tasks)
                            {
                                t.Execute();
                            }
                            Status = Statuses.Processing;
                        }
                        else
                        {
                            Status = Statuses.Finished;
                        }
                        break;

                    case Statuses.Processing:
                        bool complete = true;
                        List<QTask> newTasks = new List<QTask>();
                        foreach (QTask t in Tasks)
                        {
                            if (t.Status != QTask.Statuses.Finished)
                            {
                                newTasks.Add(t);
                                complete = false;

                                if (t.Status == QTask.Statuses.Waiting)
                                { // Any task that is Waiting has executed, but due to execute again
                                    t.Execute();
                                }
                            }
                        }
                        Tasks = newTasks;
                        if (complete) Status = Statuses.Finished;
                        break;

                    default:
                        Log.Error($"Batch has invalid status ({Status})!");
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        internal enum Statuses
        {
            Start,
            Processing,
            Finished
        }

        internal enum Queues
        {
            Main,
            Final
        }
    }
}
