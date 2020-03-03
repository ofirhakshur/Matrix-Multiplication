using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplication
{
    //This class is an implementaion of a thread safe random number generator
   public class ThreadSafeRandom
    {
        private static object locker = new object(); //seed generator lock
        private static Random seedGenerator = new Random(Environment.TickCount); //seed generator

        public int GetRandomNumber()
        {
            int seed;

            lock (locker)
            {
                seed = seedGenerator.Next(int.MinValue, int.MaxValue); //generates a new seed everytime the method is executed
            }

            var random = new Random(seed);

            return random.Next(0,10); //randomizes a number ranging 1-10 and returns it
        }

    }
}
