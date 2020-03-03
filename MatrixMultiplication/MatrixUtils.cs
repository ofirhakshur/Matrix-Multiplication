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
    //This class contains matrix utility methods that can be used on matrix classes
    public static class MatrixUtils
    {
        //This method mutiplies matrices directly without using thre

        public static void ThreadingCreateRandomMatrix(RandomMatrix result, int numOfThreads, int threadPart, int matrixDim)
        {
            // Calculates the workload for the current thread
            int numOfElements = (matrixDim * matrixDim);         //Holds the number of elements in the matrix
            int numOfOperations = matrixDim / numOfThreads;         //Holds the number of operations (iterations) that will be done
            int restOfOperations = matrixDim % numOfThreads;        //Holds the extra number of operations the first thread will have to do
            int startPosition, endPosition;

            if (threadPart == 0)
            {
                //First thread does the extra work left by the other threads (the remainder)
                startPosition = numOfOperations * threadPart;
                endPosition = (numOfOperations * (threadPart + 1)) + restOfOperations;
            }
            else
            {
                startPosition = numOfOperations * threadPart + restOfOperations;
                endPosition = (numOfOperations * (threadPart + 1)) + restOfOperations;
            }

            ThreadSafeRandom rand = new ThreadSafeRandom(); //Creates a ThreadSafe random number generator for each thread

            //Randomizes the segment in the matrix the current thread is incharge of, in this case a segment of rows in the matrix
            for (int row = startPosition; row < endPosition; row++)
            {
                for (int col = 0; col < matrixDim; col++)
                {
                    result.Matrix[row, col] = rand.GetRandomNumber();
                }
            }
        }

      public static void  ThreadingMultiplyMatrices(RandomMatrix result, RandomMatrix matA, RandomMatrix matB,MatrixConcurrentQueue matrixQueue,ref int matrixPartsLeft, int numOfThreads, int threadPart,int matrixDim)
        {
            // Calculates the workload for the current thread
            int numOfElements = (matrixDim * matrixDim);               //Holds the number of elements in the matrix
            int numOfOperations = numOfElements / numOfThreads;     //Holds the number of operations (iterations) that will be done
            int restOfOperations = numOfElements % numOfThreads;  //Holds the extra number of operations the first thread will have to do
            int startPosition, endPosition;                    //Holds the starting and ending indexes

            if (threadPart == 0)
            {
                //First thread does the extra work left by the other threads (the remainder)
                startPosition = numOfOperations * threadPart;
                endPosition = (numOfOperations * (threadPart + 1)) + restOfOperations;
            }
            else
            {
                startPosition = numOfOperations * threadPart + restOfOperations;
                endPosition = (numOfOperations * (threadPart + 1)) + restOfOperations;
            }
            //Randomizes the segment in the matrix the current thread is incharge of, in this case a segment of cells in the result matrix that will be calculated from multiplying Matrix A and Matrix B
            for (int pos = startPosition; pos < endPosition; ++pos)
            {
                int row = pos % matrixDim; //Calcuates the current row
                int col = pos / matrixDim; //Calculates the current column
                int resCell = 0;

                //Multiplies the current row in Matrix A with the current column in matrix B and saves the result in the appropriate cell in the result matrix
                for (int i = 0; i < matrixDim; ++i)
                { 
                    int cellA = matA.Matrix[row, i];
                    int cellB = matB.Matrix[i, col];
                    resCell += cellA * cellB;
                }

                result.Matrix[row, col] = resCell;
            }

          //The current segment in the result matrix has been completed so the matrixPartsLeft counter gets decremented
            Interlocked.Decrement(ref matrixPartsLeft);
          //If the matrixPartsLeft counter reaches 0, we have finished calculating the multiplication of all the segments of the result matrix
          if(matrixPartsLeft ==0)
          {
              //Since the result matrix is complete we increment the number of completed matrix multiplication opreations in the matrix multiplication queue
              Interlocked.Increment(ref matrixQueue.numOfOperations);
          }
        }

        //This method prints the matrix sent to it as a parameter
      public static void PrintMatrix(RandomMatrix matrixToPrint)
      {
          for (int i = 0; i < matrixToPrint.MatrixDim; i++)
          {
              for (int j = 0; j < matrixToPrint.MatrixDim; j++)
              {
                  Console.Write("|{0}| ", matrixToPrint.Matrix[i, j]);
              }
              Console.WriteLine("\n");
          }
      }
    }
}
