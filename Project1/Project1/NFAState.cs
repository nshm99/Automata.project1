using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    class NFAState
    {
        public List<int>[] adjList;
        public static int alphabetNumber;

        public NFAState()
        {
            adjList = new List<int>[alphabetNumber + 1];
            for (int i = 0; i < adjList.Length; i++)
                adjList[i] = new List<int>();
            
        }
    }
}
