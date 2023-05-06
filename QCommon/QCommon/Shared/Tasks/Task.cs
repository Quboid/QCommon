using ColossalFramework;
using System;

namespace QCommonLib.QTasks
{
    internal class QTask
    {
        internal const int MAX_LIFE = 20; // Task execution loop can last up to 20 seconds
        private QTimer Timer { get; set; } = null;

        private readonly QLogger Log;

        /// <summary>
        /// The thread to execute the codeblock on
        /// </summary>
        internal Threads Thread { get; set; }

        /// <summary>
        /// The task's status - written to on Main thread only
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
                Status = Statuses.Finished;
                return true;
            }

            try
            {
                Status = Statuses.Processing;
                ThreadExecute(Thread, RunCodeBlock);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
            return true;
        }

        private void RunCodeBlock()
        {
            try
            {
                if (CodeBlock())
                {
                    Finish();
                }
                else
                {
                    ReQueue();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "[Q09]");
            }
        }

        /// <summary>
        /// Called on main thread immediately after CodeBlock has executed
        /// </summary>
        internal void ReQueue()
        {
            Status = Statuses.Waiting;
        }

        /// <summary>
        /// Called on main thread immediately after CodeBlock has executed
        /// </summary>
        internal void Finish()
        {
            Status = Statuses.Finished;
        }

        internal enum Statuses
        {
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


        /// <summary>
        /// Run the action's code in the specified QTask.Thread
        /// </summary>
        /// <param name="thread">The <c ref="QTask.Threads">Thread</c> to execute the code on</param>
        /// <param name="action">The code to run</param>
        /// <exception cref="Exception">An invalid thread was passed</exception>
        internal static void ThreadExecute(Threads thread, Action action)
        {
            if (thread == Threads.Main)
            {
                QTaskManager.QueueOnMain(action);
            }
            else if (thread == Threads.Simulation)
            {
                QTaskManager.QueueOnSimulation(action);
            }
            else
            {
                throw new Exception($"Invalid thread for code execution [{thread}]");
            }
        }
    }
}
