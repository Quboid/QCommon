using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCommonLib.QTasks
{
    internal class QTask
    {
        private readonly QLogger Log;

        /// <summary>
        /// The thread to execute the codeblock on
        /// </summary>
        internal Threads Thread { get => _thread; set => _thread = value; }
        private Threads _thread;

        /// <summary>
        /// The task's status
        /// </summary>
        internal Statuses Status { get => _status; set => _status = value; }
        private Statuses _status;

        /// <summary>
        /// The code to execute in Thread
        /// </summary>
        internal DCodeBlock CodeBlock { get => _codeBlock; set => _codeBlock = value; }
        private DCodeBlock _codeBlock;
        internal delegate void DCodeBlock();

        //internal Task(Action action, Threads thread, DCodeBlock codeBlock)
        internal QTask(Threads thread, DCodeBlock codeBlock, QLogger log)
        {
            CodeBlock = codeBlock;
            Thread = thread;
            Status = Statuses.Waiting;
            Log = log;
        }

        private static object lockSim = new object(), lockMain = new object();

        /// <summary>
        /// Execute the task, then run Finish method
        /// </summary>
        /// <returns>Did the task run successfully?</returns>
        internal bool Execute()
        {
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
                                    CodeBlock();
                                    this.Finish();
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
                                    CodeBlock();
                                    this.Finish();
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
            None,
            Waiting,
            Processing,
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
