using System.Net;

namespace Gomory_HuCalculator.GomoryHu;

public class GomoryHuAlgorithm
{
    private int [,] _weightMatrix;
    private bool[] _markedNodes;
    private List<List<int>> _treeNodes;
    private List<List<List<int>>> _treePaths;
    
    public GomoryHuAlgorithm(int[,] weightMatrix)
    {
        var nodeCount = weightMatrix.GetLength(0);
        
        _weightMatrix = weightMatrix;
        _markedNodes = new bool[nodeCount];

        _treeNodes = [];
        var tempList = new List<int>();
        for (var i = 0; i < nodeCount; i++)
        {
            tempList.Add(i);
        }
        _treeNodes.Add(tempList);

        _treePaths = [];
    }
    
    public void Solve()
    {
        while (true)
        {
           var stop = _treeNodes.All(sublist => sublist.Count == 1);
           if (stop)
               break;

           var sublist = _treeNodes[0].ToList();
           var sublistIndex = 0;
           for (var i = 1; i < _treeNodes.Count; i++)
           {
               if (_treeNodes[i].Count >= 2 && _treeNodes[i].Count <= sublist.Count)
               {
                   sublist = _treeNodes[i].ToList();
                   sublistIndex = i;
               }
           }
           
           ChooseElementsToCut(sublist, out var s, out var t);
           PerformCut(sublist, s, t, out var a, out var b, out var sum);
           
           UpdateTree(sublistIndex, a, b, sum);
        }

    }
    
    private void PerformCut(List<int> initialNodeList, int s, int t, out List<int> a, out List<int> b, out int sum)
    {
        var allNodeCount = _weightMatrix.GetLength(0);
        
        var minFlowValue = int.MaxValue;
        List <int> minNodeList = [];
        var nodeList = new List<int>{s};
        var tempNodeList = nodeList; // This list hold initial node list before ChangeNodeForCut
        var last = false;
        do
        {
            var currentFlowValue = CalculateFlowValue(nodeList, allNodeCount);
            if (currentFlowValue < minFlowValue)
            {
                minFlowValue = currentFlowValue;
                minNodeList = nodeList.ToList();
            }

            if (ChangeNodeForCut(nodeList, allNodeCount, t) == -1)
            {
                last = AddNodeForCut(ref nodeList, allNodeCount, t, ref tempNodeList);
            }
        } while(nodeList.Count <= allNodeCount - 1 && !last);

        if (minNodeList.Count == allNodeCount - 1)
        {
            minNodeList.Clear();
            minNodeList.Add(t);
        }

        a = minNodeList;
        b = [];
        sum = minFlowValue;
        
        var initialNodeListEnumerator = initialNodeList.GetEnumerator();
        while (initialNodeListEnumerator.MoveNext())
        {
            if(!a.Contains(initialNodeListEnumerator.Current))
                b.Add(initialNodeListEnumerator.Current);
        }
    }
    
    private void UpdateTree(int sublistIndex, List<int> a, List<int> b, int sum)
    {
        _treeNodes.RemoveAt(sublistIndex);
        _treeNodes.Insert(sublistIndex, a);
        _treeNodes.Insert(sublistIndex, b);
        
        _treePaths.Insert(sublistIndex, new List<List<int>>{});
    }
    
    // Utility functions
    private static void ChooseElementsToCut(List<int> nodes, out int s, out int t)
    {
        s = nodes.First();
        t = nodes.Last();
    }
    
    private int CalculateFlowValue(List<int> nodeList, int allNodeCount)
    {
        var sum = 0;
        var nodeListEnumerator = nodeList.GetEnumerator();
        while (nodeListEnumerator.MoveNext())
        {
            for (var j = 0; j < allNodeCount; j++)
            {
                if (nodeList.Contains(j))
                    continue;

                sum += _weightMatrix[nodeListEnumerator.Current, j];
            }
        }

        return sum;
    }

    // private static int ChangeNodeForCut(List<int> nodeList, int allNodeCount, int t)
    // {
    //     if (nodeList.Count < 2) return allNodeCount;
    //
    //     for (var i = nodeList.Count - 1; i > 0; --i)
    //     {
    //         if (nodeList[i] == allNodeCount - 1 || nodeList[i] + 1 == t)
    //         {
    //             if (i == 1)
    //                 return -1;
    //
    //             nodeList[i] = 0;
    //         }
    //         else
    //         {
    //             return ++nodeList[i];
    //         }
    //     }
    //
    //     return -1;
    // }
    
    private static int ChangeNodeForCut(List<int> nodeList, int allNodeCount, int t)
    {
        if (nodeList.Count < 2) return -1;

        // Iterate from the last element to the first (excluding the first element at index 0)
        for (var i = nodeList.Count - 1; i > 0; --i)
        {
            // Try to increment the current node's value
            while (++nodeList[i] < allNodeCount)
            {
                // Ensure the new value is not equal to `t` and is not already in the list
                if (nodeList[i] != t && !nodeList.Take(i).Contains(nodeList[i]))
                {
                    // Reset all elements to the right of `i` to their initial valid minimum values
                    for (var j = i + 1; j < nodeList.Count; j++)
                    {
                        nodeList[j] = 0; // Start with 0 to find the next valid increment
                        while (nodeList[j] < allNodeCount)
                        {
                            if (nodeList[j] != t && !nodeList.Take(j).Contains(nodeList[j]))
                                break;
                            nodeList[j]++;
                        }

                        // If no valid value was found for nodeList[j], return -1
                        if (nodeList[j] >= allNodeCount) return -1;
                    }

                    return nodeList[i]; // Return the updated value at `i` to indicate a successful change
                }
            }
            
            // Reset the current element to 0 when it can't be incremented anymore
            nodeList[i] = 0;
        }

        // If all combinations have been exhausted, return -1
        return -1;
    }

    
    private static bool AddNodeForCut(ref List<int> nodeList, int allNodeCount, int t, ref List<int> tempNodeList)
    {
        nodeList = tempNodeList.ToList();
        
        if (nodeList.Count == allNodeCount - 1)
            return true;
        
        for (var i = 0; i < allNodeCount; ++i)
        {
            if (nodeList.Contains(i) || i == t) continue;
            nodeList.Add(i);
            tempNodeList = nodeList.ToList();
            break;
        }
        
        return false;
    }
    
}