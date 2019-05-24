using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    class DFAState
    {
        public static int alphabetNumber;
        public int[] adjList;//همسایگان:)
        public List<int> content = new List<int>(); ///محتویات استیت تبدیل یافته        
        public bool isFinal;
        public DFAState()
        {
            adjList = new int[alphabetNumber];         
        }
    }
}
