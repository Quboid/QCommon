using System;
using System.Collections.Generic;
using UnityEngine;

namespace QCommonLib.QTasks
{
    internal class QBatch
    {
        internal const int MAX_LIFE = 300; // Batch execution maximum life ceiling
        internal const int PER_TASK = 2; // Max time per task
        internal const int MIN_LIFE = QTask.MAX_LIFE + 5; // Batch execution max life floor 
        private readonly int maxLife;
        private QTimer Timer { get; set; } = null;

        private readonly QLogger Log;

        internal List<QTask> Tasks;
        internal int Size => Tasks.Count;
        internal string Name;

        internal QTask Prefix = null;
        internal QTask Postfix = null;

        /// <summary>
        /// The batch's status
        /// </summary>
        internal Statuses Status { get => _status; set => _status = value; }
        private Statuses _status;

        internal QBatch(List<QTask> tasks, QTask prefix, QTask postfix, QLogger log, string name)
        {
            Tasks = tasks;
            Prefix = prefix;
            Postfix = postfix;

            Status = Statuses.Start;
            Name = name;
            Log = log;

            maxLife = Mathf.Clamp(Size * PER_TASK, MIN_LIFE, MAX_LIFE);
        }

        internal void Update()
        {
            if (Timer == null)
            {
                Timer = new QTimer();
            }
            else if (Timer.Seconds > maxLife)
            {
                Log.Warning($"Batch reached EOL, terminating.", "[Q07]");
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
                        if (Prefix != null)
                        {
                            Prefix.Execute();
                            Status = Statuses.Prefix;
                        }
                        else
                        {
                            Status = Statuses.Waiting;
                        }
                        break;

                    case Statuses.Prefix:
                        if (Prefix.Status == QTask.Statuses.Finished) Status = Statuses.Waiting;
                        break;

                    case Statuses.Waiting:
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
                            Status = Statuses.Processed;
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
                        if (complete) Status = Statuses.Processed;
                        break;

                    case Statuses.Processed:
                        if (Postfix != null)
                        {
                            Postfix?.Execute();
                            Status = Statuses.Postfix;
                        }
                        else
                        {
                            Status = Statuses.Finished;
                        }
                        break;

                    case Statuses.Postfix:
                        if (Postfix.Status == QTask.Statuses.Finished) Status = Statuses.Finished;
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
            None,
            Start,
            Prefix,
            Waiting,
            Processing,
            Processed,
            Postfix,
            Finished
        }
    }
}
