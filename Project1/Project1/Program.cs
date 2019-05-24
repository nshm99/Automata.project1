using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    class Program
    {
        static void Main(string[] args)
        {
            //---------------------------------------------------------------------------------------------------------------------------//
            //read Input => phase 0

            string[] lines = File.ReadAllLines("../../input.txt");
            int stateNumber = int.Parse(lines[0]);
            string[] alphabet = lines[1].Split(new char[] { ',' }).ToArray();
            NFAState.alphabetNumber = alphabet.Length;
            NFAState[] NFA = new NFAState[stateNumber];
            int initaialIndex = 0;
            List<int> nfaFinalState = new List<int>();
            char alpha = ' ';

            for (int i = 0; i < NFA.Length; i++)
                NFA[i] = new NFAState();

            for (int i = 2; i < lines.Length; i++)
            {
                char[] input = lines[i].ToArray();

                //->q0,a,q1
                if(input[3] == '_')
                {
                    NFA[int.Parse(input[1].ToString())].adjList[alphabet.Length].Add(int.Parse(input[6].ToString()));
                }
                else if (input[0] == '-' && input[1] == '>')//initial state
                {
                    alpha = input[2];
                    initaialIndex = int.Parse(input[3].ToString());
                    NFA[int.Parse(input[3].ToString())].adjList[input[5] - 97].Add(int.Parse(input[8].ToString()));
                }
                //*q2,a,q0
                else if(input[0] == '*')
                {
                    nfaFinalState.Add(int.Parse(input[2].ToString()));
                    NFA[int.Parse(input[2].ToString())].adjList[input[4] - 97].Add(int.Parse(input[7].ToString()));
                }
                //q2,a,*q0
                else if (input[5] == '*')
                {
                    NFA[int.Parse(input[1].ToString())].adjList[input[3]- 97].Add(int.Parse(input[7].ToString()));
                    nfaFinalState.Add(int.Parse(input[7].ToString()));
                }
                //*q1,a,*q8
                else if(input[0] == '*' && input[6] == '*')
                {
                    NFA[int.Parse(input[2].ToString())].adjList[input[4] - 97].Add(int.Parse(input[8].ToString()));
                }
                //q2,a,q0
                else
                {
                    NFA[int.Parse(input[1].ToString())].adjList[input[3]- 97].Add(int.Parse(input[6].ToString()));

                }
            }

            //-------------------------------------------------------------------------------------------------------//

            //convert NFA to DFA => phase 1
            DFAState.alphabetNumber = alphabet.Length;

            List<DFAState> DFA = new List<DFAState>();
            List<DFAState> closure = new List<DFAState>();

            for (int i = 0; i < NFA.Length; i++)
            {
                closure.Add(new DFAState());
                Closure(closure, NFA,alphabet.Length,i);
            }

            DFA.Add(new DFAState());
            DFA[0] = closure[initaialIndex];

            Queue<int> statesToBeProcess = new Queue<int>();
            statesToBeProcess.Enqueue(0);

            while(statesToBeProcess.Count != 0)
            {
                int currentNode = statesToBeProcess.Dequeue();
                for(int i = 0; i < alphabet.Length; i++)
                {
                    FindAdjByAlphabet(DFA, NFA, closure, i, statesToBeProcess, currentNode);
                }
            }

            List<int> dfaFinalSrate = new List<int>();
            SetDFAFinalState(DFA, nfaFinalState, dfaFinalSrate);
            Print(DFA, alpha, dfaFinalSrate);
            
            //-------------------------------------------------------------------------------------------------------//
            
            //minimize DFA => phase 2
            int[] DFAParents = new int[DFA.Count];
            List<List<int>> prevPartitions = new List<List<int>>();
            for (int i = 0; i < DFAParents.Length; i++)
                //با فرض این که شماره راس ها از 0 باشه و هر راس در ایندکس اسم خودش باشد
                DFAParents[i] = i;

            prevPartitions = PartitionFinal(DFA, DFAParents);
            int prevCount = -1;
            int curCount = -1;
            int k = 0;
            k++;

            do
            {
                prevCount = prevPartitions.Count();
                prevPartitions = kOrderEquivalant(k, prevPartitions, DFAParents, DFA);
                curCount = prevPartitions.Count();
            } while (prevCount != curCount);

        }
        //-----------------------------------------------------------------------------------------------------------------//
        //method => phase0 + phase1

        private static void Print(List<DFAState> DFA, char alphabet, List<int> dfaFinalSrate)
        {
            Console.WriteLine(DFA.Count);
       
            for(int i = 0; i < DFA.Count; i++)
            {
                for(char j = 'a'; j - 97 < DFA[i].adjList.Length; j++)
                {
                    if (i == 0 && j - 97 == 0)
                        Console.Write("->");  
                    if (dfaFinalSrate.Contains(i))
                        Console.Write("*");
                    Console.Write($"{alphabet}{i},{j},");
                    if (dfaFinalSrate.Contains(DFA[i].adjList[j - 97]))
                        Console.Write("*");
                    Console.Write($"{alphabet}{DFA[i].adjList[j - 97]}");

                    Console.WriteLine("");
                }
            }
        }

        private static void SetDFAFinalState(List<DFAState> DFA, List<int> nfaFinalState, List<int> dfaFinalSrate)
        {
            for(int i = 0; i < DFA.Count; i++)
            {
                for (int j = 0; j < DFA[i].content.Count; j++)
                    if (nfaFinalState.Contains(DFA[i].content[j]))
                    {
                        dfaFinalSrate.Add(i);
                        DFA[i].isFinal = true;
                    }
            }
        }

        private static void FindAdjByAlphabet(List<DFAState> dFA, NFAState[] nFA, List<DFAState> closure,
            int alphabet, Queue<int> statesToBeProcess, int currentNode)
        {
            List<int> newState = new List<int>();

            for(int k = 0; k < dFA[currentNode].content.Count; k++)
            {
                int node = dFA[currentNode].content[k];
                for (int j = 0; j < closure[node].content.Count; j++)
                {
                    int nodeToProccess = closure[node].content[j];

                    for (int i = 0; i < nFA[nodeToProccess].adjList[alphabet].Count; i++)
                        if (!newState.Contains(nFA[nodeToProccess].adjList[alphabet][i]))
                            newState.Add(nFA[nodeToProccess].adjList[alphabet][i]);
                }
            }

            
            if (!CheckForExist(dFA, newState, currentNode, alphabet))// false => state jadid
            {
                dFA.Add(new DFAState());
                dFA[dFA.Count - 1].content = newState;
                statesToBeProcess.Enqueue(dFA.Count - 1);
                dFA[currentNode].adjList[alphabet] = dFA.Count - 1;
            }
        }

        private static bool CheckForExist(List<DFAState> DFA, List<int> newState, int currentNode, int alphabet)
        {
            for(int i = 0; i < DFA.Count; i++)
            {
                int count = 0;

                for (int j = 0; j < newState.Count; j++)
                    if (DFA[i].content.Contains(newState[j]))
                        count++;

                if (count == DFA[i].content.Count && count == newState.Count)
                {
                    DFA[currentNode].adjList[alphabet] = i;
                    return true;
                }
            }
            return false ;
        }

        private static void Closure(List<DFAState> DFA, NFAState[] NFA,int alphaCount,int index)
        {
            DFA[index].content.Add(index);
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(index);
            while(queue.Count != 0)
            {
                int currentNode = queue.Dequeue();
                for (int i = 0; i < NFA[currentNode].adjList[alphaCount].Count; i++)
                {
                    if (!DFA[index].content.Contains(NFA[currentNode].adjList[alphaCount][i]))
                        DFA[index].content.Add(NFA[currentNode].adjList[alphaCount][i]);
                }
            }
        }


        //-------------------------------------------------------------------------------------------------------//
        //methods of phase2

        private static List<List<int>> kOrderEquivalant(int k, List<List<int>> prevPartitions, int[] dFAParents, List<DFAState> dFA)
        {
            List<List<int>> result = new List<List<int>>();
            List<List<int>> newPartition = new List<List<int>>();
            bool newpart = false;
            for (int i = 0; i < prevPartitions.Count; i++)
            {
                newpart = false;
                for (int j = 0; j < prevPartitions[i].Count; j++)
                {
                    for (int p = 0; p < prevPartitions[i].Count; p++)
                    {
                        if (j != p)
                            if (IsDistinguishable(prevPartitions[i][j], prevPartitions[i][p], k, dFA, dFAParents))
                            {
                                dFAParents[prevPartitions[i][j]] = prevPartitions[i][j];
                                dFAParents[prevPartitions[i][p]] = prevPartitions[i][p];
                                newpart = true;
                                break;
                            }
                        /*else
                        {
                            dFAParents[prevPartitions[i][p]] = dFAParents[prevPartitions[i][j]];
                        }
                        */
                        if (newpart)
                            break;
                    }
                }
                if (newpart)
                {
                    newPartition = MakeNewPartion(dFAParents, prevPartitions[i], dFA);
                    for (int l = 0; l < newPartition.Count(); l++)
                        result.Add(newPartition[l]);
                }
                else
                {
                    result.Add(prevPartitions[i]);
                }
            }
            return result;
        }

        private static List<List<int>> MakeNewPartion(int[] dFAParents, List<int> prevPartion, List<DFAState> dFA)
        {
            List<List<int>> result = new List<List<int>>();
            List<int> firstPartition = new List<int>();
            List<int> secondPartition = new List<int>(prevPartion);
            firstPartition.Add(prevPartion[0]);
            secondPartition.Remove(prevPartion[0]);
            for (int i = 1; i < prevPartion.Count(); i++)
            {
                if (!IsDistinguishable(prevPartion[0], prevPartion[i], 0, dFA, dFAParents))
                {
                    firstPartition.Add(prevPartion[i]);
                    secondPartition.Remove(prevPartion[i]);
                }

            }
            result.Add(firstPartition);
            result.Add(secondPartition);
            return result;
        }

        private static List<int> FindEquivalants(int i, int[] dFAParents, List<int> prevPartion)
        {
            List<int> result = new List<int>();
            result.Add(i);
            int parent = dFAParents[i];
            for (int j = 0; j < prevPartion.Count(); j++)
            {
                if (i != j)
                    if (dFAParents[j] == parent)
                        result.Add(j);
            }
            return result;
        }

        private static bool IsDistinguishable(int firstState, int secondState, int k, List<DFAState> dFA, int[] dFAParents)
        {
            int firstIndex;
            int secondIndex;
            for (int i = 0; i < dFA[firstState].adjList.Count(); i++)
            {
                firstIndex = dFA[firstState].adjList[i];
                secondIndex = dFA[secondState].adjList[i];
                if (dFAParents[firstIndex] != dFAParents[secondIndex])
                    return true;
            }
            return false;
        }

        private static List<List<int>> PartitionFinal(List<DFAState> dFA, int[] dFAParents)
        {
            int finalStateParent = -1;
            int nonFinalStateParent = -1;
            List<int> finals = new List<int>();
            List<int> nonFinals = new List<int>();
            for (int i = 0; i < dFA.Count; i++)
            {
                if (dFA[i].isFinal)
                {
                    if (finalStateParent == -1)
                    {
                        finalStateParent = i;
                    }
                    dFAParents[i] = finalStateParent;
                    finals.Add(i);
                }
                else
                {
                    if (nonFinalStateParent == -1)
                    {
                        nonFinalStateParent = i;
                    }
                    dFAParents[i] = nonFinalStateParent;
                    nonFinals.Add(i);
                }
            }
            List<List<int>> result = new List<List<int>>();
            result.Add(finals);
            result.Add(nonFinals);
            return result;

        }
        //-------------------------------------------------------------------------------------------------------//
    }
}
