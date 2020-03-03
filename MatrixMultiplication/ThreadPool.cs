using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
namespace MatrixMultiplication
{
    public delegate void WaitCallback();

    //This class is my thread pool implementation with the ability to change the pool size dynamically
    public class CustomThreadPool
    {
        private int m_ThreadPoolSize;
        private List <Thread> m_WorkerThreads = new List<Thread>(); //Holds all the worker threads in the thread pool
        private Queue<WaitCallback> m_TaskQueue = new Queue<WaitCallback>(); //Holds all available tasks in a queue form
        public AutoResetEvent DoneWorking = new AutoResetEvent(false);
        private int m_NumOfWorkingThreads; //Holds the number of currently active, non-idle threads

        //m_ThreadPoolSize property
        public int ThreadPoolSize
        {
            get
            {
                return m_ThreadPoolSize;
            }
        }
        //m_NumOfWorkingThreads Property
        public int NumOfWorkingThreads
        {
            get
            {
                return m_NumOfWorkingThreads;
            }
            set
            {
                m_NumOfWorkingThreads = value;
            }
        }

        //CurrentPoolSize Property
        public int CurrentPoolSize
        {
            get
            {
                return m_WorkerThreads.Count;
            }
        }      
        
        //This method sets the Thread pool size, creates the threads and starts them so they can start doing tasks
        public void SetThreadPoolSize(int i_PoolSize) // Changes the size of the thread pool so it can be exte
        {
            lock (m_WorkerThreads)
            {
                m_ThreadPoolSize = i_PoolSize;            //sets the poolsize with the paremeter sent
                if (m_ThreadPoolSize > m_WorkerThreads.Count)
                {
                    m_NumOfWorkingThreads = i_PoolSize;   //sets the number of active threads to the current pool size (initialization)
                    CreateThreads(); //Calls to CreateThreads so new threads will be made
                }
                else if (m_ThreadPoolSize < m_WorkerThreads.Count)
                {
                    lock (m_TaskQueue)
                    {
                        Monitor.PulseAll(m_TaskQueue); //Wakes up all threads so some of them can finish their tasks
                    }
                }
                // The thread pool size is already at the requested size and enough threads are alerady created
            }

            return;
        }

        //This method queues tasks and pulses the task queue so a worker thread is notified that there is a task available
        public void QueueTask(WaitCallback callBack)
        {

            lock (m_TaskQueue)
            {
                m_TaskQueue.Enqueue(callBack);
                Monitor.Pulse(m_TaskQueue);
            }

            return;
        }     

        //This method creates the threads for the thread pool
        private void CreateThreads()
        {
            int i = 0;
            while (m_WorkerThreads.Count < m_ThreadPoolSize)
            {
                Thread workerThread = new Thread(ExecuteTask);   //initializes the treak with the ExecuteTask method     
                m_WorkerThreads.Add(workerThread);
                workerThread.Start();  //The thread starts,executes the ExecuteTask method and waits for tasks to be available in the queue
                i++;
            }

            return;
        }

        //This method is where the thread fetches tasks from the Task Queue and executes them.If there are no tasks the thread becomes idle until a new one is available
        private void ExecuteTask()
        {
            WaitCallback task; //Holds the current task to be executed by the current thread
            while (true)
            {
                if (CheckIfToRemoveThread()) //Checks if the thread is even needed before checking for available tasks - prevents waste of system resources via pointless waiting.
                    return;

                lock (m_TaskQueue)
                {
                    while (m_TaskQueue.Count == 0 && !(m_ThreadPoolSize < m_WorkerThreads.Count)) //Checks if there are no more available tasks and if the thread is still needed in the thread pool 
                    {
                        Interlocked.Decrement(ref m_NumOfWorkingThreads); //Thread is now idle so the Number of working threads is decremented by 1

                        //If the number of working threads is 0, all threads have completed all available tasks and are now idle.
                        if (m_NumOfWorkingThreads == 0) 
                        {
                            //Signals that all threads have completed their tasks,there are no more tasks in the queue and the threads are now idle
                            DoneWorking.Set();
                        }
                        //The current thread waits until a new task is available
                        Monitor.Wait(m_TaskQueue);
                        //The current has been signled that a new task is available and therefor the current thread is no longer idle and will attempt to fetch the task if the thread is still needed.
                        Interlocked.Increment(ref m_NumOfWorkingThreads);
                    }

                    if (CheckIfToRemoveThread()) //Checks in case the current thread isn't needed anymore (there are more threads than required).
                        return;
                    //The task is removed to the queue and saved into the currents thread task so it can be executed
                    task = m_TaskQueue.Dequeue();
                }

                task();  //Executes current task
            }
        }

        //This method checks if the current thread is needed in the thread pool or can it be removed from the worker list
        private bool CheckIfToRemoveThread()
        {
            if (m_ThreadPoolSize < m_WorkerThreads.Count) // Checks if the current thread pool size is smaller than the size set by the user
            {
                lock (m_WorkerThreads)
                {
                    if (m_ThreadPoolSize < m_WorkerThreads.Count) //Checks if the thread pool size changed after acquiring the lock
                    {
                        lock (m_WorkerThreads)
                        {
                            m_WorkerThreads.Remove(Thread.CurrentThread); //Removes the current thread from the worker list as its no longer needed
                        }
                        return true;
                    }
                }
            }
            return false;  //There was no need to remove a thread.
        }
    }
}

