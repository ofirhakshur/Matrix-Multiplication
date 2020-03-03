using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace MatrixMultiplication
{
    //This class holds a matrix that will be randomly generated,the matrix dimension and
    public class RandomMatrix
    {
        private int m_MatrixDim;
       private  int[,] m_RandomMatrix;

        public RandomMatrix(int i_dim) //RandomMatrix constructor
        {
            m_MatrixDim = i_dim;
            m_RandomMatrix = new int[i_dim, i_dim];
        }

        public int MatrixDim  //m_MatrixDim Property
        {
            get
            {
                return m_MatrixDim;
            }
        }

        public int[,] Matrix //m_randomMatrix property
        {
            get
            {
                return m_RandomMatrix;
            }
            set
            {
                m_RandomMatrix = value;
            }
        }
    }
}
