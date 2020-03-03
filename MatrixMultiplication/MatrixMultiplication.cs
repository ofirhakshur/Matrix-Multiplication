using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MatrixMultiplication
{
    class MatrixMultiplication
    {
        public void run()
        {
            int poolSize;      //holds the user desired pool size
            int numOfMatrices;         //holds the user desired number of matrices
            int matrixDim;           //holds the user desired matrix dimension

            while (true)
            {
                getUserInput(out poolSize, out numOfMatrices, out matrixDim); //Gets input from the user
                RandomMatrix resMatrix = CreateAndMultiplayMatrices(poolSize, numOfMatrices, matrixDim); //Creates random matricies and then multiplies them with one another, return the final result matrix at the end.
                MatrixUtils.PrintMatrix(resMatrix);
            }
        }

        //This method fetches input from the users
        private void getUserInput(out int poolSize, out int numOfMatrices, out int matrixDim)
        {
           
            Console.WriteLine("Please enter the number of threads (must be between 2 and 20): ");
            poolSize = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Please enter the quantity of matrices (must be at least 2): ");
            numOfMatrices = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Please enter matrix dimension (must be at least 1000): ");
            matrixDim = Convert.ToInt32(Console.ReadLine());          
        }

        //This function first creates the designated amount of matrices with randomly generated numbers in them and then multiplies the matrices with one another using threads to multiply different sections of the matrix simultaneously
        private RandomMatrix CreateAndMultiplayMatrices(int poolSize,int numOfMatrices,int matrixDim)
        {
            
            CustomThreadPool threadPool = new CustomThreadPool(); //Creates a thread pool
            List<RandomMatrix> matrices = new List<RandomMatrix>(); //Creates a vector of matrices
            MatrixConcurrentQueue multiMatrixQueue = new MatrixConcurrentQueue(); //A custom Queue class that holds the matrices and the amount of multiplications that have been completed

            threadPool.SetThreadPoolSize(poolSize); //Set the thread pool's pool size (amount of threads)

            //Initializes the matrix array with matrices
            for (int i = 0; i < numOfMatrices; i++) 
            {
                //Initializes the current matrix
                RandomMatrix currMatrix = new RandomMatrix(matrixDim);
                matrices.Add(currMatrix);
                for (int j = 0; j < poolSize; j++)
                {
                    //Splits the matrix into segments so each thread will randomize values for a different segment of the matrix
                    int threadRandPart = j;
                    WaitCallback task = () => MatrixUtils.ThreadingCreateRandomMatrix(currMatrix, poolSize, threadRandPart, matrixDim); //Saves the task into a callback
                    threadPool.QueueTask(task); //Queues the task so the thread pool will allocate a thread to run it
                }
            }

           threadPool.DoneWorking.WaitOne(); //Waits for all threads to finish before continuing

            //Enqueues all the matrices into a Concurrent Queue so it can run constantly until all the matrices have been multiplied and only the final result matrix remains
           foreach (RandomMatrix mat in matrices)
           {
               multiMatrixQueue.m_MatrixQueue.Enqueue(mat);
           }

            //Runs across the queue, sending 2 matrices at a time to the multiplication method
           while (multiMatrixQueue.numOfOperations < numOfMatrices - 1)
            {
                    RandomMatrix matA = null; //matrix that will hold the 1st matrix
                    RandomMatrix matB = null; //matrix that will hold the 2nd matrix

                    if (multiMatrixQueue.m_MatrixQueue.Count > 1) //checks if theres more than one matrix in the queue, so it won't attempt to multiply a null matrix
                    {
                        multiMatrixQueue.m_MatrixQueue.TryDequeue(out matA); //Tries to dequeues the 1st matrix into matA
                        multiMatrixQueue.m_MatrixQueue.TryDequeue(out matB); ////Tries to dequeues the 2nd matrix into matA
                    }

                    if (matA != null && matB != null)
                    {
                        RandomMatrix resMatrix = new RandomMatrix(matrixDim); //Creates a result matrix to hold the multiplication result
                        multiMatrixQueue.m_MatrixQueue.Enqueue(resMatrix); //Insers the result matrix into the queue
                        int matrixPartsLeft = poolSize; //Amount of parts the matrix multiplication will be split into

                        //Divides the matrix multiplication work, so each thread calculates the matrix multiplication in different parts of the matrices
                        for (int i = 0; i < poolSize; i++)
                        {
                            int threadMultiPart = i;                   //saves the index in order to avoid it being different when the task is executed
                            WaitCallback task = () => MatrixUtils.ThreadingMultiplyMatrices(resMatrix, matA, matB, multiMatrixQueue, ref matrixPartsLeft, poolSize, threadMultiPart, matrixDim);
                            threadPool.QueueTask(task);
                        }
                       // threadPool.DoneWorking.WaitOne();
                    }
            }

            threadPool.DoneWorking.WaitOne();  //Waits for all threads to finish before continuing
            RandomMatrix finalMatrix;
            multiMatrixQueue.m_MatrixQueue.TryDequeue(out finalMatrix); //Dequeues the final result matrix and returns it.
            return (finalMatrix);
        }

    }
}