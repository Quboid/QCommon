using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCommonLib.QTasks
{
    internal class QTask
    {
        private const int MAX_LIFE = 10; // Task can last up to 10 seconds
        private QTimer Timer { get; set; } = null;

        private readonly QLogger Log;

        /// <summary>
        /// The thread to execute the codeblock on
        /// </summary>
        internal Threads Thread { get; set; }

        /// <summary>
        /// The task's status
        /// </summary>
        internal Statuses Status { get; set; }

        /// <summary>
        /// The code to execute in Thread
        /// </summary>
        /// <returns>Has the code completed?</returns>
        internal DCodeBlock CodeBlock { get; set; }
        internal delegate bool DCodeBlock();

        //internal Task(Action action, Threads thread, DCodeBlock codeBlock)
        internal QTask(Threads thread, DCodeBlock codeBlock, QLogger log)
        {
            CodeBlock = codeBlock;
            Thread = thread;
            Status = Statuses.Waiting;
            Log = log;
        }

        private static readonly object lockSim = new object();
        private static readonly object lockMain = new object();

        /// <summary>
        /// Execute the task, then run Finish method
        /// </summary>
        /// <returns>Did the task run successfully?</returns>
        internal bool Execute()
        {
            if (Timer == null)
            {
                Timer = new QTimer();
            }
            else if (Timer.Seconds > MAX_LIFE)
            {
                Log.Warning($"Task reached EOL, terminating.", "[Q07]");
                Status = Statuses.Finished;
                return true;
            }

            try
            {
                Status = Statuses.Processing;

                switch (Thread)
                {
                    case Threads.Simulation:
                        Singleton<SimulationManager>.instance.AddAction(() =>
                        {
                            try
                            {
                                lock (lockSim)
                                {
                                    if (CodeBlock()) Finish();
                                    else Status = Statuses.Waiting;
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                            }
                        });
                        break;

                    case Threads.Main:
                        Singleton<SimulationManager>.instance.m_ThreadingWrapper.QueueMainThread(() =>
                        {
                            try
                            {
                                lock (lockMain)
                                {
                                    if (CodeBlock()) Finish();
                                    else Status = Statuses.Waiting;
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                            }
                        });
                        break;

                    default:
                        Log.Error($"Task called invalid thread!", "[MI77]");
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called immediately after CodeBlock has executed, on same thread
        /// </summary>
        internal void Finish()
        {
            Status = Statuses.Finished;
        }

        internal enum Statuses
        {
            /// <summary>
            /// Something has gone wrong
            /// </summary>
            None,
            /// <summary>
            /// Task is ready to (re)start
            /// </summary>
            Waiting,
            /// <summary>
            /// Task is executing
            /// </summary>
            Processing,
            /// <summary>
            /// Task has completed
            /// </summary>
            Finished
        }

        internal enum Threads
        {
            None,
            Main,
            Simulation
        }
    }
}
