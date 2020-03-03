using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace MatrixMultiplication
{
    //This custom queue is used to hold all matrices awaiting multiplication while keeping a count on how many multiplications have been completed so far
   public class MatrixConcurrentQueue
    {
        public ConcurrentQueue<RandomMatrix> m_MatrixQueue;
        public int numOfOperations;

        public MatrixConcurrentQueue()
        {
            numOfOperations = 0;
            m_MatrixQueue = new ConcurrentQueue<RandomMatrix>();
        }           
    }
}
