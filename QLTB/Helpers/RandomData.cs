using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.Helpers
{
    public class RandomData
    {
        public static int RandomNumber(int min, int max)
        {
            Random rd = new Random();
           return rd.Next(min, max);
        }
    }
}
